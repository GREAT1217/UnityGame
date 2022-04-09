using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public sealed class ConfigGenerator
    {
        private static readonly string ConfigExcelPath = Application.dataPath + "/../Excels/Configs";
        private const string CollectionFileName = "Assets/GameMain/Configs/Editor/ConfigCollection.json";

        /// <summary>
        /// 生成配置Text文件
        /// </summary>
        [MenuItem("Generator/Generate Config Text File", false, 21)]
        private static void GenerateConfigTextFile()
        {
            ExcelHelper.BatchExcelToText(ConfigExcelPath, AssetUtility.ConfigPath, CollectionFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Config Text File Generated !");
        }

        /// <summary>
        /// 生成配置数据文件
        /// </summary>
        [MenuItem("Generator/Generate Config Data File", false, 22)]
        public static void GenerateConfigDataFile()
        {
            string collection = File.ReadAllText(CollectionFileName);
            if (string.IsNullOrEmpty(collection))
            {
                Debug.LogWarning("Please Generate Text First.");
                return;
            }

            if (!Directory.Exists(AssetUtility.ConfigPath))
            {
                Directory.CreateDirectory(AssetUtility.ConfigPath);
            }

            List<string> configNames = JsonMapper.ToObject<List<string>>(collection);
            for (int i = 0; i < configNames.Count; i++)
            {
                string configName = configNames[i];
                EditorUtility.DisplayProgressBar("Generate Config Data File", Utility.Text.Format("Generate {0}", configName), (float)i / configNames.Count);
                try
                {
                    DictionaryProcessor processor = new DictionaryProcessor(Utility.Path.GetRegularPath(AssetUtility.GetConfigAsset(configName, false)), Encoding.UTF8, 1, 2);
                    GenerateDataFile(processor, configName);
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.Message);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Config Data File Generated !");
        }

        public static void GenerateDataFile(DictionaryProcessor configProcessor, string configName)
        {
            string binaryDataFileName = Utility.Path.GetRegularPath(AssetUtility.GetConfigAsset(configName, true));
            if (!configProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
            {
                File.Delete(binaryDataFileName);
            }
        }
    }
}
