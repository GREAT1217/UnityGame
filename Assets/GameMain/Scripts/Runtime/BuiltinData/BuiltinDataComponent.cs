//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        [SerializeField]
        private TextAsset m_BuildInfoTextAsset = null;

        [SerializeField]
        private TextAsset[] m_DefaultDictionaryTextAssets = null;

        [SerializeField]
        private UpdateResourceForm m_UpdateResourceFormTemplate = null;

        private BuildInfo m_BuildInfo = null;

        public BuildInfo BuildInfo
        {
            get
            {
                return m_BuildInfo;
            }
        }

        public UpdateResourceForm UpdateResourceFormTemplate
        {
            get
            {
                return m_UpdateResourceFormTemplate;
            }
        }

        public void InitBuildInfo()
        {
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            m_BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (m_BuildInfo == null)
            {
                Log.Warning("Parse build info failure.");
                return;
            }
        }

        public void InitDefaultDictionary()
        {
            if (m_DefaultDictionaryTextAssets == null)
            {
                Log.Info("Default dictionaries can not be found.");
                return;
            }

            string language = GameEntry.Localization.Language.ToString();
            string dictionaryText = null;
            for (int i = 0; i < m_DefaultDictionaryTextAssets.Length; i++)
            {
                if (m_DefaultDictionaryTextAssets[i].name.StartsWith(language))
                {
                    dictionaryText = m_DefaultDictionaryTextAssets[i].text;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(dictionaryText))
            {
                Log.Info("Default dictionary can not be found or empty.");
                return;
            }

            if (!GameEntry.Localization.ParseData(dictionaryText))
            {
                Log.Warning("Parse default dictionary failure.");
            }
        }
    }
}
