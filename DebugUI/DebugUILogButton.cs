using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUILogButton : MonoBehaviour
{
    public void Clear()
    {
        WPLogger.Clear();
    }

    public void Save()
    {
        InputField field = GetComponentInChildren<InputField>();
        if (field)
        {
            WPLogger.Save(field.text);
        }
        else
        {
            WPLogger.Save();
        }
    }
}
