using UnityEngine;
using System.Collections;

public class DebugPanel : MonoBehaviour {
    public Rect m_ButtonRect = new Rect(700, 0, 100, 50);
    public Rect m_StatisticsButton = new Rect(900, 0, 100, 50);
    public int m_BordTop = 20;
    public int m_BordBottom = 10;
    public int m_BordLeftRight = 300;
    public int m_FontSize = 40;
    public int m_DesignScreenWidth = 1280;

    //---------------------------------------------------------------------------
    long m_FrameCount = 0;
    long m_LastTime = 0;
    int m_LastFPS = 0;
    int m_UpdateCount = 0;

    bool m_ShowDebugPanel = false;
    //GlowEffect m_GlowEffectScript;

    //---------------------------------------------------------------------------
    // Detail info
    int m_MaxFps = -1;
    int m_MinFps = 65535;
    int m_averageFps = 0;
    int m_TotalFps = 0;
    int m_UpdateStatisticsCount = 0;
    long m_TestTime = 0;
    long m_StartStatisticsTime = 0;
    long m_CurrentMaxPerFrameTime = 0;
    long m_MaxPerFrameTime = 0;
    long m_LastFrameTime = 0;
    bool m_UpdateLastFrameTime = false;
    bool m_ShowDetailInfo = false;

    ////////////////////////////////////////////////////////////////////////////
    // Use this for initialization
    void Start() {
        //if (Camera.main)
        //    m_GlowEffectScript = Camera.main.GetComponent<GlowEffect>();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Update is called once per frame
    void Update() {
        m_FrameCount++;

        long currentTime = System.DateTime.Now.Ticks / 10000; //10,000 ticks in a millisecond.
        if (m_LastTime == 0)
            m_LastTime = currentTime;

        long PerFrameTime = currentTime - m_LastFrameTime;
        m_LastFrameTime = currentTime;
        if (m_UpdateLastFrameTime && PerFrameTime > m_CurrentMaxPerFrameTime)
        {
            m_CurrentMaxPerFrameTime = PerFrameTime;
        }
        if (m_UpdateLastFrameTime == false)
        {
            m_UpdateLastFrameTime = true;
        }

        long deltaTime = currentTime - m_LastTime;
        if (deltaTime >= 1000) {
            int fps = Mathf.RoundToInt(m_FrameCount * 1000.0f / deltaTime);

            if (m_ShowDetailInfo)
            {
                if (fps > m_MaxFps)
                    m_MaxFps = fps;
                if (fps < m_MinFps)
                    m_MinFps = fps;

                m_TotalFps += fps;
                m_UpdateStatisticsCount++;
            }

            m_LastFPS = fps;
            m_FrameCount = 0;
            m_LastTime = currentTime;
            m_UpdateCount++;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    void OnGUI() {
        float UIScale = (float)Screen.width / m_DesignScreenWidth;

        //----------------------- button ---------------------------
        int OldFontSize = GUI.skin.button.fontSize;
        GUI.skin.button.fontSize = (int)(22 * UIScale);
        {
            Rect buttonPosition = m_ButtonRect;
            buttonPosition.width = buttonPosition.width * UIScale;
            buttonPosition.height = buttonPosition.height * UIScale;
            buttonPosition.x = buttonPosition.x * UIScale;
            buttonPosition.y = buttonPosition.y * UIScale;

            string buttonText = "调试面板\n";
            //string buttonText = GameLogic.GameLogicManager.Instance().debug_string;
            buttonText += "Time:" + System.DateTime.Now.ToString("HH:mm:ss") + "\n";
            buttonText += "FPS:" + (m_LastFPS > 0 ? m_LastFPS.ToString() : "--");
            int dotCount = m_UpdateCount % 3 + 1;
            for (int i = 0; i < dotCount; i++)
                buttonText += ".";

            if (GUI.Button(buttonPosition, buttonText))
                m_ShowDebugPanel = !m_ShowDebugPanel;
        }
        GUI.skin.button.fontSize = OldFontSize;

        ShowStatisticsButton();

        //------------------------ deubg panel  -------=-------------
        if (m_ShowDebugPanel) {
            SetGUISkinFontSize((int)(m_FontSize * UIScale));

            Rect rect = new Rect(
                m_BordLeftRight * UIScale,
                m_BordTop * UIScale,
                Screen.width - m_BordLeftRight * UIScale * 2,
                Screen.height - (m_BordTop + m_BordBottom) * UIScale
            );

            GUILayout.BeginArea(rect, "调试面板", GUI.skin.window);
            ShowDebugPanelWindow();
            GUILayout.EndArea();
            SetGUISkinFontSize(OldFontSize);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    void ShowDebugPanelWindow() {
        Vector2 CharSize = GUI.skin.label.CalcSize(new GUIContent("M"));
        var LabelMinHeight = GUILayout.MinHeight(CharSize.y);
        var DisableExtend = GUILayout.ExpandWidth(false);

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("屏幕：" + Screen.width + "X" + Screen.height, LabelMinHeight);
            if (GUILayout.Button("×", GUILayout.MinWidth(CharSize.x * 1.5f), DisableExtend))
                m_ShowDebugPanel = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(
                "质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()],
                LabelMinHeight,
                GUILayout.MinWidth(CharSize.x * 10),
                DisableExtend
            );
            /*
            if (GUILayout.Button("　-　", DisableExtend))
                QualitySettings.DecreaseLevel();

            if (GUILayout.Button("　+　", DisableExtend))
                QualitySettings.IncreaseLevel();
            */

        }
        GUILayout.EndHorizontal();

        //GUILayout.BeginVertical();
        //if (m_GlowEffectScript)
        //    m_GlowEffectScript.enabled = GUILayout.Toggle(m_GlowEffectScript.enabled, "Glow Effect");
        //GUILayout.EndVertical();

        //GUILayout.BeginVertical();
        //GUILayout.Label("编译时间：" + BuildInformation.GetBuildTimestamp(), LabelMinHeight);
        //GUILayout.EndVertical();

        if (m_ShowDetailInfo == false && m_UpdateStatisticsCount > 0)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("详细统计信息：", LabelMinHeight);
            GUILayout.Label("   平均FPS：" + m_averageFps, LabelMinHeight);
            GUILayout.Label("   最低FPS：" + m_MinFps, LabelMinHeight);
            GUILayout.Label("   最高FPS：" + m_MaxFps, LabelMinHeight);
            GUILayout.Label("   最大帧间隔时间：" + m_MaxPerFrameTime + "ms", LabelMinHeight);
            GUILayout.Label("   测试持续时间：" + m_TestTime + "s", LabelMinHeight);
            GUILayout.EndVertical();
        }                    
    }

    void SetGUISkinFontSize(int fontSize) {
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.toggle.fontSize = fontSize;
    }

    void ShowStatisticsButton()
    {
        float UIScale = (float)Screen.width / m_DesignScreenWidth;

        int OldFontSize = GUI.skin.button.fontSize;
        GUI.skin.button.fontSize = (int)(22 * UIScale);
        Rect buttonPosition = m_StatisticsButton;
        buttonPosition.x *= UIScale;
        buttonPosition.y *= UIScale;
        buttonPosition.width *= UIScale;
        buttonPosition.height *= UIScale;

        string buttonText = m_ShowDetailInfo ? "停止统计" : "开始统计";

        if (GUI.Button(buttonPosition, buttonText))
        {
            if (m_ShowDetailInfo == false)
            { // Start
                m_averageFps = 0;
                m_TotalFps = 0;
                m_MinFps = 65535;
                m_MaxFps = -1;
                m_CurrentMaxPerFrameTime = 0;
                m_UpdateStatisticsCount = 0;
                m_TestTime = 0;

                m_StartStatisticsTime = System.DateTime.Now.Ticks / 10000000; // 10,000,000 ticks in a second.
            }
            else
            { // End
                long currentTime = System.DateTime.Now.Ticks / 10000000;
                m_TestTime = currentTime - m_StartStatisticsTime;

                m_averageFps = m_UpdateStatisticsCount > 0 ? m_TotalFps / m_UpdateStatisticsCount : 0;
                m_MaxPerFrameTime = m_CurrentMaxPerFrameTime;
            }

            m_ShowDetailInfo = !m_ShowDetailInfo;
        }

        GUI.skin.button.fontSize = OldFontSize;
    }
}
