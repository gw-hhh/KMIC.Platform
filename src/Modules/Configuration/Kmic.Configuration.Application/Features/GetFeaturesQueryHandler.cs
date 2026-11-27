using Kmic.BuildingBlocks.Application.Features;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application.Features;

public sealed class GetFeaturesQueryHandler(IFeatureFlags features) : IQueryHandler<GetFeaturesQuery, FeaturesResponse>
{
    public async ValueTask<Result<FeaturesResponse>> Handle(GetFeaturesQuery query, CancellationToken cancellationToken)
    {
        return new FeaturesResponse(await features.EnabledAsync(cancellationToken));
    }
}
