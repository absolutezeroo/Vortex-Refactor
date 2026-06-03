// @see WIN63-202407091256-704579380-Source-main/core/class_79.as

using System;

using Godot;

using Vortex.Core.Runtime;

namespace Vortex.Core;

/// @see WIN63-202407091256-704579380-Source-main/core/class_79.as
public static class CoreEnvironment
{
    public static Action<string>? ExternalLogWarn { get; set; }
    public static Action<string>? ExternalLogDebug { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_FRAME_UPDATE_SIMPLE
    public const uint CORE_SETUP_FRAME_UPDATE_SIMPLE = 0;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_FRAME_UPDATE_COMPLEX
    public const uint CORE_SETUP_FRAME_UPDATE_COMPLEX = 1;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_FRAME_UPDATE_PROFILER
    public const uint CORE_SETUP_FRAME_UPDATE_PROFILER = 2;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_FRAME_UPDATE_EXPERIMENT
    public const uint CORE_SETUP_FRAME_UPDATE_EXPERIMENT = 4;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_FRAME_UPDATE_MASK
    public const uint CORE_SETUP_FRAME_UPDATE_MASK = 15;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::CORE_SETUP_DEBUG
    public const uint CORE_SETUP_DEBUG = 15;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_CONFIGURATION
    public const int ERROR_CATEGORY_DOWNLOAD_CONFIGURATION = 1;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_LIBRARY
    public const int ERROR_CATEGORY_DOWNLOAD_LIBRARY = 2;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_CRITICAL_ASSET
    public const int ERROR_CATEGORY_DOWNLOAD_CRITICAL_ASSET = 3;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_PREPARE_COMPONENT
    public const int ERROR_CATEGORY_PREPARE_COMPONENT = 4;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_COMPONENT_RESOURCE_LOAD_ERROR
    public const int ERROR_CATEGORY_COMPONENT_RESOURCE_LOAD_ERROR = 5;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_INTERFACE_AVAILABILITY
    public const int ERROR_CATEGORY_INTERFACE_AVAILABILITY = 6;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_PRODUCT_DATA
    public const int ERROR_CATEGORY_PRODUCT_DATA = 7;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_LOCALIZATION
    public const int ERROR_CATEGORY_DOWNLOAD_LOCALIZATION = 8;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_FINALIZE_PRELOADING
    public const int ERROR_CATEGORY_FINALIZE_PRELOADING = 9;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_INITIALIZE_CORE
    public const int ERROR_CATEGORY_INITIALIZE_CORE = 10;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_FONT
    public const int ERROR_CATEGORY_DOWNLOAD_FONT = 11;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_FURNIDATA_DOWNLOAD
    public const int ERROR_CATEGORY_FURNIDATA_DOWNLOAD = 12;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES
    public const int ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES = 20;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES_OVERRIDE
    public const int ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES_OVERRIDE = 21;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_COMMMUNICATION_INIT
    public const int ERROR_CATEGORY_COMMMUNICATION_INIT = 29;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_CONNECT_TO_PROXY
    public const int ERROR_CATEGORY_CONNECT_TO_PROXY = 30;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_UNCAUGHT_ERROR
    public const int ERROR_UNCAUGHT_ERROR = 40;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::ERROR_CATEGORY_INTENTIONAL_DEBUG_CRASH
    public const int ERROR_CATEGORY_INTENTIONAL_DEBUG_CRASH = 99;

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::version
    public static string version => "0.0.3";

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::instance
    private static ICore? instance { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::instantiate
    public static ICore Instantiate(Node? param1, uint param2, ICoreErrorReporter? param3 = null,
        Dictionary<string, object?>? param4 = null)
    {
        instance ??= new CoreComponentContext(param1, param3 ?? new class_516(), param2, param4);

        return instance;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::error
    public static void Error(string param1, bool param2, int param3 = -1, Exception? param4 = null)
    {
        switch (instance)
        {
            case null:
                return;
            case CoreComponentContext context:
                context.Error(param1, param2, param3, param4);

                return;
            default:
                instance.Error(param1, param2, param3, param4);

                break;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::warning
    public static void Warning(string param1)
    {
        instance?.Warning(param1);

        Logger.Warn(param1);

        ExternalLogWarn?.Invoke(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::debug
    public static void Debug(string param1)
    {
        instance?.Debug(param1);

        Logger.Debug(param1);

        ExternalLogDebug?.Invoke(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::crash
    public static void Crash(string param1, int param2, Exception? param3 = null)
    {
        if (instance is CoreComponentContext context)
        {
            context.Error(param1, true, param2, param3);

            return;
        }

        instance?.Error(param1, true, param2, param3);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::purge
    public static void Purge()
    {
        instance?.Purge();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/class_79.as::dispose
    public static void Dispose()
    {
        if (instance == null)
        {
            return;
        }

        instance.Dispose();
        instance = null;
    }
}
