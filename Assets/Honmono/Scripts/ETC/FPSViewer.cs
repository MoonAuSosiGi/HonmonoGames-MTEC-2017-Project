using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSViewer : MonoBehaviour {

    float delta_time = 0.0f;

    void Update()
    {
        delta_time += (Time.deltaTime - delta_time) * 0.1f;
    }

    void OnGUI()
    {
        int screen_width = Screen.width;
        int screen_height = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, 100, 50);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = screen_height * 2 / 100;
        style.normal.textColor = Color.red;
        float msec = delta_time * 1000.0f;
        float fps = 1.0f / delta_time;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }

}
