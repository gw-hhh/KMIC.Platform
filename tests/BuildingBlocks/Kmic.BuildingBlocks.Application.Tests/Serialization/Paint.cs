namespace Kmic.BuildingBlocks.Application.Tests.Serialization;

internal sealed record Paint(string Name, Shade Shade, PaintFinish Finish, DateTimeOffset MixedAt, int Coats);
