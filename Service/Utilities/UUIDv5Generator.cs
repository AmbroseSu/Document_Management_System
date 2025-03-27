using System.Security.Cryptography;
using System.Text;

namespace Service.Response;

public class UUIDv5Generator
{
    private static readonly Guid namespaceId = new("e8b7f460-28e4-5c8b-bd6d-dc5e5b7db1fe");

    public static Guid Generate(string name)
    {
        using (var sha1 = SHA1.Create())
        {
            var namespaceBytes = namespaceId.ToByteArray();
            Array.Reverse(namespaceBytes); // Đảm bảo đúng thứ tự byte
            var nameBytes = Encoding.UTF8.GetBytes(name);

            var hash = sha1.ComputeHash(Combine(namespaceBytes, nameBytes));

            // Định dạng lại theo chuẩn UUID (phiên bản 5)
            hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // Phiên bản 5
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // Biến thể UUID

            return new Guid(new ReadOnlySpan<byte>(hash, 0, 16));
        }
    }

    // Hàm kết hợp 2 mảng byte
    private static byte[] Combine(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }
}