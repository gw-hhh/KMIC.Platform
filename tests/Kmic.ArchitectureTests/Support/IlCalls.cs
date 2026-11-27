using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Kmic.ArchitectureTests.Support;

// 解析方法体 IL 指令（含 async 与 lambda 编译器嵌套类型），提取调用的方法及字符串常量
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

    public static IEnumerable<MethodBase> CalledBy(Type type) => Operands(type).OfType<MethodBase>();

    // 包含插值字符串的固定常量段
    public static IEnumerable<string> StringsUsedBy(Type type) => Operands(type).OfType<string>();

    private static IEnumerable<object> Operands(Type type)
    {
        foreach (var method in type.GetMethods(AllMembers).Cast<MethodBase>().Concat(type.GetConstructors(AllMembers)))
        {
            foreach (var operand in Operands(method))
            {
                yield return operand;
            }
        }

        foreach (var nested in type.GetNestedTypes(AllMembers).Where(n => n.GetCustomAttribute<CompilerGeneratedAttribute>() is not null))
        {
            foreach (var operand in Operands(nested))
            {
                yield return operand;
            }
        }
    }

    private static IEnumerable<object> Operands(MethodBase method)
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

            if (code.OperandType == OperandType.InlineMethod
                && method.Module.ResolveMethod(BitConverter.ToInt32(il, position), typeArguments, methodArguments) is { } called)
            {
                yield return called;
            }
            else if (code.OperandType == OperandType.InlineString)
            {
                yield return method.Module.ResolveString(BitConverter.ToInt32(il, position));
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
