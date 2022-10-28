using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class WPLoggerEditor : EditorWindow
{
	Vector2 liveScrollView;
	string liveAddTag;
	public void GUILive()
	{
		if (EditorApplication.isPlaying)
		{
			GUIRunning();
		}
		else
		{
			EditorGUILayout.HelpBox("This display WPLogger settings (Editable when running)", MessageType.Info);
			EditorGUILayout.LabelField("LogToUnity", WPLogger.LogToUnity.ToString());
			EditorGUILayout.LabelField("LogToHistory", WPLogger.LogToHistory.ToString());
			EditorGUILayout.LabelField("LogTagHeader", WPLogger.LogTagHeader.ToString());
			EditorGUILayout.LabelField("LogTime", WPLogger.LogTime.ToString());
			EditorGUILayout.Popup("Tag List", 0, WPLogger.GetTags());
		}
	}

	public void GUIRunning()
	{
		liveScrollView = EditorGUILayout.BeginScrollView(liveScrollView);
		WPLogger.LogToUnity = EditorGUILayout.Toggle("LogToUnity", WPLogger.LogToUnity);
		WPLogger.LogToHistory = EditorGUILayout.Toggle("LogToHistory", WPLogger.LogToHistory);
		WPLogger.LogTagHeader = EditorGUILayout.Toggle("LogTagHeader", WPLogger.LogTagHeader);
		WPLogger.LogTime = EditorGUILayout.Toggle("LogTime", WPLogger.LogTime);
		GUILayout.Space(5);
		EditorGUILayout.LabelField("TAG LIST");
		for (int i = 0; i < WPLogger.activeTags.Count; i++)
		{
			EditorGUILayout.BeginHorizontal("Box");
			EditorGUILayout.LabelField(i + " - " + WPLogger.activeTags[i]);
			if (WPLogger.activeTags[i] != "F" && GUILayout.Button("X", GUILayout.Width(35)))
			{
				WPLogger.SetTagDisabled(WPLogger.activeTags[i]);
				i--;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.BeginHorizontal();
		liveAddTag = EditorGUILayout.TextField(liveAddTag);
		if (GUILayout.Button("Add", GUILayout.Width(55)))
		{
			WPLogger.SetTagActive(liveAddTag);
			liveAddTag = "";
		}
		EditorGUILayout.EndHorizontal();
	}
}