using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Configuration;

/// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as
public interface IHabboConfigurationManager : ICoreConfiguration
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::isInitialized
    bool IsInitialized();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::updateEnvironmentId
    void UpdateEnvironmentId(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::resetAll
    void ResetAll();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::initConfigurationDownload
    void InitConfigurationDownload();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::events
    EventDispatcherWrapper events { get; }
}
