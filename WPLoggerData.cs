using System;
using UnityEngine;

/// <summary>
/// Settings of WPLogger for all platforms
/// </summary>
public class WPLoggerData : ScriptableObject
{
	public const string DATA_FILE_NAME = "WPLoggerData";
	public const string TAG = "WPLOG";

	public enum LogMode
	{
		NoRelease,
		Full,
	}

	[System.Serializable]
	public struct Settings
	{
		public string[] defaultActiveTags;
		public bool logToUnity;
		public bool logToHistory;
		public bool logTagHeader;
		public bool logTime;
	}

	// Remove useless data from release build
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	[SerializeField] LogMode logMode = LogMode.NoRelease;
	[SerializeField] Settings editorSettings;
	[SerializeField] Settings devBuildSettings;
#endif
	[SerializeField] Settings releaseBuildSettings;

	public Settings GetSettings()
	{
#if UNITY_EDITOR
		return editorSettings;
#elif DEVELOPMENT_BUILD
        return devBuildSettings;
#else
        return releaseBuildSettings;
#endif
	}

	public static Settings GetCurrentSettings()
	{
		WPLoggerData data = Resources.Load<WPLoggerData>(DATA_FILE_NAME);
		return data.GetSettings();
	}
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class WPLoggerTag : Attribute
{
	public WPLoggerTag(int b) { Bar = b; }
	public int Bar { get; set; }
	//Class Members
}
