using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUILogTag : MonoBehaviour
{
	public GameObject prefabToggleTag;
	List<GameObject> objects = new List<GameObject>();
	void Start()
	{
		Create(WPTag.INFO);
		Create(WPTag.IMPORTANT);
		Create(WPTag.PLAYER);
		Create(WPTag.CLIENT);
		Create(WPTag.SERVER);
		Create(WPTag.STATS);
	}

	void Create(string tag)
	{
		GameObject obj = Instantiate(prefabToggleTag, transform);
		obj.GetComponentInChildren<Text>().text = tag;

		var toggle = obj.GetComponent<Toggle>();
		toggle.isOn = WPLogger.IsTagActive(tag);
		toggle.onValueChanged.AddListener((bool v) =>
		{
			if (v)
			{
				WPLogger.SetTagActive(tag);
			}
			else
			{
				WPLogger.SetTagDisabled(tag);
			}
		});
		objects.Add(obj);
	}

}
