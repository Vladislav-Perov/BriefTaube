using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BriefTaube.Core;
using Xunit;

namespace BriefTaube.Tests;

public class FileNameProtocolTests
{
    [Theory]
    [InlineData("README.md", "00000009524541444D452E6D64")]
    [InlineData("Отчёт.pdf", "0000000ED09ED182D187D191D1822E706466")]
    public async Task WriteAsync_WritesLengthAndUtf8Name(string fileName, string expectedHex)
    {
        // Arrange: заранее задаём ожидаемые байты и готовим поток в памяти.
        byte[] expected = Convert.FromHexString(expectedHex);
        using var output = new MemoryStream();

        // Act: запускаем тот же метод, который вызывает кнопка в MAUI.
        await FileNameProtocol.WriteAsync(output, fileName);

        // Assert: проверяем заголовок и каждый байт имени.
        Assert.Equal(expected, output.ToArray());
    }

    [Fact]
    public async Task ReceiverReadsTwoNames_WhileSenderRemainsConnected()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = timeout.Token;

        // Порт 0 позволяет ОС выбрать свободный порт для этого теста.
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        using var sender = new TcpClient();
        await sender.ConnectAsync(endpoint.Address, endpoint.Port, token);
        using var receiver = await listener.AcceptTcpClientAsync(token);
        using var outgoing = sender.GetStream();
        using var incoming = receiver.GetStream();

        string[] names = ["README.md", "Отчёт.pdf"];
        foreach (string name in names)
            await FileNameProtocol.WriteAsync(outgoing, name, token);

        foreach (string expected in names)
        {
            byte[] header = new byte[sizeof(int)];
            await incoming.ReadExactlyAsync(header, token);
            int length = BinaryPrimitives.ReadInt32BigEndian(header);
            Assert.Equal(Encoding.UTF8.GetByteCount(expected), length);

            byte[] body = new byte[length];
            await incoming.ReadExactlyAsync(body, token);
            Assert.Equal(expected, Encoding.UTF8.GetString(body));
        }
    }
}
