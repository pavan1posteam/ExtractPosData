﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtractPosData
{
    public class Verify
    {
        public int StoreId { get; set; }
        public DataRow dr { get; set; }

        private readonly DataRow _row;
        
        private readonly int _storeId;

        public Verify(int id, DataRow data)// By Column Name
        {
            StoreId = id;
            dr = data;
        }

        public Verify(DataRow row, int storeId)// By Index Position
        {
            _storeId = storeId;
            _row = row;
        }

        private string GetColumnValue(string columnName)
        {
            var column = dr.Table.Columns
                .Cast<DataColumn>()
                .FirstOrDefault(c => string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));

            if (column == null)
                throw new Exception($"Invalid or Missing Column: {columnName} in {StoreId}");

            return dr[column]?.ToString().Trim() ?? "";
        }

        public string GetString(string columnName)
        {
            return GetColumnValue(columnName);
        }

        public int GetInt(string columnName)
        {
            string value = GetColumnValue(columnName);
            if (!int.TryParse(value, out int result))
                throw new Exception($"Invalid Column: {columnName} in {StoreId}");
            return result;
        }

        public decimal GetDecimal(string columnName)
        {
            string value = GetColumnValue(columnName);
            if (!decimal.TryParse(value, out decimal result))
                throw new Exception($"Invalid Column: {columnName} in {StoreId}");
            return result;
        }

        public bool GetBool(string columnName)
        {
            string value = GetColumnValue(columnName);
            if (!bool.TryParse(value, out bool result))
                throw new Exception($"Invalid Column: {columnName} in {StoreId}");
            return result;
        }

        public string GetStringByIndex(int index)
        {
            if (index >= _row.ItemArray.Length)
                throw new Exception($"Missing column at index {index} in store {_storeId}");

            return _row[index]?.ToString().Trim() ?? "";
        }

        //public int GetIntByIndex(int index)
        //{
        //    string val = GetStringByIndex(index);
        //    if (!int.TryParse(val, out int result))
        //        throw new Exception($"Invalid integer at index {index} in store {_storeId}");
        //    return result;
        //}
        public int GetIntByIndex(int index)
        {
            if (index >= _row.ItemArray.Length)
                throw new Exception($"Missing column at index {index} in store {_storeId}");

            var value = _row[index];

            try
            {
                if (value == null || Convert.IsDBNull(value))
                    return 0;

                if (decimal.TryParse(value.ToString(), out decimal result))
                {
                    return (int)Math.Floor(result);
                }
                string val = GetStringByIndex(index);
                if (!int.TryParse(val, out int result1))
                    return 0;
                else
                    return result1;
            }
            catch (Exception)
            {
                throw new Exception($"Invalid integer value at index {index} in store {_storeId}. Value: '{value}'");
            }
        }
        //public decimal GetDecimalByIndex(int index)
        //{
        //    string val = GetStringByIndex(index);
        //    if (!decimal.TryParse(val, out decimal result))
        //        throw new Exception($"Invalid decimal at index {index} in store {_storeId}");
        //    return result;
        //}
        public decimal GetDecimalByIndex(int index)
        {
            if (index >= _row.ItemArray.Length)
                throw new Exception($"Missing column at index {index} in store {_storeId}");

            var value = _row[index];

            try
            {
                if (value == null || Convert.IsDBNull(value))
                    return 0;

                if (decimal.TryParse(value.ToString(), out decimal result))
                {
                    return result;
                }
                string val = GetStringByIndex(index);
                if (!decimal.TryParse(val, out decimal result1))
                    return 0;
                else
                    return result1;
            }
            catch (Exception)
            {
                throw new Exception($"Invalid decimal value at index {index} in store {_storeId}. Value: '{value}'");
            }
        }
        public int getpack(string prodName)
        {
            prodName = prodName.ToUpper();
            var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)PK");
            var prodPack = regexMatch.Groups["Result"].Value;
            if (prodPack.Length > 0)
            {
                int outVal = 0;
                int.TryParse(prodPack.Replace("$", ""), out outVal);
                return outVal;
            }
            return 1;
        }
        public string getVolume(string prodName)
        {
            prodName = prodName.ToUpper();
            var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)ML| (?<Result>\d+)LTR| (?<Result>\d+)OZ | (?<Result>\d+)L|(?<Result>\d+)OZ");
            var prodPack = regexMatch.Groups["Result"].Value;
            if (prodPack.Length > 0)
            {
                return regexMatch.ToString();
            }
            return "";
        }
    }

}
