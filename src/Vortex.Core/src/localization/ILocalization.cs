namespace Vortex.Core.Localization;

public interface ILocalization
{
    bool isInitialized { get; }

    string value { get; }

    string raw { get; }
}
