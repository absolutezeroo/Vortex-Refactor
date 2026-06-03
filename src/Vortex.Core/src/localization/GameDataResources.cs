using System.Text.Json;

namespace Vortex.Core.Localization;

/// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as
public class GameDataResources : IGameDataResources
{
    private string _externalTextsUrl = string.Empty;
    private string _externalTextsHash = string.Empty;
    private string _externalVariablesUrl = string.Empty;
    private string _externalVariablesHash = string.Empty;
    private string _furniDataUrl = string.Empty;
    private string _furniDataHash = string.Empty;
    private string _productDataUrl = string.Empty;
    private string _productDataHash = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::parse
    public static GameDataResources? Parse(string param1)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(param1);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("hashes", out JsonElement hashes) || hashes.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            GameDataResources result = new();

            foreach (JsonElement entry in hashes.EnumerateArray())
            {
                if (!entry.TryGetProperty("name", out JsonElement nameProp) ||
                    !entry.TryGetProperty("url", out JsonElement urlProp) ||
                    !entry.TryGetProperty("hash", out JsonElement hashProp))
                {
                    continue;
                }

                string name = nameProp.GetString() ?? string.Empty;
                string url = urlProp.GetString() ?? string.Empty;
                string hash = hashProp.GetString() ?? string.Empty;

                switch (name)
                {
                    case "external_texts":
                        result._externalTextsUrl = url;
                        result._externalTextsHash = hash;
                        break;
                    case "external_variables":
                        result._externalVariablesUrl = url;
                        result._externalVariablesHash = hash;
                        break;
                    case "furnidata":
                        result._furniDataUrl = url;
                        result._furniDataHash = hash;
                        break;
                    case "productdata":
                        result._productDataUrl = url;
                        result._productDataHash = hash;
                        break;
                }
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::isValid
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(_externalTextsUrl) &&
               !string.IsNullOrEmpty(_externalTextsHash) &&
               !string.IsNullOrEmpty(_externalVariablesUrl) &&
               !string.IsNullOrEmpty(_externalVariablesHash) &&
               !string.IsNullOrEmpty(_furniDataUrl) &&
               !string.IsNullOrEmpty(_furniDataHash) &&
               !string.IsNullOrEmpty(_productDataUrl) &&
               !string.IsNullOrEmpty(_productDataHash);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getExternalTextsUrl
    public string GetExternalTextsUrl()
    {
        return _externalTextsUrl;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getExternalTextsHash
    public string GetExternalTextsHash()
    {
        return _externalTextsHash;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getExternalVariablesUrl
    public string GetExternalVariablesUrl()
    {
        return _externalVariablesUrl;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getExternalVariablesHash
    public string GetExternalVariablesHash()
    {
        return _externalVariablesHash;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getFurniDataUrl
    public string GetFurniDataUrl()
    {
        return _furniDataUrl;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getFurniDataHash
    public string GetFurniDataHash()
    {
        return _furniDataHash;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getProductDataUrl
    public string GetProductDataUrl()
    {
        return _productDataUrl;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/class_50.as::getProductDataHash
    public string GetProductDataHash()
    {
        return _productDataHash;
    }
}
