//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace GABAK
{
    internal class csvexport
    {
        /// <summary>
        /// To keep the ordered list of column names
        /// </summary>
        private List<string> fields = new List<string>();

        /// <summary>
        /// The list of rows
        /// </summary>
        private List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        /// <summary>
        /// The current row
        /// </summary>
        private Dictionary<string, object> currentRow
        { get { return rows[rows.Count - 1]; } }

        /// <summary>
        /// Set a value on this column
        /// </summary>
        public object this[string field]
        {
            set
            {
                // Keep track of the field names, because the dictionary loses the ordering
                if (!fields.Contains(field)) fields.Add(field);
                currentRow[field] = value;
            }
        }

        /// <summary>
        /// Call this before setting any fields on a row
        /// </summary>
        public void addRow()
        {
            rows.Add(new Dictionary<string, object>());
        }

        /// <summary>
        /// Converts a value to how it should output in a csv file
        /// </summary>
        private string makeValueCsvFriendly(object value)
        {
            if (value == null) return "";
            if (value is INullable && ((INullable)value).IsNull) return "";
            if (value is DateTime)
            {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            string output = value.ToString();
            if (output.Contains(",") || output.Contains("\""))
            {
                output = '"' + output.Replace("\"", "\"\"") + '"';
            }
            return output;
        }

        /// <summary>
        /// Return all rows as a CSV returning a string
        /// </summary>
        public string Export()
        {
            StringBuilder sb = new StringBuilder();

            // The header
            foreach (string field in fields)
                sb.Append(field).Append(",");
            sb.AppendLine();

            // The rows
            foreach (Dictionary<string, object> row in rows)
            {
                foreach (string field in fields)
                    sb.Append(makeValueCsvFriendly(row[field])).Append(",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Append all rows as a CSV returning a string
        /// </summary>
        public string Append()
        {
            StringBuilder sb = new StringBuilder();

            // The rows
            foreach (Dictionary<string, object> row in rows)
            {
                foreach (string field in fields)
                    sb.Append(makeValueCsvFriendly(row[field])).Append(",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Exports to a file
        /// </summary>
        public void exportToFile(string path)
        {
            try
            {
                File.WriteAllText(path, Export());
            }
            catch
            {
            }
        }

        /// <summary>
        /// Append to a file
        /// </summary>
        public void appendToFile(string path)
        {
            File.AppendAllText(path, Append());
        }

        /// <summary>
        /// Exports as raw UTF8 bytes
        /// </summary>
        public byte[] exportToBytes()
        {
            return Encoding.UTF8.GetBytes(Export());
        }
    }
}