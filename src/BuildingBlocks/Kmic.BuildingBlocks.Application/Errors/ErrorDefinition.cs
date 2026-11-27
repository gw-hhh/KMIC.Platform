using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Errors;

public sealed record ErrorDefinition(string Code, ErrorType Type, string Message);
