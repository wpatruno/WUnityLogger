using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public static class WPLoggerEditorUtility
{
	public static T GetAssemblyAttribute<T>(System.AppDomain appDomain) where T : Attribute
	{
		System.Reflection.Assembly[] assemblys = appDomain.GetAssemblies();
		foreach (var ass in assemblys)
		{
			//WPLogger.Log(ass.FullName);
			object[] attributes = ass.GetCustomAttributes(typeof(T), true);

			if (attributes == null || attributes.Length == 0)
				continue;

			foreach (var item in attributes)
			{
				//WPLogger.Log(item.ToString());
			}
			return attributes.OfType<T>().SingleOrDefault();
		}
		return null;
	}
}

public partial class WPLoggerEditor : EditorWindow
{
	public const string CONDITIONAL = "WPLOG";
	const string CSC_FILE = "Assets/csc.rsp";
	const string CSC_LINE = "-define:";
	const string DEFAULT_DATA_FILE_PATH = "Assets/Tools/WPLoggerData/";
	const string EDITORKEY_PATH = "WPLogPath";
	string localDataPath;
	int mainTab;
	int tempStrippingMode;


	[MenuItem("Tools/WPLogger")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<WPLoggerEditor>();
	}

	// Setup
	void OnEnable()
	{
		titleContent = new GUIContent("WP LOGGER", "Homemade logging tool");

		LoadSettings();
		LoadCustomTags();
		RefreshTagPopup();
		// Test code
		var a = WPLoggerEditorUtility.GetAssemblyAttribute<WPLoggerTag>(System.AppDomain.CurrentDomain);
		if (a != null)
			Debug.Log(a.ToString());
	}

	// Tick UI
	void OnGUI()
	{
		if (loggerData == null)
		{
			GUIDataCreation();
			return;
		}
		EditorGUILayout.BeginHorizontal("Box");
		EditorGUILayout.PropertyField(serialStripping);
		if (serialStripping.intValue == (int)WPLoggerData.LogMode.NoRelease)
		{
			EditorGUILayout.LabelField("Logs are removed from release build");
		}
		else
		{
			EditorGUILayout.LabelField("Logs are displayed in release build");
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		mainTab = GUILayout.SelectionGrid(mainTab, new string[] { "Live", "Tags", "Settings" }, 3, EditorStyles.toolbarButton);
		EditorGUILayout.EndHorizontal();

		// Select the active settings tab
		if (mainTab == 0)
		{
			// Display live WPLogger Settings
			EditorGUILayout.HelpBox("This display current WPLogger settings (Editable when running))", MessageType.Info);
			EditorGUILayout.LabelField("LogToUnity", WPLogger.LogToUnity.ToString());
			EditorGUILayout.LabelField("LogToHistory", WPLogger.LogToHistory.ToString());
			EditorGUILayout.LabelField("LogTagHeader", WPLogger.LogTagHeader.ToString());
			EditorGUILayout.LabelField("LogTime", WPLogger.LogTime.ToString());
			EditorGUILayout.Popup("Tag List", 0, WPLogger.GetTags());
		}
		else if (mainTab == 2)
		{
			// Display default settings
			GUISettingMenu();
		}
		else if (mainTab == 1)
		{
			// Display custom Tags
			GUITags();
		}

		if (serializedObject.ApplyModifiedProperties())
		{
			// refresh CSC file if logMode changed
			if (tempStrippingMode != serialStripping.intValue)
			{
				tempStrippingMode = serialStripping.intValue;
				UpdateCSCFile(serialStripping.intValue);
			}
		}
	}

	void RefreshTagPopup()
	{
		popupTags = new List<string> { WPMainTag.INFO, WPMainTag.WARNING, WPMainTag.IMPORTANT, WPLoggerData.TAG };
		popupTags.AddRange(customTags);
	}

	/// <summary>
	/// Create and modify CSC.RSP file that contain custom preprocessor directives
	/// </summary>
	/// <param name="mode"></param>
	void UpdateCSCFile(int mode)
	{
		// MAY NEED OPTIMISATION 
		if (mode == (int)WPLoggerData.LogMode.NoRelease)
		{
			// Remove line from CSC
			if (!File.Exists(CSC_FILE)) return;
			var linesToKeep = File.ReadLines(CSC_FILE).Where(line => !line.EndsWith(CONDITIONAL)).ToArray();
			File.WriteAllLines(CSC_FILE, linesToKeep);
		}
		else
		{
			// ADD LINE TO CSC IF NEEDED
			WPLogger.Log("Preprocessor directive removed: " + CONDITIONAL, WPLoggerData.TAG);
			var lines = new List<string>(File.ReadLines(CSC_FILE));

			foreach (var line in lines)
			{
				if (line.EndsWith(CONDITIONAL)) return;
			}
			lines.Add(CSC_LINE + CONDITIONAL);
			File.WriteAllLines(CSC_FILE, lines);
		}

		// FORCE REFRESH
		AssetDatabase.ImportAsset(CSC_FILE, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
	}
}