using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides a randomly generating PKCE code verifier and it's corresponding code challenge.
/// </summary>
public static class Pkce
{
    /// <summary>
    /// Generates a code_verifier and the corresponding code_challenge, as specified in the rfc-7636.
    /// </summary>
    /// <remarks>See https://datatracker.ietf.org/doc/html/rfc7636#section-4.1 and https://datatracker.ietf.org/doc/html/rfc7636#section-4.2</remarks>
    public static (string code_challenge, string verifier) Generate(int size = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[size];
        rng.GetBytes(randomBytes);
        var verifier = Base64UrlEncode(randomBytes);

        var buffer = Encoding.UTF8.GetBytes(verifier);
        var hash = SHA256.Create().ComputeHash(buffer);
        var challenge = Base64UrlEncode(hash);

        return (challenge, verifier);
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}