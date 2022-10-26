
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class WPLoggerEditor : EditorWindow
{
	const string TAG_TEXT_FILE_NAME = "tags.txt";
	const string TAG_SCRIPT_FILE_NAME = "WPTag.cs";
	string newTag;
	Vector2 tagScrollView;
	List<string> customTags;

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
			RefreshTagPopup();
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