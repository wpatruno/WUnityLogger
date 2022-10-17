using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
	public GameObject canvas;
	public ScrollRect scroll;
	public Text debugText;
	bool isDown;

	void Start()
	{

		WPLogger.debugTextUI = debugText;
		canvas.SetActive(false);
		WPLogger.OnLogged += ScrollDown;
	}

	void Update()
	{
		if (Keyboard.current.f1Key.isPressed)
		{
			if (!isDown) canvas.SetActive(!canvas.activeSelf);
			isDown = true;
		}
		else
		{
			isDown = false;
		}
	}

	public void ScrollDown(string n)
	{
		scroll.verticalNormalizedPosition = 0f;
	}
}
