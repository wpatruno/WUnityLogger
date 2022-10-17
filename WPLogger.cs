using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// Custom log system that allow to choose when to display message
/// Optimized for runtime and builds.
/// </summary>
public static class WPLogger
{
	const string FILE_NAME = "WPLOG";
	const string EXTENSION = ".txt";

	/// <summary>
	/// Reference to an UI text class to display in scene logs. Only used for dev build.
	/// </summary>
	public static UnityEngine.UI.Text debugTextUI;
	/// <summary>
	/// String containing all logs
	/// </summary>
	static StringBuilder globalText = new StringBuilder();
	/// <summary>
	/// List of active tags (tag can be any string)
	/// </summary>
	static List<string> activeTags = new List<string>() { WPTag.INFO, WPTag.IMPORTANT, WPTag.SERVER, WPTag.CLIENT };
	/// <summary>
	/// Write log to Debug unity class
	/// </summary>
	public static bool LogToUnity = true;
	/// <summary>
	/// Write log to local global log
	/// </summary>
	public static bool LogToGlobal = true;
	/// <summary>
	/// Diplay tag on front of log
	/// </summary>
	public static bool DisplayHeader = true;
	/// <summary>
	/// Display in header even inactive tags
	/// </summary>
	public static bool DisplayHeaderAllTag = true;
	/// <summary>
	/// Display time on front of log
	/// </summary>
	public static bool LogTime = false;
	/// <summary>
	/// Called when a new log is processed
	/// </summary>
	public static Action<string> OnLogged;
	/// <summary>
	/// Called when a new error is processed
	/// </summary>
	public static Action<string> OnErrorLogged;

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
			text = "[" + String.Join(',', tags) + "] " + text;
		}

		if (LogTime)
		{
			text = "(" + GetTime() + ")" + text;
		}

		if (LogToGlobal)
		{
			globalText.AppendLine(text);
			RefreshDebugUI();
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
			if (t == "F") return true; // F tag force to always display the log

			if (activeTags.Contains(t))
			{
				return true;
			}
		}
		return false;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	static void RefreshDebugUI()
	{
		if (debugTextUI != null)
		{
			debugTextUI.text = globalText.ToString();
		}
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
			text = "[" + String.Join(',', tags) + "] " + text;
		}

		if (LogTime)
		{
			text = "(" + GetTime() + ")" + text;
		}

		if (LogToGlobal)
		{
			globalText.AppendLine("<color=red>" + text + "</color>");
			RefreshDebugUI();
		}

		UnityEngine.Debug.LogError(text);
		OnErrorLogged?.Invoke(text);
	}

	static string GetTime()
	{
		return DateTime.Now.ToString("HH:mm:ss");
	}

	public static void Save(string add = "")
	{
		string filepath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + FILE_NAME + EXTENSION;
		System.IO.Directory.CreateDirectory(Application.persistentDataPath);
		System.IO.File.WriteAllText(filepath, globalText.ToString());
		Log("Log file created at -> " + filepath, "F", "WPLOGGER");
	}

	public static void Clear()
	{
		globalText.Clear();
		if (debugTextUI != null)
		{
			debugTextUI.text = "";
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagActive(string tag)
	{
		if (!activeTags.Contains(tag))
		{
			activeTags.Add(tag);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagDisabled(string tag)
	{
		activeTags.Remove(tag);
	}

	public static bool IsTagActive(string tag)
	{
		return activeTags.Contains(tag);
	}
}

/// <summary>
/// This static class only contain default active tags.
/// You can use any string as tag.
/// </summary>
public static class WPTag
{
	// Special tag that always display a log
	public static string FORCE = "F";

	// Examples of usable tag
	public static string INFO = "INFO";
	public static string IMPORTANT = "IMP";
	public static string SERVER = "SERVER";
	public static string CLIENT = "CLIENT";
	public static string NETWORK = "NETWORK";
	public static string STATS = "STATS";
	public static string PLAYER = "PLAYER";
	public static string UI = "UI";
}
