using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WPLoggerEditor : EditorWindow
{
	public const string CONDITIONAL = "WPLOG";
	const string CSC_FILE = "Assets/csc.rsp";
	const string CSC_LINE = "-define:";
	const string DEFAULT_DATA_FILE_PATH = "Assets/Resources/";
	const string DATA_FILE_EXTENSION = ".asset";

	static string[] DEFAULT_TAGS = new string[] { WPTag.INFO, WPTag.WARNING, WPTag.IMPORTANT, WPLoggerData.TAG };

	string newFilePath;
	int windowsTab;
	WPLoggerData loggerData;
	int tempStrippingMode;
	int tempWindowsTab;
	int tempIndexDefTag;

	SerializedObject serializedObject;
	SerializedProperty serialStripping;
	SerializedProperty serialSetting;
	SerializedProperty serialActiveList;
	SerializedProperty serialLogToUnity;
	SerializedProperty serialLogToHistory;
	SerializedProperty serialDisplayTagHeader;
	SerializedProperty serialLogTime;


	[MenuItem("Tools/WPLogger")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<WPLoggerEditor>();
	}

	void OnEnable()
	{
		titleContent = new GUIContent("WP-LOGGER", "Homemade logging tool");
		loggerData = Resources.Load<WPLoggerData>(WPLoggerData.DATA_FILE_NAME);
		newFilePath = DEFAULT_DATA_FILE_PATH;
		serializedObject = new SerializedObject(loggerData);
		WPLoggerData.GlobalApply();
		RefreshSerialized();
	}

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
		windowsTab = GUILayout.SelectionGrid(windowsTab, new string[] { "Editor", "Editor Runtime", "Dev", "Release" }, 4, EditorStyles.toolbarButton);
		if (GUILayout.Button("Exit", EditorStyles.toolbarButton, GUILayout.Width(50)))
		{
			EditorGUILayout.EndHorizontal();
			this.Close();
			return;
		}
		EditorGUILayout.EndHorizontal();

		// Select the active settings tab

		if (tempWindowsTab != windowsTab)
		{
			tempWindowsTab = windowsTab;
			if (windowsTab != 0) RefreshSerialized();
		}

		// Display the selected settings
		if (windowsTab == 0)
		{
			EditorGUILayout.HelpBox("This only display current WPLogger settings in Editor", MessageType.Info);
			EditorGUILayout.LabelField("LogToUnity", WPLogger.LogToUnity.ToString());
			EditorGUILayout.LabelField("LogToHistory", WPLogger.LogToHistory.ToString());
			EditorGUILayout.LabelField("DisplayTagHeader", WPLogger.DisplayTagHeader.ToString());
			EditorGUILayout.LabelField("LogTime", WPLogger.LogTime.ToString());
			EditorGUILayout.Popup("Tag List", 0, WPLogger.GetTags());
		}
		else if (serialSetting != null)
		{
			GUISettingDisplay();
		}

		if (serializedObject.ApplyModifiedProperties())
		{
			WPLoggerData.GlobalApply();
			// refresh CSC file if logMode changed
			if (tempStrippingMode != serialStripping.intValue)
			{
				tempStrippingMode = serialStripping.intValue;
				UpdateCSCFile(serialStripping.intValue);
			}
		}
	}

	void GUISettingDisplay()
	{
		EditorGUILayout.PropertyField(serialLogToUnity);
		EditorGUILayout.PropertyField(serialLogToHistory);
		EditorGUILayout.PropertyField(serialDisplayTagHeader);
		EditorGUILayout.PropertyField(serialLogTime);

		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField("TAG ACTIVE LIST");
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("COPY", GUILayout.Width(60))) CopyListToClipboard();
		if (GUILayout.Button("PASTE", GUILayout.Width(60))) PastClipboardToList();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(serialActiveList);

		EditorGUILayout.BeginHorizontal();
		tempIndexDefTag = EditorGUILayout.Popup(tempIndexDefTag, DEFAULT_TAGS);
		if (GUILayout.Button("ADD", GUILayout.Width(50)))
		{
			AddTagToActive(DEFAULT_TAGS[tempIndexDefTag]);
		}
		EditorGUILayout.EndHorizontal();
	}

	void CopyListToClipboard()
	{
		string list = "";
		for (int i = 0; i < serialActiveList.arraySize; i++)
		{
			var line = serialActiveList.GetArrayElementAtIndex(i);
			list += line.stringValue + ",";
		}
		WPLogger.Log("Tag list copyed to clipboard");
		GUIUtility.systemCopyBuffer = list;
	}

	void PastClipboardToList()
	{
		string[] list = GUIUtility.systemCopyBuffer.Split(',');
		foreach (var tag in list)
		{
			if (string.IsNullOrWhiteSpace(tag)) continue;
			AddTagToActive(tag);
		}
		WPLogger.Log("Tag clipboard added to list");
	}

	void AddTagToActive(string tag)
	{
		for (int i = 0; i < serialActiveList.arraySize; i++)
		{
			if (serialActiveList.GetArrayElementAtIndex(i).stringValue == tag) return;
		}
		serialActiveList.InsertArrayElementAtIndex(serialActiveList.arraySize);
		var line = serialActiveList.GetArrayElementAtIndex(serialActiveList.arraySize - 1);
		line.stringValue = tag;
	}

	void GUIDataCreation()
	{
		EditorGUILayout.LabelField("Missing WPLogger Data !");
		newFilePath = EditorGUILayout.TextField("Resources Folder Path:", newFilePath);
		if (GUILayout.Button("Create"))
		{
			CreateDataAsset(newFilePath);
		}
	}

	void RefreshSerialized()
	{
		serialStripping = serializedObject.FindProperty("logMode");
		tempStrippingMode = serialStripping.intValue;

		if (windowsTab == 1)
		{
			serialSetting = serializedObject.FindProperty("editorSettings");
		}
		else if (windowsTab == 2)
		{
			serialSetting = serializedObject.FindProperty("devBuildSettings");
		}
		else
		{
			serialSetting = serializedObject.FindProperty("releaseBuildSettings");
		}

		serialLogToUnity = serialSetting.FindPropertyRelative("logToUnity");
		serialLogToHistory = serialSetting.FindPropertyRelative("logToHistory");
		serialDisplayTagHeader = serialSetting.FindPropertyRelative("displayTagHeader");
		serialLogTime = serialSetting.FindPropertyRelative("logTime");

		serialActiveList = serialSetting.FindPropertyRelative("defaultActiveTags");
		if (serialActiveList.arraySize == 0)
		{
			AddTagToActive(WPTag.INFO);
			AddTagToActive(WPTag.WARNING);
			AddTagToActive(WPTag.IMPORTANT);
			AddTagToActive(WPLoggerData.TAG);
		}
	}

	void CreateDataAsset(string path)
	{
		if (!path.EndsWith('/') && !path.EndsWith('\\')) path += "/";
		if (!path.EndsWith("Resources/") && !path.EndsWith(@"Resources\")) path += "Resources/";

		try
		{
			Directory.CreateDirectory(path);
			loggerData = new WPLoggerData();
			AssetDatabase.CreateAsset(loggerData, path + WPLoggerData.DATA_FILE_NAME + DATA_FILE_EXTENSION);
			AssetDatabase.SaveAssetIfDirty(loggerData);
			RefreshSerialized();
		}
		catch (System.Exception e)
		{
			WPLogger.Error("WPLogger data creation failed with error:\n" + e.Message, WPLoggerData.TAG);
		}
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
			foreach (var item in linesToKeep) WPLogger.Log(item);
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
