using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameUtil
{
    public static void Log(string format, params object[] args)
    {
        _log = string.Format(format, args) + "\r\n" + _log;
        _logPosition.y = 0f;
    }

    public static string _log = "";
    public static Vector2 _logPosition = Vector2.zero;

    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(Math.Min(value, max), min);
    }

    public static int FontSize = 24;
}

public class GameInterface
{
    public static GameInterface Instance = new GameInterface();

    public static string[] ObjectNames = new string[] 
    { 
        "Environment/Models", 
        "Environment/Models/level_1", 
        "Environment/Models/level_2", 
        "Environment/Models/level_3", 
        "Environment/SceneEffect", 
        "Environment/Terrain", 
        "Main/UIMgr", 
        "Main/_Players", 
        "Main/AnimationEffects", 
    };

    public bool Init()
    {
        foreach (var name in ObjectNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                KeyNodes[name] = go;
            }
        }
        return true;
    }

    private void DoFilter<T>(List<T> visible, List<T> disabled, float percentage) where T : Renderer
    {
        int expectedCount = (int)((float)(visible.Count + disabled.Count) * percentage);
        if (visible.Count > expectedCount)
        {
            for (int i = 0; i < visible.Count - expectedCount; ++i)
            {
                var r = visible[i];
                r.enabled = false;
                disabled.Add(r);
                GameUtil.Log("{0} is hidden.", r.gameObject.name);
            }
        }
        else if (expectedCount > visible.Count)
        {
            for (int i = 0; i < expectedCount - visible.Count; ++i)
            {
                var r = disabled[i];
                r.enabled = true;
                disabled.Remove(r);
                GameUtil.Log("{0} is shown.", r.gameObject.name);
            }
        }
    }

    public void FilterVisibleObjects(float percentage, float psysPercent)
    {
        List<Renderer> VisibleRenderers = new List<Renderer>();
        List<ParticleSystemRenderer> VisiblePSysRenderers = new List<ParticleSystemRenderer>();
        foreach (Renderer r in UnityEngine.Object.FindObjectsOfType(typeof(Renderer)) as Renderer[])
        {
            if (r.isVisible && r.enabled)
            {
                if (r is ParticleSystemRenderer)
                {
                    VisiblePSysRenderers.Add((ParticleSystemRenderer)r);
                } 
                else
                {
                    VisibleRenderers.Add(r);
                }
            }
        }

        DoFilter<Renderer>(VisibleRenderers, DisabledRenderers, GameUtil.Clamp(percentage, 0.0f, 1.0f));
        DoFilter<ParticleSystemRenderer>(VisiblePSysRenderers, DisabledParticleSystems, GameUtil.Clamp(psysPercent, 0.0f, 1.0f));
    }

    public Dictionary<string, GameObject> KeyNodes = new Dictionary<string, GameObject>();

    public List<Renderer> DisabledRenderers = new List<Renderer>();
    public List<ParticleSystemRenderer> DisabledParticleSystems = new List<ParticleSystemRenderer>();
}

public class utest : IDisposable  
{
    public class FontSetter : IDisposable
    {
        public FontSetter()
        {
            GUI.skin.button.fontSize = GameUtil.FontSize;
        }

        public void Dispose()
        {
            GUI.skin.button.fontSize = m_oldSize;
        }

        int m_oldSize = GUI.skin.button.fontSize;
    }

    public bool m_enable = false;

    public float m_renderOrdinaryPercentage = 100.0f;
    public float m_renderParticlePercentage = 100.0f;

    public void Dispose()
    {
    }

    public void OnLevelWasLoaded()
    {
        if (Application.loadedLevelName == "Loading0")
            return;

        GameUtil.Log("on_level loaded.");
        GameInterface.Instance.Init();
    }

    public void OnGUI() 
    {
        using (FontSetter fs = new FontSetter())
        {
            if (m_enable)
            {
                GUILayout.BeginArea(new Rect(200, 50, Screen.width - 400, Screen.height - 100), "utest", GUI.skin.window);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (Gui_ShowCloseButton())
                            m_enable = false;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();
                    {
                        Gui_ShowToggles();

                        if (Gui_ChangeByPercentSlider(ref m_renderOrdinaryPercentage, GameInterface.Instance.DisabledRenderers.Count) ||
                            Gui_ChangeByPercentSlider(ref m_renderParticlePercentage, GameInterface.Instance.DisabledParticleSystems.Count))
                            GameInterface.Instance.FilterVisibleObjects(m_renderOrdinaryPercentage * 0.01f, m_renderParticlePercentage * 0.01f);

                        Gui_ShowLogs();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndArea();
            }
            else
            {
                if (GUI.Button(new Rect(50, Screen.height - 50 - 80, 80, 80), "utest"))
                    m_enable = !m_enable;
            }
        }
    }

    bool Gui_ShowCloseButton()
    {
        Vector2 CharSize = GUI.skin.label.CalcSize(new GUIContent("M"));
        return GUILayout.Button("×", GUILayout.MinWidth(CharSize.x * 1.5f), GUILayout.ExpandWidth(false));
    }

    void Gui_ShowToggles()
    {
        foreach (var pair in GameInterface.Instance.KeyNodes)
        {
            if (!pair.Value.activeInHierarchy && pair.Value.activeSelf)
            {
                GUI.enabled = false;
            }
            bool newTest = GUILayout.Toggle(pair.Value.activeSelf, pair.Key);
            if (pair.Value.activeSelf != newTest)
            {
                pair.Value.SetActive(newTest);
            }
            GUI.enabled = true;
        }
    }

    void Gui_ShowLogs()
    {
        GUILayout.Box("Log");
        GameUtil._logPosition = GUILayout.BeginScrollView(GameUtil._logPosition);
        GUILayout.Label(GameUtil._log);
        GUILayout.EndScrollView();
    }

    bool Gui_ChangeByPercentSlider(ref float percentage, int disabledCount)
    {
        bool changed = false;
        GUILayout.BeginHorizontal();
        float newPercentage = GUILayout.HorizontalSlider(percentage, 0, 100);
        if (newPercentage != percentage)
        {
            percentage = newPercentage;
            changed = true;
        }
        GUILayout.Label(percentage.ToString("0.00"), GUILayout.MinWidth(GUI.skin.label.CalcSize(new GUIContent("100.00")).x * 1.5f), GUILayout.ExpandWidth(false));
        GUILayout.Label(disabledCount.ToString(), GUILayout.MinWidth(GUI.skin.label.CalcSize(new GUIContent("000")).x * 1.5f), GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        return changed;
    }
}
