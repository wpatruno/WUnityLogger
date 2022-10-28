using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DebugWPLog
{
	public class DebugUI : MonoBehaviour
	{
		const string FILE_NAME = "WPLOG";
		const string EXTENSION = ".txt";

		[SerializeField]
		GameObject canvas;
		[SerializeField]
		ScrollRect scroll;
		[SerializeField]
		Toggle toggleGoDown;
		[SerializeField]
		Text debugText;
		bool isDown;

		void Start()
		{
			debugText.text = "*** *** *** START - " + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + " *** *** ***" + Environment.NewLine;
			canvas.SetActive(false);
			WPLogger.OnLogged += OnNewLog;
			WPLogger.OnErrorLogged += OnNewLogError;
		}

		void Update()
		{
#if ENABLE_INPUT_SYSTEM

			if (Keyboard.current.f12Key.isPressed)
			{
				if (!isDown) canvas.SetActive(!canvas.activeSelf);
				isDown = true;
			}
			else
			{
				isDown = false;
			}

			//if (Keyboard.current.f11Key.isPressed) WPLogger.Log("TEST LOG TEST LOG");

#else

			if (Input.GetKeyDown(KeyCode.F12))
			{
				canvas.SetActive(!canvas.activeSelf);
			}

			//if (Input.GetKey(KeyCode.F11)) WPLogger.Log("TEST LOG TEST LOG");

#endif
		}

		void OnNewLog(string logText)
		{
			debugText.text += logText + Environment.NewLine;
			if (toggleGoDown.isOn)
				scroll.verticalNormalizedPosition = 0f;
		}

		void OnNewLogError(string logText)
		{
			debugText.text += "<color=red>" + logText + "</color>" + Environment.NewLine;
			if (toggleGoDown.isOn)
				scroll.verticalNormalizedPosition = 0f;
		}

		public void Clear()
		{
			debugText.text = "..." + Environment.NewLine;
			WPLogger.Clear();
		}

		public void Save(bool openFolder = false)
		{
			try
			{
				string filepath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + FILE_NAME + EXTENSION;
				System.IO.Directory.CreateDirectory(Application.persistentDataPath);
				System.IO.File.WriteAllText(filepath, WPLogger.LogHistory.ToString());
				WPLogger.Log("Log file created at -> " + filepath, WPMainTag.FORCE, "DEBUG");
				if (openFolder) OpenSaveFolder();
			}
			catch (System.Exception e)
			{
				WPLogger.LogError(e.Message);
			}
		}

		public void OpenSaveFolder()
		{
#if UNITY_STANDALONE_WIN
			Process.Start("explorer.exe", Application.persistentDataPath.Replace('/', '\\'));
#else
			WPLogger.Error("Open folder only available on Windows for now");
#endif
		}
	}
}