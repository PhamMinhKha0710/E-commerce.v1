using E_commerce.v1.Application.DTOs.Shipping;
using E_commerce.v1.Application.Interfaces;
using E_commerce.v1.Application.Shipping;
using MediatR;
using Microsoft.Extensions.Options;

namespace E_commerce.v1.Application.Features.Shipping.Queries.GetShippingFee;

public class GetShippingFeeQueryHandler : IRequestHandler<GetShippingFeeQuery, ShippingFeeResponse>
{
    private readonly IAhamoveClient _ahamoveClient;
    private readonly AhamoveOptions _options;

    public GetShippingFeeQueryHandler(IAhamoveClient ahamoveClient, IOptions<AhamoveOptions> options)
    {
        _ahamoveClient = ahamoveClient;
        _options = options.Value;
    }

    public async Task<ShippingFeeResponse> Handle(GetShippingFeeQuery request, CancellationToken cancellationToken)
    {
        var body = request.Body;
        var path = body.Path.ToList();
        if (path.Count == 1)
        {
            var p = _options.Pickup;
            path.Insert(0, new AhamovePathPoint
            {
                Lat = p.Lat,
                Lng = p.Lng,
                Address = p.Address,
                Name = p.Name,
                Mobile = p.Mobile
            });
        }

        var estimateRequest = new AhamoveEstimateRequest
        {
            OrderTime = body.OrderTime,
            Path = path,
            Services = body.Services
                .Select(s => new AhamoveEstimateService { Id = s })
                .ToList(),
            Items = body.Items,
            RouteOptimized = body.RouteOptimized
        };

        var results = await _ahamoveClient.EstimateAsync(estimateRequest, cancellationToken);

        var response = new ShippingFeeResponse();
        foreach (var item in results)
        {
            if (item.Data == null)
                continue;

            response.Estimates.Add(new ShippingFeeEstimateDto
            {
                ServiceId = item.ServiceId,
                TotalFee = item.Data.TotalFee,
                Distance = item.Data.Distance,
                Duration = item.Data.Duration,
                DistanceFee = item.Data.DistanceFee,
                RequestFee = item.Data.RequestFee
            });
        }

        return response;
    }
}
