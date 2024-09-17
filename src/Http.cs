using System.Text;

namespace HeartbeatServer;
internal ref struct Http
{
    private const byte ByteCR = (byte)'\r';
    private const byte ByteLF = (byte)'\n';
    private const byte ByteColon = (byte)':';

    /// <summary>
    /// Extracts the DeviceId from the HTTP/1.1 request.
    /// Iterates over the request and looks for the "DeviceId" header.
    /// Otherwise, returns false.
    /// </summary>
    /// <param name="request">The HTTP/1.1 request</param>
    /// <param name="deviceId">The DeviceId extracted from the request</param>
    /// <returns>True if the DeviceId was found, otherwise false</returns>
    public static bool TryExtractDeviceId(Span<byte> request, out int deviceId)
    {
        // Iterate over the request and look for the "DeviceId" header.
        while (request.Length > 0)
        {
            int crIndex = request.IndexOf(ByteCR);
            if (crIndex > 0)
            {
                // Check if the next byte is LF
                if ((uint)request.Length > (uint)(crIndex + 1) && request[crIndex + 1] == ByteLF)
                {
                    int colonIndex = request.Slice(0, crIndex).IndexOf(ByteColon);

                    if (colonIndex > 0)
                    {
                        Span<byte> headerName = request.Slice(0, colonIndex);

                        // Check if the header name is "DeviceId"
                        if (headerName.SequenceEqual(DeviceIdHeaderName))
                        {
                            Span<byte> headerValue = request.Slice(colonIndex + 1, crIndex - colonIndex - 1);
                            string deviceIdStr = Encoding.UTF8.GetString(headerValue);
                            if (int.TryParse(deviceIdStr, out int id))
                            {
                                deviceId = id;
                                return true;
                            }
                            else
                            {
                                break; // Invalid integer
                            }

                        }
                    }
                    request = request.Slice(crIndex + 2); // Move to the next line to examine the next header
                    continue;
                }
                request = request.Slice(crIndex);
            }
            else
            {
                break; // No more headers
            }
        }
        deviceId = default;
        return false;
    }

    public static bool IsEndOfRequest(Span<byte> request)
    {
        if (request.Length >= 4 && request[0..4].SequenceEqual(EndOfHttp1Request))
        {
            return true;
        }
        return false;
    }
    public static ReadOnlySpan<byte> EndOfHttp1Request => "\r\n\r\n"u8;
    public static ReadOnlySpan<byte> DeviceIdHeaderName => "Device-Id"u8;
    public static Memory<byte> OkResponse => "HTTP/1.1 204 No Content\r\n\r\n"u8.ToArray();
    public static Memory<byte> BadRequestResponse => "HTTP/1.1 400 Bad Request\r\n\r\n"u8.ToArray();
}