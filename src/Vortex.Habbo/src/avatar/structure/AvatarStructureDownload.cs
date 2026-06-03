// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AvatarStructureDownload.as

using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Assets.Loaders;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AvatarStructureDownload.as
public class AvatarStructureDownload
{
    public const string STRUCTURE_DONE = "AVATAR_STRUCTURE_DONE";

    private readonly IStructureData _target;

    public EventDispatcherWrapper Events { get; } = new();

    /// True if data was already loaded (synchronously) during construction.
    public bool IsDone { get; private set; }

    /// @see AvatarStructureDownload.as::AvatarStructureDownload
    public AvatarStructureDownload(IAssetLibrary assets, string downloadUrl, IStructureData target)
    {
        _target = target;

        if (assets.HasAsset(downloadUrl))
        {
            Logger.Info("[AvatarStructureDownload] reload data for url: " + downloadUrl);
        }

        // Attempt to load from file path
        try
        {
            BinaryFileLoader loader = new("text/plain", downloadUrl);

            if (loader.Bytes is { Length: > 0 })
            {
                string text = System.Text.Encoding.UTF8.GetString(loader.Bytes);
                loader.Dispose();
                OnDataComplete(text);
            }
            else
            {
                loader.Dispose();
                OnDataFailed(downloadUrl);
            }
        }
        catch (System.Exception e)
        {
            Logger.Warn("[AvatarStructureDownload] Failed to load " + downloadUrl + ": " + e.Message);
            OnDataFailed(downloadUrl);
        }
    }

    /// @see AvatarStructureDownload.as::onDataComplete
    public void OnDataComplete(string xmlContent)
    {
        if (string.IsNullOrEmpty(xmlContent))
        {
            Logger.Error("[AvatarStructureDownload] Could not load avatar structure, got empty data");
            return;
        }

        try
        {
            XElement xml = System.Xml.Linq.XElement.Parse(xmlContent);
            _target.AppendXml(xml);
            IsDone = true;
            Events.DispatchEvent(STRUCTURE_DONE);
        }
        catch (System.Exception e)
        {
            Logger.Error("[AvatarStructureDownload] Error: " + e.Message);
        }
    }

    /// @see AvatarStructureDownload.as::onDataFailed
    public static void OnDataFailed(string url)
    {
        Logger.Error("[AvatarStructureDownload] Could not load avatar structure. Failed to get data from URL " + url);
    }
}
