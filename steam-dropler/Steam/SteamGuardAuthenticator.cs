using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using steam_dropler.Model;

namespace steam_dropler.Steam;

public class SteamGuardAuthenticator
{
    private static readonly byte[] SteamGuardCodeTranslations = new byte[] { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };


    public static async Task<string> GetGuardCodeLongTimeValid(string key)
    {
        var time = await TimeAligner.GetSteamTimeAsync();

        var currentCode = GenerateSteamGuardCodeForTime(time, key);

        var prevCode = GenerateSteamGuardCodeForTime(time - 5, key);
        if (prevCode != currentCode)
        {
            return currentCode;
        }

        while (currentCode == prevCode)
        {
            await Task.Delay(2000);
            time = await TimeAligner.GetSteamTimeAsync();
            prevCode = currentCode;
            currentCode = GenerateSteamGuardCodeForTime(time, key);
        }

        return currentCode;

    }

    public static async Task<string> GetGuardCode(string key)
    {
        var code = GenerateSteamGuardCodeForTime(await TimeAligner.GetSteamTimeAsync(), key);
        return code;
    }

    static string GenerateSteamGuardCodeForTime(long time, string secret)
    {
        if (string.IsNullOrEmpty(secret))
        {
            return "";
        }

        string sharedSecretUnescaped = Regex.Unescape(secret);
        byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
        byte[] timeArray = new byte[8];

        time /= 30L;

        for (int i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)time;
            time >>= 8;
        }

        var hmacGenerator = new HMACSHA1
        {
            Key = sharedSecretArray
        };

        byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
        byte[] codeArray = new byte[5];
        try
        {
            byte b = (byte)(hashedData[19] & 0xF);
            int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

            for (int i = 0; i < 5; ++i)
            {
                codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                codePoint /= SteamGuardCodeTranslations.Length;
            }
        }
        catch (Exception)
        {
            return string.Empty; //Change later, catch-alls are bad!
        }
        return Encoding.UTF8.GetString(codeArray);
    }
}