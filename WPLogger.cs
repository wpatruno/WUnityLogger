using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Custom log system that allow to choose when to display message
/// Optimized for runtime and builds.
/// </summary>
public static class WPLogger
{
	/// <summary>
	/// String containing all logs
	/// </summary>
	public static StringBuilder LogHistory { get; private set; } = new StringBuilder();
	/// <summary>
	/// List of active tags (tag can be any string)
	/// </summary>
	static List<string> activeTags = new List<string>() { WPTag.FORCE, WPTag.INFO, WPTag.IMPORTANT };
	/// <summary>
	/// Write log to Debug unity class
	/// </summary>
	public static bool LogToUnity = true;
	/// <summary>
	/// Write log to local global log
	/// </summary>
	public static bool LogToHistory = true;
	/// <summary>
	/// Diplay tag on front of log
	/// </summary>
	public static bool DisplayTagHeader = true;
	/// <summary>
	/// Display time on front of log
	/// </summary>
	public static bool LogTime = false;

	public delegate void WPLoggerEvent(string logText);

	/// <summary>
	/// Called when a new log is processed
	/// </summary>
	public static WPLoggerEvent OnLogged;
	/// <summary>
	/// Called when a new error is processed
	/// </summary>
	public static WPLoggerEvent OnErrorLogged;

	public static void ApplySettings(WPLoggerData.Settings settings)
	{
		LogToUnity = settings.logToUnity;
		LogToHistory = settings.logToHistory;
		DisplayTagHeader = settings.displayTagHeader;
		LogTime = settings.logTime;
		activeTags = new List<string>(settings.defaultActiveTags);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void Show(params object[] objs)
	{
		string str = "";
		foreach (var item in objs)
		{
			if (item == null) str += "[null]";
			else str += "[" + item.GetType() + ":" + item.ToString() + "]";
		}
		UnityEngine.Debug.Log(str);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	/// <summary>
	/// Fast log only wrap default Unity logger
	/// </summary>
	/// <param name="text"></param>
	public static void FLog(string text)
	{
		UnityEngine.Debug.Log(text);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	/// <summary>
	/// Fast log only wrap default Unity logger
	/// </summary>
	/// <param name="text"></param>
	public static void FLog(string text, UnityEngine.Object context)
	{
		UnityEngine.Debug.Log(text, context);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void Log(string text, params string[] tags)
	{
		if (tags != null && tags.Length > 0)
		{
			if (!HasActiveTag(tags)) return;
			if (DisplayTagHeader)
				text = "[" + System.String.Join(',', tags) + "] " + text;
		}

		if (LogTime)
		{
			text = "(" + GetTime() + ") " + text;
		}

		if (LogToHistory)
		{
			LogHistory.AppendLine(text);
		}

		if (LogToUnity)
		{
			UnityEngine.Debug.Log(text);
		}

		OnLogged?.Invoke(text);
	}

	static bool HasActiveTag(params string[] tags)
	{
		foreach (var t in tags)
		{
			if (activeTags.Contains(t))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Logging Error always display them, even when using tags
	/// </summary>
	/// <param name="text"></param>
	/// <param name="tags"></param>
	public static void Error(string text, params string[] tags)
	{
		if (tags != null && tags.Length > 0)
		{
			if (DisplayTagHeader)
				text = "[" + System.String.Join(',', tags) + "] " + text;
		}

		if (LogTime)
		{
			text = "(" + GetTime() + ") " + text;
		}

		if (LogToHistory)
		{
			LogHistory.AppendLine(text);
		}

		UnityEngine.Debug.LogError(text);
		OnErrorLogged?.Invoke(text);
	}

	static string GetTime()
	{
		return System.DateTime.Now.ToString("HH:mm:ss");
	}

	public static void Clear()
	{
		LogHistory.Clear();
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagActive(string tag)
	{
		if (!string.IsNullOrWhiteSpace(tag) && !activeTags.Contains(tag))
		{
			activeTags.Add(tag);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagDisabled(string tag)
	{
		if (!string.IsNullOrWhiteSpace(tag) && tag != "F")  // F tag can't be disabled
		{
			activeTags.Remove(tag);
		}
	}

	public static bool IsTagActive(string tag)
	{
		if (string.IsNullOrWhiteSpace(tag)) return false;
		return activeTags.Contains(tag);
	}

	public static string[] GetTags()
	{
		return activeTags.ToArray();
	}
}

/// <summary>
/// This static class only contain some possible tags.
/// You can use any other string as tag.
/// Tips: Make your own static class that contain const string for your own tags
/// </summary>
public static class WPTag
{
	// Special tag that always display a log
	public const string FORCE = "F";

	// Examples of tags
	public const string INFO = "INFO";
	public const string WARNING = "WARN";
	public const string IMPORTANT = "IMP";
	public const string ANALYTIC = "ANALYTIC";
	public const string PLAYER = "PLAYER";
	public const string UI = "UI";
}
