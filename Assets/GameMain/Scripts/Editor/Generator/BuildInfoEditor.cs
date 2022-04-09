using System;
using System.IO;
using GameFramework;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public class BuildInfoEditor : EditorWindow
    {
        private string m_BuildInfoFileName;
        private BuildInfo m_BuildInfo;
        private string m_GameVersion;
        private int m_InternalGameVersion;
        private string m_CheckVersionUrl;
        private string m_WindowsAppUrl;
        private string m_MacOSAppUrl;
        private string m_IOSAppUrl;
        private string m_AndroidAppUrl;

        [MenuItem("Generator/Edit BuildInfo", false, 0)]
        private static void ShowWindow()
        {
            BuildInfoEditor window = GetWindow<BuildInfoEditor>("Edit BuildInfo", true);
            window.minSize = new Vector2(600f, 200f);
        }

        private void OnEnable()
        {
            m_BuildInfoFileName = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameMain/Configs/BuildInfo.txt"));
            if (File.Exists(m_BuildInfoFileName))
            {
                string json = File.ReadAllText(m_BuildInfoFileName);
                m_BuildInfo = JsonMapper.ToObject<BuildInfo>(json);
                if (m_BuildInfo != null)
                {
                    m_GameVersion = m_BuildInfo.GameVersion;
                    m_InternalGameVersion = m_BuildInfo.InternalGameVersion;
                    m_CheckVersionUrl = m_BuildInfo.CheckVersionUrl;
                    m_WindowsAppUrl = m_BuildInfo.WindowsAppUrl;
                    m_MacOSAppUrl = m_BuildInfo.MacOSAppUrl;
                    m_IOSAppUrl = m_BuildInfo.IOSAppUrl;
                    m_AndroidAppUrl = m_BuildInfo.AndroidAppUrl;
                }
                else
                {
                    Debug.LogError("build info is null");
                }
            }
        }

        private void OnGUI()
        {
            m_GameVersion = EditorGUILayout.TextField("GameVersion:", m_GameVersion);
            m_InternalGameVersion = EditorGUILayout.IntField("InternalGameVersion:", m_InternalGameVersion);
            m_CheckVersionUrl = EditorGUILayout.TextField("CheckVersionUrl:", m_CheckVersionUrl);
            m_WindowsAppUrl = EditorGUILayout.TextField("WindowsAppUrl:", m_WindowsAppUrl);
            m_MacOSAppUrl = EditorGUILayout.TextField("MacOSAppUrl:", m_MacOSAppUrl);
            m_IOSAppUrl = EditorGUILayout.TextField("IOSAppUrl:", m_IOSAppUrl);
            m_AndroidAppUrl = EditorGUILayout.TextField("AndroidAppUrl:", m_AndroidAppUrl);

            if (GUILayout.Button("Generate"))
            {
                string directoryName = Path.GetDirectoryName(m_BuildInfoFileName);
                if (directoryName != null && Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                m_BuildInfo = new BuildInfo()
                {
                    GameVersion = m_GameVersion,
                    InternalGameVersion = m_InternalGameVersion,
                    CheckVersionUrl = m_CheckVersionUrl,
                    WindowsAppUrl = m_WindowsAppUrl,
                    MacOSAppUrl = m_MacOSAppUrl,
                    IOSAppUrl = m_IOSAppUrl,
                    AndroidAppUrl = m_AndroidAppUrl
                };
                JsonWriter json = new JsonWriter { PrettyPrint = true };
                JsonMapper.ToJson(m_BuildInfo, json);
                using (StreamWriter stream = File.CreateText(m_BuildInfoFileName))
                {
                    stream.Write(json.ToString());
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("BuildInfo Generated : " + m_BuildInfoFileName);
            }

            if (GUILayout.Button("Select"))
            {
                if (File.Exists(m_BuildInfoFileName))
                {
                    string path = m_BuildInfoFileName.Substring(m_BuildInfoFileName.IndexOf("Asset", StringComparison.Ordinal));
                    TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    EditorGUIUtility.PingObject(textAsset);
                }
                else
                {
                    Debug.LogWarning("BuildInfo not exist !");
                }
            }
        }
    }
}
