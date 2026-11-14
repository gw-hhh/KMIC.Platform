using Kmic.BuildingBlocks.Application.Features;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application.Features;

/// <summary>
/// 处理 <see cref="GetFeaturesQuery"/>
/// </summary>
public sealed class GetFeaturesQueryHandler(IFeatureFlags features) : IQueryHandler<GetFeaturesQuery, FeaturesResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<FeaturesResponse>> Handle(GetFeaturesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return new FeaturesResponse(await features.EnabledAsync(cancellationToken));
    }
}
