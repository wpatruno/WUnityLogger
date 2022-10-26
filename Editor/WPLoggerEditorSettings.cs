using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class WPLoggerEditor : EditorWindow
{
	const string DATA_FILE_EXTENSION = ".asset";
	List<string> popupTags;
	int settingsTab;
	int _tempSettingsTab = -1;
	int tempIndexDefTag;

	WPLoggerData loggerData;
	SerializedObject serializedObject;
	SerializedProperty serialStripping;
	SerializedProperty serialSetting;
	SerializedProperty serialActiveList;
	SerializedProperty serialLogToUnity;
	SerializedProperty serialLogToHistory;
	SerializedProperty serialDisplayTagHeader;
	SerializedProperty serialLogTime;

	/// <summary>
	/// Load WPLoggerData and setup data path
	/// </summary>
	public void LoadSettings()
	{
		loggerData = Resources.Load<WPLoggerData>(WPLoggerData.DATA_FILE_NAME);
		if (loggerData)
		{
			serializedObject = new SerializedObject(loggerData);
			WPLoggerData.GetCurrentSettings();
			RefreshSerialized();

			if (!PlayerPrefs.HasKey(EDITORKEY_PATH))
			{
				localDataPath = AssetDatabase.GetAssetPath(loggerData);
				if (!string.IsNullOrWhiteSpace(localDataPath))
				{
					List<string> splitted = new List<string>(localDataPath.Split('/'));
					splitted.RemoveAt(splitted.Count - 1); // remove file
					splitted.RemoveAt(splitted.Count - 1); // remove resource folder
					localDataPath = string.Join('/', splitted);
					WPLogger.Log(localDataPath);
					PlayerPrefs.SetString(EDITORKEY_PATH, localDataPath);
				}
			}
			else
			{
				localDataPath = PlayerPrefs.GetString(EDITORKEY_PATH);
			}
		}
		else
		{
			localDataPath = DEFAULT_DATA_FILE_PATH;
		}
	}



	/// <summary>
	/// Set data path and create WPloggerData 
	/// </summary>
	void GUIDataCreation()
	{
		EditorGUILayout.HelpBox("Select a path inside Assets folder where WPLogger generated files will be created !", MessageType.Warning);
		EditorGUILayout.HelpBox("Generate 1 Scriptable file for settings", MessageType.Info);
		EditorGUILayout.HelpBox("(Optional) Generate 1 text file and 1 script file for custom tags", MessageType.Info);
		GUILayout.Space(10);
		localDataPath = EditorGUILayout.TextField("Resources Folder Path:", localDataPath);
		GUILayout.Space(10);
		if (GUILayout.Button("Create"))
		{
			CreateDataAsset(localDataPath);
		}
	}

	/// <summary>
	/// Settings Menu selection
	/// </summary>
	void GUISettingMenu()
	{
		settingsTab = GUILayout.SelectionGrid(settingsTab, new string[] { "Editor", "Dev", "Release" }, 3, EditorStyles.toolbarButton);

		if (_tempSettingsTab != settingsTab)
		{
			_tempSettingsTab = settingsTab;
			RefreshSerialized();
		}
		GUISettingDisplay();
	}

	/// <summary>
	/// Display selected settings
	/// </summary>
	void GUISettingDisplay()
	{
		if (GUILayout.Button("APPLY TO LIVE", GUILayout.Width(110))) ApplyCurrentToLive();
		EditorGUILayout.PropertyField(serialLogToUnity);
		EditorGUILayout.PropertyField(serialLogToHistory);
		EditorGUILayout.PropertyField(serialDisplayTagHeader);
		EditorGUILayout.PropertyField(serialLogTime);

		EditorGUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("ACTIVE TAG LIST");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("COPY", GUILayout.Width(60))) CopyListToClipboard();
		if (GUILayout.Button("PASTE", GUILayout.Width(60))) PastClipboardToList();

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(serialActiveList);

		EditorGUILayout.BeginHorizontal();
		tempIndexDefTag = EditorGUILayout.Popup(tempIndexDefTag, popupTags.ToArray());
		if (GUILayout.Button("ADD", GUILayout.Width(50)))
		{
			AddTagToActive(popupTags[tempIndexDefTag]);
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

	void ApplyCurrentToLive()
	{
		WPLogger.ApplySettings(GetCurrentSettings());
		WPLogger.Log("Apply to live settings", WPLoggerData.TAG);
	}

	WPLoggerData.Settings GetCurrentSettings()
	{
		List<string> arr = new List<string>();
		for (int i = 0; i < serialActiveList.arraySize; i++)
		{
			arr.Add(serialActiveList.GetArrayElementAtIndex(i).stringValue);
		}
		return new WPLoggerData.Settings()
		{
			logToUnity = serialLogToUnity.boolValue,
			logToHistory = serialLogToHistory.boolValue,
			logTagHeader = serialDisplayTagHeader.boolValue,
			logTime = serialLogTime.boolValue,
			defaultActiveTags = arr.ToArray()
		};
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



	void RefreshSerialized()
	{
		serialStripping = serializedObject.FindProperty("logMode");
		tempStrippingMode = serialStripping.intValue;

		if (settingsTab == 0)
		{
			serialSetting = serializedObject.FindProperty("editorSettings");
		}
		else if (settingsTab == 1)
		{
			serialSetting = serializedObject.FindProperty("devBuildSettings");
		}
		else
		{
			serialSetting = serializedObject.FindProperty("releaseBuildSettings");
		}

		serialLogToUnity = serialSetting.FindPropertyRelative("logToUnity");
		serialLogToHistory = serialSetting.FindPropertyRelative("logToHistory");
		serialDisplayTagHeader = serialSetting.FindPropertyRelative("logTagHeader");
		serialLogTime = serialSetting.FindPropertyRelative("logTime");

		serialActiveList = serialSetting.FindPropertyRelative("defaultActiveTags");
		if (serialActiveList.arraySize == 0)
		{
			AddTagToActive(WPMainTag.INFO);
			AddTagToActive(WPMainTag.WARNING);
			AddTagToActive(WPMainTag.IMPORTANT);
			AddTagToActive(WPLoggerData.TAG);
		}
	}

	void CreateDataAsset(string path)
	{
		if (!path.Contains("Assets"))
		{
			WPLogger.LogError("The path is not inside Assets folder !");
		}
		if (!path.EndsWith('/') && !path.EndsWith('\\')) path += "/";

		try
		{
			Directory.CreateDirectory(path);
			PlayerPrefs.SetString(EDITORKEY_PATH, path);
			string resPath = path + "Resources/";
			Directory.CreateDirectory(resPath);
			loggerData = new WPLoggerData();
			AssetDatabase.CreateAsset(loggerData, resPath + WPLoggerData.DATA_FILE_NAME + DATA_FILE_EXTENSION);
			AssetDatabase.SaveAssetIfDirty(loggerData);
			RefreshSerialized();
		}
		catch (System.Exception e)
		{
			WPLogger.LogError("WPLogger data creation failed with error:\n" + e.Message, WPLoggerData.TAG);
		}
	}
}