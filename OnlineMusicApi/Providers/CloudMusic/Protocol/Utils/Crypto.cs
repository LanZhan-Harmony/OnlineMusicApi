using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace OnlineMusicApi.Internal.Providers.CloudMusic.Protocol.Utils;

internal static class Crypto
{
    private static readonly byte[] iv = Encoding.ASCII.GetBytes("0102030405060708");
    private static readonly byte[] presetKey = Encoding.ASCII.GetBytes("0CoJUm6Qyw8W8jud");
    private static readonly byte[] linuxapiKey = Encoding.ASCII.GetBytes("rFgB&h#%2?^eDg:Q");
    private const string base62 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string publicKey =
        "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDgtQn2JZ34ZC28NWYpAUd98iZ37BUrX/aKzmFbt7clFSs6sXqHauqKWqdtLkF2KexO40H1YTX8z2lSgBBOAxLsvaklV8k4cBFK9snQXE9/DDaFt6Rr7iVZMldczhC0JNgTz+SHXT6CBHuX3e9SdB1Ua44oncaTWz7OBGLbCiK45wIDAQAB\n-----END PUBLIC KEY-----";
    private static readonly byte[] eapiKey = Encoding.ASCII.GetBytes("e82ckenh8dichen8");
    private static RSAParameters? _cachedPublicKey;

    public static Dictionary<string, string> WEApi(Dictionary<string, string> data)
    {
        string text = ToJsonObject(data).ToJsonString();
        byte[] secretKey = RandomNumberGenerator.GetBytes(16);
        secretKey = [.. secretKey.Select(n => (byte)base62[n % 62])];

        return new()
        {
            {
                "params",
                Base64(
                    AesEncrypt(
                        Utf8(Base64(AesEncrypt(Utf8(text), CipherMode.CBC, presetKey, iv))),
                        CipherMode.CBC,
                        secretKey,
                        iv
                    )
                )
            },
            { "encSecKey", HexLower(RsaEncrypt([.. secretKey.Reverse()])) },
        };
    }

    public static Dictionary<string, string> LinuxApi(JsonObject payload)
    {
        string text = payload.ToJsonString();
        return new()
        {
            { "eparams", HexUpper(AesEncrypt(Utf8(text), CipherMode.ECB, linuxapiKey, null)) },
        };
    }

    public static Dictionary<string, string> EApi(string url, Dictionary<string, string> data)
    {
        string text = ToJsonObject(data).ToJsonString();
        string message = $"nobody{url}use{text}md5forencrypt";
        string digest = HexLower(MD5.HashData(Utf8(message)));
        string payload = $"{url}-36cd479b6b5-{text}-36cd479b6b5-{digest}";
        return new()
        {
            { "params", HexUpper(AesEncrypt(Utf8(payload), CipherMode.ECB, eapiKey, null)) },
        };
    }

    public static byte[] Decrypt(byte[] cipherBuffer) =>
        AesDecrypt(cipherBuffer, CipherMode.ECB, eapiKey, null);

    private static JsonObject ToJsonObject(Dictionary<string, string> data)
    {
        JsonObject result = [];
        foreach ((string key, string value) in data)
        {
            result[key] = value;
        }

        return result;
    }

    private static byte[] Utf8(string value) => Encoding.UTF8.GetBytes(value);

    private static string Base64(byte[] value) => Convert.ToBase64String(value);

    private static string HexLower(byte[] value) => Convert.ToHexString(value).ToLowerInvariant();

    private static string HexUpper(byte[] value) => Convert.ToHexString(value);

    private static byte[] AesEncrypt(byte[] buffer, CipherMode mode, byte[] key, byte[]? iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        return mode switch
        {
            CipherMode.CBC => aes.EncryptCbc(buffer, iv!, PaddingMode.PKCS7),
            CipherMode.ECB => aes.EncryptEcb(buffer, PaddingMode.PKCS7),
            _ => throw new NotSupportedException($"Mode {mode} is not supported."),
        };
    }

    private static byte[] AesDecrypt(byte[] buffer, CipherMode mode, byte[] key, byte[]? iv)
    {
        using Aes aes = Aes.Create();
        aes.Key = key;
        return mode switch
        {
            CipherMode.CBC => aes.DecryptCbc(buffer, iv!, PaddingMode.PKCS7),
            CipherMode.ECB => aes.DecryptEcb(buffer, PaddingMode.PKCS7),
            _ => throw new NotSupportedException($"Mode {mode} is not supported."),
        };
    }

    private static byte[] RsaEncrypt(byte[] buffer)
    {
        if (_cachedPublicKey is null)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            _cachedPublicKey = rsa.ExportParameters(false);
        }

        RSAParameters rsaParameters = _cachedPublicKey.Value;
        return BigInteger
            .ModPow(
                new BigInteger(buffer, true, true),
                new BigInteger(rsaParameters.Exponent, true, true),
                new BigInteger(rsaParameters.Modulus, true, true)
            )
            .ToByteArray(true, true);
    }
}
