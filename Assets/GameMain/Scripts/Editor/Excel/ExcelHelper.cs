using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Excel;
using GameFramework;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor
{
    public static class ExcelHelper
    {
        private static readonly string TextExtension = ".txt";
        private static readonly string ExcelExtension = ".xlsx";

        private static List<FileInfo> GetFilesWithExtension(string directoryPath, string fileExtension)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*" + fileExtension, SearchOption.AllDirectories);
            List<FileInfo> result = new List<FileInfo>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                if (fileInfos[i].Name.StartsWith("~$"))
                {
                    continue;
                }
                result.Add(fileInfos[i]);
            }
            return result;
        }

        public static void BatchExcelToText(string excelDirectory, string textDirectory, string collectionFileName)
        {
            if (string.IsNullOrEmpty(excelDirectory) || string.IsNullOrEmpty(textDirectory))
            {
                return;
            }

            if (!Directory.Exists(textDirectory))
            {
                Directory.CreateDirectory(textDirectory);
            }

            List<string> names = new List<string>();
            List<FileInfo> fileInfos = GetFilesWithExtension(excelDirectory, ExcelExtension);
            for (int i = 0; i < fileInfos.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Excel To Csv", Utility.Text.Format("Converting {0}", fileInfos[i].Name), (float)i / fileInfos.Count);
                FileInfo fileInfo = fileInfos[i];
                string excelFile = Utility.Path.GetRegularPath(fileInfo.FullName);
                string textFile = Utility.Path.GetRegularPath(Path.Combine(textDirectory, fileInfo.Name.Replace(ExcelExtension, TextExtension)));
                ExcelToText(excelFile, textFile);
                names.Add(fileInfo.Name.Split('.')[0]);
            }

            string collectionDirectory = Path.GetDirectoryName(collectionFileName);
            if (!string.IsNullOrEmpty(collectionDirectory) && !Directory.Exists(collectionDirectory))
            {
                Directory.CreateDirectory(collectionDirectory);
            }

            JsonWriter json = new JsonWriter { PrettyPrint = true };
            JsonMapper.ToJson(names, json);

            using (FileStream stream = new FileStream(collectionFileName, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(json.ToString());
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static void ExcelToText(string excelFile, string textFile)
        {
            if (!File.Exists(excelFile))
            {
                Debug.LogError("File Not Exits : " + excelFile);
                return;
            }

            if (File.Exists(textFile))
            {
                File.Delete(textFile);
            }

            try
            {
                using (FileStream excelStream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
                {
                    StringBuilder text = new StringBuilder();

                    using (IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(excelStream))
                    {
                        DataSet data = reader.AsDataSet();
                        if (data.Tables.Count < 1)
                        {
                            Debug.LogError("Excel Not Exit Any Table : " + excelFile);
                            return;
                        }

                        var sheet = data.Tables[0];
                        int rowCount = sheet.Rows.Count;
                        int columnCount = sheet.Columns.Count;
                        for (int row = 0; row < rowCount; row++)
                        {
                            if (row != 0)
                            {
                                text.Append("\r\n");
                            }
                            for (int column = 0; column < columnCount; column++)
                            {
                                if (column != 0)
                                {
                                    text.Append("\t");
                                }
                                text.Append(sheet.Rows[row][column].ToString());
                            }
                        }
                    }

                    using (FileStream stream = new FileStream(textFile, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            writer.Write(text.ToString());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString());
            }
        }
    }
}
