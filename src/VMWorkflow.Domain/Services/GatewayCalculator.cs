using System.Net;

namespace VMWorkflow.Domain.Services;

public static class GatewayCalculator
{
    public static string? LastUsableAddress(string ipAddress, string subnetMask)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            return null;
        if (!IPAddress.TryParse(subnetMask, out var mask) || mask.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            return null;

        var ipBytes = ip.GetAddressBytes();
        var maskBytes = mask.GetAddressBytes();
        if (!IsContiguousMask(maskBytes)) return null;

        var networkBytes = new byte[4];
        var broadcastBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            broadcastBytes[i] = (byte)(networkBytes[i] | (byte)~maskBytes[i]);
        }

        var prefix = PrefixLength(maskBytes);
        if (prefix >= 31) return null;

        broadcastBytes[3] = (byte)(broadcastBytes[3] - 1);
        return new IPAddress(broadcastBytes).ToString();
    }

    private static bool IsContiguousMask(byte[] maskBytes)
    {
        uint mask = ((uint)maskBytes[0] << 24) | ((uint)maskBytes[1] << 16) | ((uint)maskBytes[2] << 8) | maskBytes[3];
        uint inverted = ~mask;
        return (inverted & (inverted + 1)) == 0;
    }

    private static int PrefixLength(byte[] maskBytes)
    {
        int count = 0;
        foreach (var b in maskBytes)
            for (int i = 7; i >= 0; i--)
                if (((b >> i) & 1) == 1) count++;
                else return count;
        return count;
    }
}
