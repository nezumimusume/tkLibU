using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tkLibU_FPSDisplay : MonoBehaviour
{
    // 変数
    int frameCount;
    float prevTime;
    float fps;

    // 初期化処理
    void Start()
    {
        Application.targetFrameRate = 60;
        // 変数の初期化
        frameCount = 0;
        prevTime = 0.0f;
    }

    // 更新処理
    void Update()
    {
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 0.5f)
        {
            fps = frameCount / time;
            Debug.Log(fps);

            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
    }

    // 表示処理
    private void OnGUI()
    {
        var styleState = new GUIStyleState();
        styleState.textColor = Color.white;
        var style = new GUIStyle();
        style.fontSize = 50;
        style.normal = styleState;
        GUILayout.BeginArea(new Rect(50, 50, 400, 150));
        string text = string.Format("FPS = {0:F2}", fps);
        GUILayout.Label(text, style);
        string useAPIName = string.Format("API = {0}", SystemInfo.graphicsDeviceType.ToString());
        GUILayout.Label(useAPIName, style);
        GUILayout.EndArea();
    }
}
