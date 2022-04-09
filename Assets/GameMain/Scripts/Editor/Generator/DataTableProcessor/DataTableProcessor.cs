//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameMain.Editor.DataTable
{
    public sealed partial class DataTableProcessor
    {
        /// <summary>
        /// 注释行分隔符。
        /// </summary>
        private const string CommentLineSeparator = "#";

        /// <summary>
        /// 数据分隔符。
        /// </summary>
        private static readonly char[] DataSplitSeparators = new char[] { '\t' };

        /// <summary>
        /// 数据修剪符。
        /// </summary>
        private static readonly char[] DataTrimSeparators = new char[] { '\"' };

        /// <summary>
        /// 名称行字符数组。
        /// </summary>
        private readonly string[] m_NameRow;

        /// <summary>
        /// 类型行字符数组。
        /// </summary>
        private readonly string[] m_TypeRow;

        /// <summary>
        /// 默认值行字符数组。
        /// </summary>
        private readonly string[] m_DefaultValueRow;

        /// <summary>
        /// 注释行字符数组。
        /// </summary>
        private readonly string[] m_CommentRow;

        /// <summary>
        /// 内容起始行索引。
        /// </summary>
        private readonly int m_ContentStartRow;

        /// <summary>
        /// Id 列索引。
        /// </summary>
        private readonly int m_IdColumn;

        /// <summary>
        /// 所有列的数据处理器。
        /// </summary>
        private readonly DataProcessor[] m_DataProcessor;

        /// <summary>
        /// 数据表的所有原始值。
        /// </summary>
        private readonly string[][] m_RawValues;

        /// <summary>
        /// 所有字符串类型的值。
        /// </summary>
        private readonly string[] m_Strings;

        /// <summary>
        /// 代码模板。
        /// </summary>
        private string m_CodeTemplate;

        /// <summary>
        /// 数据表代码生成器。
        /// </summary>
        private DataTableCodeGenerator m_CodeGenerator;

        /// <summary>
        /// 数据表处理器。
        /// </summary>
        /// <param name="dataTableFileName">数据表文件名。</param>
        /// <param name="encoding">编码。</param>
        /// <param name="nameRow">名称行索引。</param>
        /// <param name="typeRow">类型行索引。</param>
        /// <param name="defaultValueRow">默认值行索引。</param>
        /// <param name="commentRow">注释行索引。</param>
        /// <param name="contentStartRow">内容起始行索引。</param>
        /// <param name="idColumn">id 列索引。</param>
        /// <exception cref="GameFrameworkException"></exception>
        public DataTableProcessor(string dataTableFileName, Encoding encoding, int nameRow, int typeRow, int? defaultValueRow, int? commentRow, int contentStartRow, int idColumn)
        {
            if (string.IsNullOrEmpty(dataTableFileName))
            {
                throw new GameFrameworkException("Data table file name is invalid.");
            }

            // if (!dataTableFileName.EndsWith(".txt", StringComparison.Ordinal))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not a txt.", dataTableFileName));
            // }

            if (!File.Exists(dataTableFileName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not exist.", dataTableFileName));
            }

            string[] lines = File.ReadAllLines(dataTableFileName, encoding);
            int rawRowCount = lines.Length; // 原始行数

            int rawColumnCount = 0; // 原始列数
            List<string[]> rawValues = new List<string[]>(); // 原始值
            for (int i = 0; i < lines.Length; i++)
            {
                string[] rawValue = lines[i].Split(DataSplitSeparators);
                for (int j = 0; j < rawValue.Length; j++)
                {
                    rawValue[j] = rawValue[j].Trim(DataTrimSeparators);
                }

                if (i == 0)
                {
                    rawColumnCount = rawValue.Length;
                }
                else if (rawValue.Length != rawColumnCount)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}', raw Column is '{2}', but line '{1}' column is '{3}'.", dataTableFileName, i, rawColumnCount, rawValue.Length));
                }

                rawValues.Add(rawValue);
            }

            m_RawValues = rawValues.ToArray();

            if (nameRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow));
            }

            if (typeRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' is invalid.", typeRow));
            }

            if (contentStartRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' is invalid.", contentStartRow));
            }

            if (idColumn < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' is invalid.", idColumn));
            }

            if (nameRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' >= raw row count '{1}' is not allow.", nameRow, rawRowCount));
            }

            if (typeRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' >= raw row count '{1}' is not allow.", typeRow, rawRowCount));
            }

            if (defaultValueRow.HasValue && defaultValueRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Default value row '{0}' >= raw row count '{1}' is not allow.", defaultValueRow.Value, rawRowCount));
            }

            if (commentRow.HasValue && commentRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Comment row '{0}' >= raw row count '{1}' is not allow.", commentRow.Value, rawRowCount));
            }

            if (contentStartRow > rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow, rawRowCount));
            }

            if (idColumn >= rawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' >= raw column count '{1}' is not allow.", idColumn, rawColumnCount));
            }

            m_NameRow = m_RawValues[nameRow];
            m_TypeRow = m_RawValues[typeRow];
            m_DefaultValueRow = defaultValueRow.HasValue ? m_RawValues[defaultValueRow.Value] : null;
            m_CommentRow = commentRow.HasValue ? m_RawValues[commentRow.Value] : null;
            m_ContentStartRow = contentStartRow;
            m_IdColumn = idColumn;

            // 设置每一列数据的处理器
            m_DataProcessor = new DataProcessor[rawColumnCount];
            for (int i = 0; i < rawColumnCount; i++)
            {
                if (i == IdColumn)
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                }
                else
                {
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(m_TypeRow[i]);
                }
            }

            // 记录字符串类型的值
            Dictionary<string, int> strings = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = contentStartRow; i < rawRowCount; i++)
            {
                if (IsCommentRow(i))
                {
                    continue;
                }

                for (int j = 0; j < rawColumnCount; j++)
                {
                    if (m_DataProcessor[j].LanguageKeyword != "string")
                    {
                        continue;
                    }

                    string str = m_RawValues[i][j];
                    if (strings.ContainsKey(str))
                    {
                        strings[str]++;
                    }
                    else
                    {
                        strings[str] = 1;
                    }
                }
            }

            m_Strings = strings.OrderBy(value => value.Key).OrderByDescending(value => value.Value).Select(value => value.Key).ToArray();

            m_CodeTemplate = null;
            m_CodeGenerator = null;
        }

        /// <summary>
        /// 原始行数。
        /// </summary>
        public int RawRowCount
        {
            get
            {
                return m_RawValues.Length;
            }
        }

        /// <summary>
        /// 原始列数。
        /// </summary>
        public int RawColumnCount
        {
            get
            {
                return m_RawValues.Length > 0 ? m_RawValues[0].Length : 0;
            }
        }

        /// <summary>
        /// 字符串类型的值的数量。
        /// </summary>
        public int StringCount
        {
            get
            {
                return m_Strings.Length;
            }
        }

        /// <summary>
        /// 内容起始行索引。
        /// </summary>
        public int ContentStartRow
        {
            get
            {
                return m_ContentStartRow;
            }
        }

        /// <summary>
        /// Id 列索引。
        /// </summary>
        public int IdColumn
        {
            get
            {
                return m_IdColumn;
            }
        }

        /// <summary>
        /// 是否是 Id 列。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool IsIdColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].IsId;
        }

        /// <summary>
        /// 是否是注释行。
        /// </summary>
        /// <param name="rawRow"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool IsCommentRow(int rawRow)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            return GetValue(rawRow, 0).StartsWith(CommentLineSeparator, StringComparison.Ordinal);
        }

        /// <summary>
        /// 是否是注释列。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool IsCommentColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return string.IsNullOrEmpty(GetName(rawColumn)) || m_DataProcessor[rawColumn].IsComment;
        }

        /// <summary>
        /// 获取列的名称。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetName(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            if (IsIdColumn(rawColumn))
            {
                return "Id";
            }

            return m_NameRow[rawColumn];
        }

        /// <summary>
        /// 是否是系统类型的列。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool IsSystem(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].IsSystem;
        }

        /// <summary>
        /// 获取列的类型。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public System.Type GetType(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].Type;
        }

        /// <summary>
        /// 获取列的关键字。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetLanguageKeyword(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].LanguageKeyword;
        }

        /// <summary>
        /// 获取列的默认值。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetDefaultValue(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DefaultValueRow != null ? m_DefaultValueRow[rawColumn] : null;
        }

        /// <summary>
        /// 获取列的注释。
        /// </summary>
        /// <param name="rawColumn"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetComment(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_CommentRow != null ? m_CommentRow[rawColumn] : null;
        }

        /// <summary>
        /// 获取值。
        /// </summary>
        /// <param name="rawRow">原始行索引。</param>
        /// <param name="rawColumn">原始列索引。</param>
        /// <returns>值。</returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetValue(int rawRow, int rawColumn)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_RawValues[rawRow][rawColumn];
        }

        /// <summary>
        /// 获取字符串值。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public string GetString(int index)
        {
            if (index < 0 || index >= StringCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("String index '{0}' is out of range.", index));
            }

            return m_Strings[index];
        }

        /// <summary>
        /// 获取字符串值的索引。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int GetStringIndex(string str)
        {
            for (int i = 0; i < StringCount; i++)
            {
                if (m_Strings[i] == str)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 生成数据文件。
        /// </summary>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool GenerateDataFile(string outputFileName)
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new GameFrameworkException("Output file name is invalid.");
            }

            try
            {
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        for (int rawRow = ContentStartRow; rawRow < RawRowCount; rawRow++)
                        {
                            if (IsCommentRow(rawRow))
                            {
                                continue;
                            }

                            byte[] bytes = GetRowBytes(outputFileName, rawRow);
                            binaryWriter.Write7BitEncodedInt32(bytes.Length);
                            binaryWriter.Write(bytes);
                        }
                    }
                }

                Debug.Log(Utility.Text.Format("Parse data table '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Parse data table '{0}' failure, exception is '{1}'.", outputFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 设置代码模板。
        /// </summary>
        /// <param name="codeTemplateFileName"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public bool SetCodeTemplate(string codeTemplateFileName, Encoding encoding)
        {
            try
            {
                m_CodeTemplate = File.ReadAllText(codeTemplateFileName, encoding);
                Debug.Log(Utility.Text.Format("Set code template '{0}' success.", codeTemplateFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Set code template '{0}' failure, exception is '{1}'.", codeTemplateFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 设置代码生成器。
        /// </summary>
        /// <param name="codeGenerator"></param>
        public void SetCodeGenerator(DataTableCodeGenerator codeGenerator)
        {
            m_CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// 生成代码文件。
        /// </summary>
        /// <param name="outputFileName"></param>
        /// <param name="encoding"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <exception cref="GameFrameworkException"></exception>
        public bool GenerateCodeFile(string outputFileName, Encoding encoding, object userData = null)
        {
            if (string.IsNullOrEmpty(m_CodeTemplate))
            {
                throw new GameFrameworkException("You must set code template first.");
            }

            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new GameFrameworkException("Output file name is invalid.");
            }

            try
            {
                StringBuilder stringBuilder = new StringBuilder(m_CodeTemplate);
                if (m_CodeGenerator != null)
                {
                    m_CodeGenerator(this, stringBuilder, userData);
                }

                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter stream = new StreamWriter(fileStream, encoding))
                    {
                        stream.Write(stringBuilder.ToString());
                    }
                }

                Debug.Log(Utility.Text.Format("Generate code file '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Generate code file '{0}' failure, exception is '{1}'.", outputFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 获取一行数据的字节。
        /// </summary>
        /// <param name="outputFileName"></param>
        /// <param name="rawRow"></param>
        /// <returns></returns>
        private byte[] GetRowBytes(string outputFileName, int rawRow)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    for (int rawColumn = 0; rawColumn < RawColumnCount; rawColumn++)
                    {
                        if (IsCommentColumn(rawColumn))
                        {
                            continue;
                        }

                        try
                        {
                            m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetValue(rawRow, rawColumn));
                        }
                        catch
                        {
                            if (m_DataProcessor[rawColumn].IsId || string.IsNullOrEmpty(GetDefaultValue(rawColumn)))
                            {
                                Debug.LogError(Utility.Text.Format("Parse raw value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                                return null;
                            }
                            else
                            {
                                Debug.LogWarning(Utility.Text.Format("Parse raw value failure, will try default value. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                                try
                                {
                                    m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetDefaultValue(rawColumn));
                                }
                                catch
                                {
                                    Debug.LogError(Utility.Text.Format("Parse default value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetComment(rawColumn)));
                                    return null;
                                }
                            }
                        }
                    }

                    return memoryStream.ToArray();
                }
            }
        }
    }
}
