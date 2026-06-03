using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Godot;

namespace Vortex.Habbo.Utils;

/// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as
public static class CommunicationUtils
{
    public const string SOL_PROPERTY_ENVIRONMENT = "environment";
    public const string SOL_PROPERTY_LOGIN_NAME = "login";
    public const string SOL_PROPERTY_CHARACTER_ID = "userid";
    public const string SOL_PROPERTY_CHARACTER_UNIQUE_ID = "useruniqueid";
    public const string SOL_PROPERTY_REMEMBER_LOGIN = "autologin";
    public const string SOL_PROPERTY_LOGIN_METHOD = "loginmethod";
    public const string SOL_PROPERTY_MACHINE_ID = "machineid";
    public const string SOL_PROPERTY_APP_RATER_STATUS = "ratingstatus";
    public const string SOL_PROPERTY_APP_RATER_TIMESTAMP = "ratingstatustime";
    public const string LOGIN_METHOD_HABBO = "habbo";
    public const string const_79 = "facebook";

    private const string SOL_FILE_NAME = "fuselogin.json";
    private const string SOL_PROPERTY_PASSWORD = "password";
    private static readonly object StorageLock = new();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::clearAllLoginData
    public static void ClearAllLoginData()
    {
        WriteSOLProperty(SOL_PROPERTY_LOGIN_METHOD, null);
        WriteSOLProperty(SOL_PROPERTY_ENVIRONMENT, null);
        WriteSOLProperty(SOL_PROPERTY_CHARACTER_ID, null);
        WriteSOLProperty(SOL_PROPERTY_REMEMBER_LOGIN, null);
        StorePassword(null);

        ForcedAutoLoginEnabled = false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::forcedAutoLoginEnabled
    public static bool ForcedAutoLoginEnabled { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::resetPassword
    public static void ResetPassword()
    {
        WriteSOLProperty(SOL_PROPERTY_PASSWORD, string.Empty);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::storePassword
    public static void StorePassword(string? param1)
    {
        WriteSOLProperty(SOL_PROPERTY_PASSWORD, param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::restorePassword
    public static string RestorePassword()
    {
        return ReadSOLString(SOL_PROPERTY_PASSWORD, string.Empty) ?? string.Empty;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::propertyExists
    public static bool PropertyExists(string param1)
    {
        lock (StorageLock)
        {
            Dictionary<string, string?> storage = ReadStorage();
            return storage.TryGetValue(param1, out string? value) && value != null;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::writeSOLProperty
    public static void WriteSOLProperty(string param1, object? param2)
    {
        lock (StorageLock)
        {
            Dictionary<string, string?> storage = ReadStorage();

            storage[param1] = ConvertToStorageValue(param2);

            WriteStorage(storage);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::readSOLProperty
    public static object? ReadSOLProperty(string param1, object? param2 = null)
    {
        lock (StorageLock)
        {
            Dictionary<string, string?> storage = ReadStorage();

            if (!storage.TryGetValue(param1, out string? value) || value == null)
            {
                return param2;
            }

            if (!string.Equals(param1, SOL_PROPERTY_ENVIRONMENT, StringComparison.Ordinal))
            {
                return value;
            }

            value = value.Replace("hh", string.Empty, StringComparison.Ordinal);
            value = value.Replace("br", "pt", StringComparison.Ordinal);
            value = value.Replace("us", "en", StringComparison.Ordinal);

            return value;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::readSOLString
    public static string? ReadSOLString(string param1, string? param2 = null)
    {
        object? value = ReadSOLProperty(param1, param2);

        return value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::readSOLBoolean
    public static bool ReadSOLBoolean(string param1, string? param2 = null)
    {
        string? value = ReadSOLString(param1, param2);

        return value != null && (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::readSOLInteger
    public static int ReadSOLInteger(string param1, string? param2 = null)
    {
        string? value = ReadSOLString(param1, param2);

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : 0;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::readSOLFloat
    public static double ReadSOLFloat(string param1, string? param2 = null)
    {
        string? value = ReadSOLString(param1, param2);

        return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double parsed)
            ? parsed
            : 0.0;
    }

    private static Dictionary<string, string?> ReadStorage()
    {
        string storagePath = GetStoragePath();

        if (!File.Exists(storagePath))
        {
            return new Dictionary<string, string?>(StringComparer.Ordinal);
        }

        try
        {
            string json = File.ReadAllText(storagePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, string?>(StringComparer.Ordinal);
            }

            return JsonSerializer.Deserialize<Dictionary<string, string?>>(json)
                   ?? new Dictionary<string, string?>(StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string?>(StringComparer.Ordinal);
        }
    }

    private static void WriteStorage(Dictionary<string, string?> storage)
    {
        string storagePath = GetStoragePath();
        string? directory = Path.GetDirectoryName(storagePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(storage);

        File.WriteAllText(storagePath, json);
    }

    private static string GetStoragePath()
    {
        string? userRoot = ProjectSettings.GlobalizePath("user://");

        if (string.IsNullOrWhiteSpace(userRoot))
        {
            userRoot = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Vortex");
        }

        Directory.CreateDirectory(userRoot);

        return Path.Combine(userRoot, SOL_FILE_NAME);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/utils/CommunicationUtils.as::generateFingerprint
    public static string GenerateFingerprint()
    {
        string seed = $"{OS.GetName()}|{Time.GetUnixTimeFromSystem()}|{GD.Randi()}";
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? ConvertToStorageValue(object? value)
    {
        return value switch
        {
            null => null,
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}
