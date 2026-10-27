using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace back_end.src.Requests;

public static class RequestIdempotency
{
    public static bool TryParse(string? value, out Guid? key)
    {
        key = null;
        if (value is null) return true; // Older clients have no idempotency contract.
        if (!Guid.TryParseExact(value, "D", out var parsed) || parsed == Guid.Empty) return false;
        key = parsed;
        return true;
    }

    public static string HashJson<T>(T value) => Hash(JsonSerializer.Serialize(value));

    public static async Task<string> HashAnalysisAsync(int coletaId, IFormFile? image, CancellationToken cancellationToken)
    {
        var imageHash = "server-image";
        if (image is not null)
        {
            using var stream = image.OpenReadStream();
            imageHash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
        }
        return Hash($"{coletaId}:{imageHash}");
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
