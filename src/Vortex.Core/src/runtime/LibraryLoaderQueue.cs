// @see WIN63-202407091256-704579380-Source-main/core/utils/LibraryLoaderQueue.as

using System;
using System.Linq;

namespace Vortex.Core.Runtime;

/// <summary>
/// Queue manager for parallel library downloads with concurrency throttling.
/// Godot adaptation: Uses callbacks instead of Flash's URLLoader events.
/// </summary>
/// @see core/utils/LibraryLoaderQueue.as
public class LibraryLoaderQueue
{
    private const int MAX_SIMULTANEOUS_DOWNLOADS = 4;

    private readonly List<LoaderEntry> _pausedLoaders = [];
    private readonly List<LoaderEntry> _activeLoaders = [];

    /// @see LibraryLoaderQueue.as::get length
    public int Length => _pausedLoaders.Count + _activeLoaders.Count;

    /// @see LibraryLoaderQueue.as::get disposed
    public bool Disposed { get; private set; }

    /// @see LibraryLoaderQueue.as::push
    public void Push(string url, Action<string, bool>? callback)
    {
        if (Disposed || IsUrlInQueue(url))
        {
            return;
        }

        LoaderEntry entry = new(url, callback);

        _pausedLoaders.Add(entry);

        Next();
    }

    /// @see LibraryLoaderQueue.as::dispose
    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        _activeLoaders.Clear();
        _pausedLoaders.Clear();
        Disposed = true;
    }

    /// @see LibraryLoaderQueue.as::next
    private void Next()
    {
        if (Disposed)
        {
            return;
        }

        while (_activeLoaders.Count < MAX_SIMULTANEOUS_DOWNLOADS && _pausedLoaders.Count > 0)
        {
            LoaderEntry entry = _pausedLoaders[0];

            _pausedLoaders.RemoveAt(0);
            _activeLoaders.Add(entry);

            StartLoad(entry);
        }
    }

    /// <summary>
    /// Starts loading a library entry. Godot adaptation: immediately completes
    /// since libraries are loaded via asset pipeline, not HTTP downloads.
    /// </summary>
    private void StartLoad(LoaderEntry entry)
    {
        // Godot adaptation: In the AS3 client, LibraryLoader starts an HTTP download.
        // In Godot, the asset library URL is registered but content is loaded locally
        // via the manifest/resource system. We complete immediately.
        OnLoadComplete(entry, true);
    }

    /// @see LibraryLoaderQueue.as::libraryLoadedHandler
    private void OnLoadComplete(LoaderEntry entry, bool success)
    {
        _activeLoaders.Remove(entry);
        entry.Callback?.Invoke(entry.Url, success);

        Next();
    }

    /// @see LibraryLoaderQueue.as::isUrlInQueue
    private bool IsUrlInQueue(string url)
    {
        string baseUrl = url;
        int queryIndex = url.IndexOf('?');

        if (queryIndex > -1)
        {
            baseUrl = url[..queryIndex];
        }

        return _pausedLoaders.Any(entry => entry.Url.StartsWith(baseUrl, StringComparison.Ordinal)) ||
               _activeLoaders.Any(entry => entry.Url.StartsWith(baseUrl, StringComparison.Ordinal));
    }

    private sealed class LoaderEntry(string url, Action<string, bool>? callback)
    {
        public string Url { get; } = url;

        public Action<string, bool>? Callback { get; } = callback;
    }
}
