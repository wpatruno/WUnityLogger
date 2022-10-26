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

public class WPLoggerEditor : EditorWindow
{
	public const string CONDITIONAL = "WPLOG";
	const string CSC_FILE = "Assets/csc.rsp";
	const string CSC_LINE = "-define:";
	const string DEFAULT_DATA_FILE_PATH = "Assets/Tools/WPLoggerData/";
	const string DATA_FILE_EXTENSION = ".asset";
	const string EDITORKEY_PATH = "WPLogPath";
	static string[] DEFAULT_TAGS = new string[] { WPMainTag.INFO, WPMainTag.WARNING, WPMainTag.IMPORTANT, WPLoggerData.TAG };
	List<string> customTags;
	string localDataPath;
	string newFilePath;
	int mainTab;
	int settingsTab;
	WPLoggerData loggerData;
	int tempStrippingMode;
	int _tempSettingsTab = -1;
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
		LoadCustomTags();
		var a = WPLoggerEditorUtility.GetAssemblyAttribute<WPLoggerTag>(System.AppDomain.CurrentDomain);
		if (a != null)
			Debug.Log(a.ToString());
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
			settingsTab = GUILayout.SelectionGrid(settingsTab, new string[] { "Editor", "Dev", "Release" }, 3, EditorStyles.toolbarButton);

			if (_tempSettingsTab != settingsTab)
			{
				_tempSettingsTab = settingsTab;
				RefreshSerialized();
			}
			GUISettingDisplay();
		}
		else if (mainTab == 1)
		{
			// Display custom Tags
			GUITags();
		}

		if (serializedObject.ApplyModifiedProperties())
		{
			WPLoggerData.GetCurrentSettings();
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

	void ApplyCurrentToLive()
	{

		WPLogger.Log("Apply to live", WPLoggerData.TAG);
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
		EditorGUILayout.HelpBox("Select a path inside Assets folder where WPLogger generated files will be created !", MessageType.Warning);
		EditorGUILayout.HelpBox("Generate 1 Scriptable file for settings", MessageType.Info);
		EditorGUILayout.HelpBox("(Optional) Generate 1 text file and 1 script file for custom tags", MessageType.Info);
		GUILayout.Space(10);
		newFilePath = EditorGUILayout.TextField("Resources Folder Path:", newFilePath);
		GUILayout.Space(10);
		if (GUILayout.Button("Create"))
		{
			CreateDataAsset(newFilePath);
		}
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

	string newTag;
	Vector2 tagScrollView;
	const string TAG_TEXT_FILE_NAME = "tags.txt";
	const string TAG_SCRIPT_FILE_NAME = "WPTag.cs";
	void GUITags()
	{
		tagScrollView = EditorGUILayout.BeginScrollView(tagScrollView);
		if (customTags.Count == 0)
		{
			EditorGUILayout.HelpBox("Creating custom tags here is optionnal, this will generate a new static script file with tags as [const string] variables.\n"
						+ "You can skip this page and create your own script/variables or just use simple strings.", MessageType.Info);
		}
		else
		{
			EditorGUILayout.HelpBox("To use your custom tag:  WPTag.YOURTAG", MessageType.Info);
		}
		for (int i = 0; i < customTags.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			customTags[i] = EditorGUILayout.TextField(customTags[i]);
			if (GUILayout.Button("X", GUILayout.Width(50)))
			{
				customTags.RemoveAt(i);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		EditorGUILayout.BeginHorizontal();
		newTag = EditorGUILayout.TextField(newTag);
		if (GUILayout.Button("Add", GUILayout.Width(80)))
		{
			if (!customTags.Contains(newTag) && !string.IsNullOrWhiteSpace(newTag))
				customTags.Add(newTag);
			newTag = "";
		}
		EditorGUILayout.EndHorizontal();
		if (GUILayout.Button("Save"))
		{
			SaveCustomTags();
		}
	}

	void LoadCustomTags()
	{
		if (File.Exists(localDataPath + "/" + TAG_TEXT_FILE_NAME))
		{
			try
			{
				customTags = new List<string>(File.ReadAllLines(localDataPath + "/" + TAG_TEXT_FILE_NAME));
			}
			catch (System.Exception e)
			{
				WPLogger.LogError(e.Message);
			}
		}
		else
		{
			customTags = new List<string>();
		}
	}

	void SaveCustomTags()
	{
		try
		{
			File.WriteAllLines(localDataPath + "/" + TAG_TEXT_FILE_NAME, customTags);

			CreateScriptTags();
			AssetDatabase.Refresh();
		}
		catch (System.Exception e)
		{
			WPLogger.LogError(e.Message);
		}
	}

	void CreateScriptTags()
	{
		string varLine = "\tpublic const string ";
		List<string> fileLines = new List<string>();
		fileLines.Add("public static class WPTag");
		fileLines.Add("{");
		foreach (var tag in customTags)
		{
			string varName = tag.Replace(' ', '_').ToUpper();
			fileLines.Add(varLine + varName + " = \"" + tag + "\";");
		}

		fileLines.Add("}");
		try
		{
			File.WriteAllLines(localDataPath + "/" + TAG_SCRIPT_FILE_NAME, fileLines);
			AssetDatabase.ImportAsset(localDataPath + "/" + TAG_SCRIPT_FILE_NAME, ImportAssetOptions.ForceUpdate);
		}
		catch (System.Exception e)
		{
			WPLogger.LogError(e.Message);
		}
	}
}