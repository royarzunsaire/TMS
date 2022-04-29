using MySql.Data.MySqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace SGC.Models.SQL
{
    public class MySqlUtils
    {
        private static MySqlConnection connection;
        private static string GetSqlUpdate(object model,Type type,string table, List<string> keysWhere)
        {
            string sql = "UPDATE {0} SET {1} WHERE {2}";
            string setFormat = "{0} = {1} {2}";
            string setSql = "";
            string whereFormat = "{0} = {1} {2}";
            string whereSql = "";
            PropertyInfo[] propertyInfo = type.GetProperties();
            int i = 0;
            int pos = 0;
            int posWhere = 0;
            foreach (PropertyInfo info in propertyInfo)
            {
                pos = i + 1;
                if(keysWhere.Exists(x => x.Contains(info.Name)))
                {
                    
                    whereSql += string.Format(whereFormat,
                                    info.Name,
                                    "'"+info.GetValue(model)+ "'",
                                     (posWhere +1) == keysWhere.Count ? "" : " AND ");

                    posWhere++;

                }
                else
                {
                    setSql += string.Format(setFormat,
                                        info.Name,
                                        info.GetValue(model) == ""?"''": "'"+info.GetValue(model)+ "'",
                                        pos == propertyInfo.Length ? "" : ",");
                }
         
                i++;
            }
            sql = string.Format(sql,table,setSql,whereSql);


            return sql;
        }
        private static string GetSqlInsert(object model, Type type, string table)
        {
            string sql = "INSERT INTO {0} ({1}) VALUES ({2})";
            string fields = "";
            string values = "";
            string join = "";
            PropertyInfo[] propertyInfo = type.GetProperties();
            int i = 0;
            int pos = 0;
            
            foreach (PropertyInfo info in propertyInfo)
            {
                pos = i + 1;
                
                    join = pos == propertyInfo.Length ? "" : ",";
                    fields += info.Name + join;
                var value = info.GetValue(model) != null ? Convert.ToString(info.GetValue(model)) : "''";
                    values += value + join;

                
                i++;
            }
            sql = string.Format(sql, table, fields, values);
            return sql;
        }
        private static object GetData(MySqlDataReader reader, Type type)
        {
            Dictionary<string, object> _dict = new Dictionary<string, object>();
            PropertyInfo[] propertyInfo = type.GetProperties();
            foreach (PropertyInfo info in propertyInfo)
            {
                    string propName = info.Name;
                    string auth = GetString(reader, info.Name);
                    _dict.Add(propName, auth);
               
            }

          
            return GetObject(_dict, type);
        }
        private static string GetString(MySqlDataReader reader, string colName)
        {
            int colIndex = reader.GetOrdinal(colName);
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }
        private static Object GetObject(Dictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null) continue;

                object value = kv.Value;
                if (value is Dictionary<string, object>)
                {
                    value = GetObject((Dictionary<string, object>)value, prop.PropertyType); // <= This line
                }
                prop.SetValue(obj, Convert.ChangeType(kv.Value, prop.PropertyType),null);
                //prop.SetValue(obj, value, null);
            }
            return obj;
        }

        public static List<object> ExecuteSelectQuery(string sql, Type type)
        {
            List<object> models = new List<object>();
           
            try
            {
                Get_Connection();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = sql; 

                MySqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read()) {
                        models.Add(MySqlUtils.GetData(reader, type));
                    }
                    reader.Close();

                }
                catch (MySqlException e)
                {
                    string MessageString = "Read error occurred  / entry not found loading the Column details: "
                        + e.ErrorCode + " - " + e.Message + "; \n\nPlease Continue";
                    reader.Close();
                    models.Add(new ErrorException{ message = MessageString });
                }
            }
            catch (MySqlException e)
            {
                string MessageString = "The following error occurred loading the Column details: "
                    + e.ErrorCode + " - " + e.Message;
                models.Add(new ErrorException { message = MessageString });
            }



            connection.Close();

            return models;
        }
        private static void Get_Connection()
        {
           
            connection = new MySqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;

            connection.Open();

        }
        //update 
        public static List<object> ExecuteUpdateQuery(object model, Type type, string table, List<string> keysWhere)
        {
            List<object> models = new List<object>();

            try
            {
                Get_Connection();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = GetSqlUpdate(model,type,table,keysWhere);

                MySqlDataReader reader = cmd.ExecuteReader();
                models.Add(new ErrorException { message = "Updated" });

            }
            catch (MySqlException e)
            {
                string MessageString = "The following error occurred loading the Column details: "
                    + e.ErrorCode + " - " + e.Message;
                models.Add(new ErrorException { message = MessageString });
           
            }



            connection.Close();

            return models;
        }
        public static List<object> ExecuteInsertQuery(object model, Type type, string table)
        {
            List<object> models = new List<object>();

            try
            {
                Get_Connection();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = GetSqlInsert(model, type, table);

                MySqlDataReader reader = cmd.ExecuteReader();
                models.Add(new ErrorException { message = "Updated" });

            }
            catch (MySqlException e)
            {
                string MessageString = "The following error occurred loading the Column details: "
                    + e.ErrorCode + " - " + e.Message;
                models.Add(new ErrorException { message = MessageString });

            }



            connection.Close();

            return models;
        }


    }
}