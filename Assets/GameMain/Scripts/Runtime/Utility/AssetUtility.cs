//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;

namespace GameMain
{
    public static class AssetUtility
    {
        public static readonly string ConfigPath = "Assets/GameMain/Configs/Runtime";
        public static readonly string DataTablePath = "Assets/GameMain/DataTables";
        public static readonly string DictionaryPath = "Assets/GameMain/Localizations/Dictionaries";

        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("{0}/{1}.{2}", ConfigPath, assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("{0}/{1}.{2}", DataTablePath, assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("{0}/{1}.{2}", DictionaryPath, assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetCurrentDictionaryAsset(string dictionaryName, bool fromBytes)
        {
            return Utility.Text.Format("{0}/{1}_{2}.{3}", DictionaryPath, GameEntry.Localization.Language.ToString(), dictionaryName, fromBytes ? "bytes" : "txt");
        }

        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Fonts/{0}.ttf", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Scenes/{0}.unity", assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Music/{0}.mp3", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Sounds/{0}.wav", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/UI/UIForms/{0}.prefab", assetName);
        }

        public static string GetUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/UI/UISounds/{0}.wav", assetName);
        }
    }
}
