using System;
using System.IO;
using System.Runtime.CompilerServices;

using Godot;

namespace Vortex.Core.Logging;

public enum LogLevel
{
	DEBUG = 0,
	INFO = 1,
	WARNING = 2,
	ERROR = 3,
}

/// <summary>
///     Centralized logging utility for Vortex. Provides formatted console output
///     via Godot methods and optional file persistence.
/// </summary>
public static class Logger
{
	private static readonly object _lock = new();
	private static StreamWriter? _writer;
	private static bool _initialized;

	public static LogLevel MinLevel { get; set; } = LogLevel.DEBUG;
	public static bool FileLogging { get; set; } = true;

	public static void Initialize()
	{
		lock (_lock)
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;

			if (!FileLogging)
			{
				return;
			}

			try
			{
				string? logsDir = ProjectSettings.GlobalizePath("user://logs");

				Directory.CreateDirectory(logsDir);

				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string logPath = Path.Combine(logsDir, $"vortex_{timestamp}.log");

				_writer = new StreamWriter(logPath, false)
				{
					AutoFlush = false,
				};

				Info("Log file opened: " + logPath);
			}
			catch (Exception e)
			{
				GD.PushError($"[Logger] Failed to open log file: {e.Message}");
				_writer = null;
			}
		}
	}

	public static void Shutdown()
	{
		lock (_lock)
		{
			if (_writer != null)
			{
				try
				{
					_writer.Flush();
					_writer.Dispose();
				}
				catch
				{
					// Best-effort cleanup.
				}

				_writer = null;
			}

			_initialized = false;
		}
	}

	public static void Debug
	(
		string message,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = ""
	)
	{
		Log(LogLevel.DEBUG, message, null, filePath, lineNumber, memberName);
	}

	public static void Info
	(
		string message,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = ""
	)
	{
		Log(LogLevel.INFO, message, null, filePath, lineNumber, memberName);
	}

	public static void Warn
	(
		string message,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = ""
	)
	{
		Log(LogLevel.WARNING, message, null, filePath, lineNumber, memberName);
	}

	public static void Error
	(
		string message,
		Exception? ex = null,
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0,
		[CallerMemberName] string memberName = ""
	)
	{
		Log(LogLevel.ERROR, message, ex, filePath, lineNumber, memberName);
	}

	private static void Log
	(
		LogLevel level,
		string message,
		Exception? ex,
		string filePath,
		int lineNumber,
		string memberName
	)
	{
		if (level < MinLevel)
		{
			return;
		}

		string fileName = Path.GetFileName(filePath);
		string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		string levelTag = level.ToString();
		string formatted = $"[{timestamp}] [{levelTag}] [{fileName}:{lineNumber} {memberName}] {message}";

		switch (level)
		{
			case LogLevel.DEBUG:
			case LogLevel.INFO:
				GD.Print(formatted);
				break;
			case LogLevel.WARNING:
				GD.PushWarning(formatted);
				break;
			case LogLevel.ERROR:
				GD.PushError(formatted);
				GD.PrintErr(formatted);
				break;
		}

		if (ex != null)
		{
			string exFormatted = $"  {ex}";

			switch (level)
			{
				case LogLevel.ERROR:
					GD.PrintErr(exFormatted);
					break;
				default:
					GD.Print(exFormatted);
					break;
			}
		}

		WriteToFile(formatted, ex, level == LogLevel.ERROR);
	}

	private static void WriteToFile(string formatted, Exception? ex, bool flush)
	{
		if (_writer == null)
		{
			return;
		}

		lock (_lock)
		{
			try
			{
				_writer.WriteLine(formatted);

				if (ex != null)
				{
					_writer.WriteLine($"  {ex}");
				}

				if (flush)
				{
					_writer.Flush();
				}
			}
			catch
			{
				// Best-effort file writing — don't crash on log failures.
			}
		}
	}
}
