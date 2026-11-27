using Kmic.BuildingBlocks.Domain.Results;
using Shouldly;

namespace Kmic.BuildingBlocks.Testing.Assertions;

/// <summary>Result 断言扩展。</summary>
public static class ResultAssertions
{
    public static void ShouldBeSuccess(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsFailure)
        {
            throw new ShouldAssertException($"期望成功，实际失败：{Describe(result.Error)}");
        }
    }

    public static T ShouldBeSuccess<T>(this Result<T> result)
    {
        ((Result)result).ShouldBeSuccess();
        return result.Value;
    }

    /// <summary>断言失败且错误码与错误类型均匹配，返回实际错误。</summary>
    public static Error ShouldBeFailure(this Result result, Error expected)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(expected);
        if (result.IsSuccess)
        {
            throw new ShouldAssertException($"期望失败（{Describe(expected)}），实际成功。");
        }

        if (!string.Equals(result.Error.Code, expected.Code, StringComparison.Ordinal) || result.Error.Type != expected.Type)
        {
            throw new ShouldAssertException($"期望失败（{Describe(expected)}），实际为 {Describe(result.Error)}");
        }

        return result.Error;
    }

    /// <summary>断言失败且错误类型匹配（不校验错误码），返回实际错误。</summary>
    public static Error ShouldBeFailure(this Result result, ErrorType expectedType)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsSuccess || result.Error.Type != expectedType)
        {
            var actual = result.IsSuccess ? "成功" : Describe(result.Error);
            throw new ShouldAssertException($"期望 {expectedType} 类型的失败，实际为 {actual}");
        }

        return result.Error;
    }

    private static string Describe(Error error) => $"{error.Code}（{error.Type}）{error.Message}";
}
