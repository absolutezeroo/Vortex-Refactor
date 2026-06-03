// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as
public interface ICoreConfiguration : IUnknown
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::propertyExists
    bool PropertyExists(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getProperty
    string GetProperty(string param1, IDictionary<string, string>? param2 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::setProperty
    void SetProperty(string param1, string param2, bool param3 = false, bool param4 = false);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getBoolean
    bool GetBoolean(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::getInteger
    int GetInteger(string param1, int param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::interpolate
    string? Interpolate(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICoreConfiguration.as::updateUrlProtocol
    string UpdateUrlProtocol(string param1);
}
