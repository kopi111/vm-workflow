using VMWorkflow.Domain.Services;
using Xunit;

namespace VMWorkflow.Tests;

public class GatewayCalculatorTests
{
    [Theory]
    [InlineData("172.0.0.1", "255.255.255.0", "172.0.0.254")]
    [InlineData("192.168.1.100", "255.255.255.0", "192.168.1.254")]
    [InlineData("10.0.0.50", "255.255.0.0", "10.0.255.254")]
    [InlineData("10.10.10.33", "255.255.255.240", "10.10.10.46")]
    public void LastUsableAddress_ValidInputs_ReturnsLastUsable(string ip, string mask, string expected)
    {
        Assert.Equal(expected, GatewayCalculator.LastUsableAddress(ip, mask));
    }

    [Theory]
    [InlineData("not-an-ip", "255.255.255.0")]
    [InlineData("192.168.1.1", "not-a-mask")]
    [InlineData("", "")]
    public void LastUsableAddress_InvalidInputs_ReturnsNull(string ip, string mask)
    {
        Assert.Null(GatewayCalculator.LastUsableAddress(ip, mask));
    }

    [Theory]
    [InlineData("192.168.1.1", "255.255.255.254")]
    [InlineData("192.168.1.1", "255.255.255.255")]
    public void LastUsableAddress_TooSmallSubnet_ReturnsNull(string ip, string mask)
    {
        Assert.Null(GatewayCalculator.LastUsableAddress(ip, mask));
    }

    [Fact]
    public void LastUsableAddress_NonContiguousMask_ReturnsNull()
    {
        Assert.Null(GatewayCalculator.LastUsableAddress("192.168.1.1", "255.0.255.0"));
    }
}
