using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPLog.Debug
{
	public class DebugUILogTag : MonoBehaviour
	{
		public GameObject prefabToggleTag;
		List<GameObject> objects = new List<GameObject>();

		protected virtual void Start()
		{
			Create(WPTag.INFO);
			Create(WPTag.WARNING);
			Create(WPTag.IMPORTANT);
			Create(WPTag.ANALYTIC);
			Create(WPTag.UI);
			Create(WPTag.PLAYER);
		}

		protected void Create(string tag)
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
}