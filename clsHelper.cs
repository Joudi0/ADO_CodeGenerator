using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenarator
{
    public class clsHelper
    {
        public static string tableName = "";

        public struct Column { public string name; public string type; public string isNullable; };
        public static string objectName = "";
        public static string connectionString = @"Server=.;Database=DVLD;User Id=sa;Password=123456;TrustServerCertificate=True;";
        public static string className => "cls" + objectName;

        public static List<Column> Columns;
        public static List<Column> mappedColumns;
        public static Column makeMappedColumnByName(string name)
        {
            int index = clsHelper.getColumnIndex(name);
            if(index == -1)
            {
                Console.WriteLine($"Column with name '{name}' not found.");
                return new Column();
            }
            else
            {
                return mappedColumns[clsHelper.getColumnIndex(name)];
            }
        }
        public static Column makeColumnByName(string name)
        {
            int index = clsHelper.getColumnIndex(name);
            if (index == -1)
            {
                Console.WriteLine($"Column with name '{name}' not found.");
                return new Column();
            }
            else
            {
                return mappedColumns[clsHelper.getColumnIndex(name)];
            }
        }

        public static List<Column> getColumnsNameAndType()
        {
            List<Column> columnsList = new List<Column>();

            SqlConnection connection = new SqlConnection(connectionString);
            string query = $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' ORDER BY ORDINAL_POSITION;";
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Column column = new Column();
                    column.name = reader.GetString(0);
                    column.type = reader.GetString(1);
                    column.isNullable = reader.GetString(2);
                    columnsList.Add(column);
                }
                reader.Close();
            }
            catch (Exception) { throw; }
            finally { connection.Close(); }

            return columnsList;
        }
        public static List<Column> mappingTheColumns(List<Column> ogList)
        {
            List<Column> newList = new List<Column>();
            foreach (Column col in ogList)
            {
                Column c = new Column();
                c.name = col.name;
                c.isNullable = col.isNullable;

                string sqlType = col.type.ToLower();

                switch (sqlType)
                {
                    case "varchar":
                    case "nvarchar":
                    case "char":
                    case "nchar":
                    case "text":
                    case "ntext": c.type = "string"; break;

                    case "bigint": c.type = "long"; break;
                    case "int": c.type = "int"; break;
                    case "smallint": c.type = "short"; break;
                    case "tinyint": c.type = "byte"; break;

                    case "bit": c.type = "bool"; break;

                    case "decimal":
                    case "numeric":
                    case "money":
                    case "smallmoney": c.type = "decimal"; break;
                    case "float": c.type = "double"; break;
                    case "real": c.type = "float"; break;  

                    case "date":
                    case "datetime":
                    case "smalldatetime":
                    case "datetime2":
                        c.type = "DateTime";
                        break;
                    case "time": c.type = "TimeSpan"; break;

                    case "uniqueidentifier": c.type = "Guid"; break;

                    case "binary":
                    case "varbinary":
                    case "image":
                        c.type = "byte[]";
                        break;

                    default: c.type = "object"; break;
                }
                newList.Add(c);
            }
            return newList;
        }
        public static int getColumnIndex(string columnName)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                Column column = Columns[i];
                if (column.name == columnName)
                {
                    return i;
                }
            }
            return -1;
        }
        public static string writeParameters(int columnIndex = 0, bool byRef = true, bool withFirstColumn = true)
        {
            string parameters = "";
            List<Column> newColumns = mappingTheColumns(Columns);
            if (!withFirstColumn) newColumns.RemoveAt(0);
            if (newColumns.Count == 0) return "";

            if (byRef)
            {
                for (int i = 0; i < newColumns.Count; ++i)
                {
                    if (i == columnIndex)
                    {
                        parameters += $@"{newColumns[i].type} {newColumns[columnIndex].name}, ";

                    }
                    else parameters += $"ref {newColumns[i].type} {newColumns[i].name}, ";
                }
            }
            else
            {
                foreach (Column col in newColumns)
                {
                    parameters += $"{col.type} {col.name}, ";
                }
            }

            string result = "";
            if (parameters.Length > 2)
            {
                result = parameters.Substring(0, parameters.Length - 2);
            }
            return result;
        }
        public static string writeParametersToSend(bool byRef = false, int withoutRefIndex = -1)
        {
            string parameters = "";
            List<Column> cols = new List<Column>(Columns);
            if(byRef)
            {
                for(int i = 0; i < cols.Count; ++i)
                {
                    if(i == withoutRefIndex)
                    {
                        parameters += $@"{cols[withoutRefIndex].name}, ";

                    }
                    else parameters += $"ref {cols[i].name}, ";
                }
            }
            else
            {
                if (withoutRefIndex > -1) cols.RemoveAt(withoutRefIndex);
                    foreach (Column col in cols)
                    {
                        parameters += $"{col.name}, ";
                    } 
            }
            string result = "";
            if (parameters.Length > 2)
            {
                result = parameters.Substring(0, parameters.Length - 2);
            }
            return result;
        }

    }
}
