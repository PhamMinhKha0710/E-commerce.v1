using E_commerce.v1.Application.Common.Shipping;
using E_commerce.v1.Domain.Enums;

namespace E_commerce.v1.Application.Tests;

public class AhamoveStatusMapperTests
{
    [Theory]
    [InlineData("COMPLETED", OrderStatus.Completed)]
    [InlineData("CANCELLED", OrderStatus.Cancelled)]
    [InlineData("FAILED", OrderStatus.Cancelled)]
    [InlineData("ASSIGNING", OrderStatus.Confirmed)]
    [InlineData("DELIVERING", OrderStatus.Delivering)]
    [InlineData("PICKED_UP", OrderStatus.Delivering)]
    public void should_map_known_status(string input, OrderStatus expected)
    {
        var mapped = AhamoveStatusMapper.TryMapToOrderStatus(input);
        Assert.Equal(expected, mapped);
    }

    [Fact]
    public void should_return_null_for_unknown_status()
    {
        var mapped = AhamoveStatusMapper.TryMapToOrderStatus("UNKNOWN_STATUS_XYZ");
        Assert.Null(mapped);
    }
}
