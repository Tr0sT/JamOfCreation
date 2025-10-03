using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class NoesisWelcome : EditorWindow
{
    public static void Open()
    {
        Rect r = new Rect((Screen.currentResolution.width - Width) / 2, (Screen.currentResolution.height - Height) / 2, Width, Height);
        EditorWindow.GetWindowWithRect(typeof(NoesisWelcome), r, true, "Welcome to NoesisGUI!");
    }

    private Texture2D _banner;
    private Texture2D _icon0;
    private Texture2D _icon1;
    private Texture2D _icon2;
    private Texture2D _icon3;
    private Texture2D _icon4;
    private string _version;

    private const int Width = 330;
    private const int Height = 450;
    private GUIStyle _buttonStyle;
    private GUIStyle _bannerStyle;

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Width, 60), "", _bannerStyle);

        GUILayout.BeginArea(new Rect(0, 0, Width, Height));
        GUILayout.BeginVertical();

        GUILayout.Space(4.0f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_banner);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(18.0f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("NoesisGUI v" + _version + " installed", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(18.0f);

        string docPath = Path.GetFullPath("Packages/com.noesis.noesisgui/Documentation~/Documentation.html");
        string docURL = File.Exists(docPath) ? "file://" + docPath.Replace(" ", "%20") : "http://www.noesisengine.com/docs";

        string changelogPath = Path.GetFullPath("Packages/com.noesis.noesisgui/Documentation~/Doc/Gui.Core.Changelog.html");
        string changelogURL = File.Exists(changelogPath) ? "file://" + changelogPath.Replace(" ", "%20") : "http://www.noesisengine.com/docs/Gui.Core.Changelog.html";

        Button(_icon0, "Release notes", "Read what is new in this version", changelogURL);
        GUILayout.Space(10.0f);
        Button(_icon4, "Visual Studio Code", "VS Code extension for XAML", "https://marketplace.visualstudio.com/items?itemName=NoesisTechnologies.noesisgui-tools");
        GUILayout.Space(10.0f);
        Button(_icon1, "Examples", "Learn from our samples", "https://github.com/Noesis/Tutorials");
        GUILayout.Space(10.0f);
        Button(_icon2, "Documentation", "Read local documentation", docURL);
        GUILayout.Space(10.0f);
        Button(_icon3, "Forums", "Join the noesisGUI community", "https://forums.noesisengine.com/");
        GUILayout.Space(10.0f);

        GUILayout.Space(8.0f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label("Samples available in Package Manager", EditorStyles.miniLabel);
        GUILayout.Label("Find more options at Tools -> NoesisGUI", EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void Button(Texture2D texture, string name, string desc, string url)
    {
        GUILayout.BeginHorizontal(GUILayout.MaxHeight(48));
        GUILayout.Space(38.0f);

        if (GUILayout.Button(texture, _buttonStyle))
        {
            UnityEngine.Application.OpenURL(url);
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        GUILayout.Space(20.0f);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label(name, EditorStyles.boldLabel);
        GUILayout.Label(desc, EditorStyles.label);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void OnEnable()
    {
        _version = NoesisVersion.Get();

        _bannerStyle = new GUIStyle();
        _buttonStyle = new GUIStyle();
        _buttonStyle.fixedWidth = 48;
        _buttonStyle.fixedHeight = 48;

        LoadResources();
    }

    void OnInspectorUpdate()
    {
        if (LoadResources())
        {
            Repaint();
        }
    }

    bool LoadResources()
    {
        bool doRepaint = false;

        if (_banner == null)
        {
            _banner = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/banner.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        if (_bannerStyle.normal.background == null)
        {
            _bannerStyle.normal.background = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/banner_bg.png", typeof(Texture2D));
            if (_bannerStyle.normal.background != null) doRepaint = true;
        }

        if (_icon0 == null)
        {
            _icon0 = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/ico_release.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        if (_icon1 == null)
        {
            _icon1 = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/ico_tutorials.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        if (_icon2 == null)
        {
            _icon2 = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/ico_doc.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        if (_icon3 == null)
        {
            _icon3 = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/ico_forums.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        if (_icon4 == null)
        {
            _icon4 = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.noesis.noesisgui/Editor/ico_vs.png", typeof(Texture2D));
            if (_banner != null) doRepaint = true;
        }

        return doRepaint;
    }
}