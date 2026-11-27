using System.Buffers.Binary;
using System.Text;

namespace BriefTaube.Core;

public static class FileNameProtocol
{
    public static async Task WriteAsync(Stream destination, string fileName,
        CancellationToken cancellationToken = default)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);

        // Получателю нужна длина имени в байтах UTF-8.
        byte[] header = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(header, nameBytes.Length);

        // Поток принадлежит вызывающему коду: он может отправить следующее поле.
        await destination.WriteAsync(header, cancellationToken);
        await destination.WriteAsync(nameBytes, cancellationToken);
    }
}
