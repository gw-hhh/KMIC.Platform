namespace Spike06.Tests;

public static class KmicFunctions
{
    // 05-13 第 3.5 节：时刻换算为业务时区的日期，只用于 GroupBy / Select
    public static DateOnly ToBusinessDate(DateTimeOffset value) =>
        throw new InvalidOperationException("KmicFunctions can only be used in database queries.");
}
