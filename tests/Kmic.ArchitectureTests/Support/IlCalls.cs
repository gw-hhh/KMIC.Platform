using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Kmic.ArchitectureTests.Support;

// 解析方法体 IL 指令（含 async 与 lambda 生成的嵌套类型）提取方法调用
internal static class IlCalls
{
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly OpCode[] _singleByte = new OpCode[256];
    private static readonly OpCode[] _twoByte = new OpCode[256];

    static IlCalls()
    {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var code = (OpCode)field.GetValue(null)!;
            var value = (ushort)code.Value;
            if (value < 0x100)
            {
                _singleByte[value] = code;
            }
            else if ((value & 0xff00) == 0xfe00)
            {
                _twoByte[value & 0xff] = code;
            }
        }
    }

    public static IEnumerable<MethodBase> CalledBy(Type type)
    {
        foreach (var method in type.GetMethods(AllMembers).Cast<MethodBase>().Concat(type.GetConstructors(AllMembers)))
        {
            foreach (var called in Calls(method))
            {
                yield return called;
            }
        }

        foreach (var nested in type.GetNestedTypes(AllMembers).Where(n => n.GetCustomAttribute<CompilerGeneratedAttribute>() is not null))
        {
            foreach (var called in CalledBy(nested))
            {
                yield return called;
            }
        }
    }

    private static IEnumerable<MethodBase> Calls(MethodBase method)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
        {
            yield break;
        }

        var typeArguments = method.DeclaringType is { IsGenericType: true } declaring ? declaring.GetGenericArguments() : null;
        var methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : null;
        for (var position = 0; position < il.Length;)
        {
            OpCode code;
            if (il[position] == 0xfe)
            {
                code = _twoByte[il[position + 1]];
                position += 2;
            }
            else
            {
                code = _singleByte[il[position]];
                position++;
            }

            if (code.OperandType == OperandType.InlineMethod)
            {
                var called = method.Module.ResolveMethod(BitConverter.ToInt32(il, position), typeArguments, methodArguments);
                if (called is not null)
                {
                    yield return called;
                }
            }

            position += OperandSize(code, il, position);
        }
    }

    private static int OperandSize(OpCode code, byte[] il, int position) => code.OperandType switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        OperandType.InlineSwitch => 4 + (4 * BitConverter.ToInt32(il, position)),
        _ => 4,
    };
}
