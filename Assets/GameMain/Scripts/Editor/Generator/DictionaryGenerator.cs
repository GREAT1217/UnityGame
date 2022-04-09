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
    public sealed class DictionaryGenerator
    {
        private static readonly string ExcelPath = Application.dataPath + "/../Excels/Dictionaries";
        private const string CollectionFileName = "Assets/GameMain/Configs/Editor/DictionaryCollection.json";

        /// <summary>
        /// 生成字典Text文件
        /// </summary>
        [MenuItem("Generator/Generate Dictionary Text File", false, 61)]
        private static void GenerateDictionaryTextFile()
        {
            ExcelHelper.BatchExcelToText(ExcelPath, AssetUtility.DictionaryPath, CollectionFileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Dictionary Text File Generated !");
        }

        /// <summary>
        /// 生成字典数据文件
        /// </summary>
        [MenuItem("Generator/Generate Dictionary Data File", false, 62)]
        public static void GenerateDictionaryDataFile()
        {
            string collection = File.ReadAllText(CollectionFileName);
            if (string.IsNullOrEmpty(collection))
            {
                Debug.LogWarning("Please Generate Text First.");
                return;
            }

            if (!Directory.Exists(AssetUtility.DictionaryPath))
            {
                Directory.CreateDirectory(AssetUtility.DictionaryPath);
            }

            List<string> dictionaryNames = JsonMapper.ToObject<List<string>>(collection);
            for (int i = 0; i < dictionaryNames.Count; i++)
            {
                string dictionaryName = dictionaryNames[i];
                EditorUtility.DisplayProgressBar("Generate Dictionary Data File", Utility.Text.Format("Generate {0}", dictionaryName), (float)i / dictionaryNames.Count);
                try
                {
                    DictionaryProcessor processor = new DictionaryProcessor(Utility.Path.GetRegularPath(AssetUtility.GetDictionaryAsset(dictionaryName, false)), Encoding.UTF8, 0, 1);
                    GenerateDataFile(processor, dictionaryName);
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.ToString());
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Dictionary Data File Generated !");
        }

        public static void GenerateDataFile(DictionaryProcessor localizationProcessor, string localizationName)
        {
            string binaryDataFileName = Utility.Path.GetRegularPath(AssetUtility.GetDictionaryAsset(localizationName, true));
            if (!localizationProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
            {
                File.Delete(binaryDataFileName);
            }
        }
    }
}
