using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using static CodeGenarator.clsHelper;

namespace CodeGenarator
{
    public class clsDAL
    {


        // Helpers

        public static string updateParametersValue(int index)
        {
            List<Column> newColumns = mappingTheColumns(Columns);
            newColumns.RemoveAt(index);
            string parameters = "";
            foreach (Column col in newColumns)
            {
                string tabs = "\t\t\t    ";
                if (col.isNullable == "NO")
                {
                    parameters += tabs + $@"{col.name} = ({col.type})reader[""{col.name}""];" + "\n";
                }
                else if (col.isNullable == "YES")
                {
                    parameters += tabs;
                    switch (col.type)
                    {
                        case "byte": parameters += $@"{col.name} = (reader[""{col.name}""] == DBNull.Value) ? 0 : ({col.type})reader[""{col.name}""];" + "\n"; break;
                        case "decimal":
                        case "int": parameters += $@"{col.name} = (reader[""{col.name}""] == DBNull.Value) ? -1 : ({col.type})reader[""{col.name}""];" + "\n"; break;
                        case "string": parameters += $@"{col.name} = (reader[""{col.name}""] == DBNull.Value) ? """" : ({col.type})reader[""{col.name}""];" + "\n"; break;
                        case "DateTime": parameters += $@"{col.name} = (reader[""{col.name}""] == DBNull.Value) ? DateTime.Now : ({col.type})reader[""{col.name}""];" + "\n"; break;
                        case "bool": parameters += $@"{col.name} = (reader[""{col.name}""] == DBNull.Value) ? false : ({col.type})reader[""{col.name}""];" + "\n"; break;
                        default: break;
                    }
                }
            }
            return parameters;
        }

        public static string updateScript(string columnName)
        {

            string script = $"UPDATE {tableName} SET\n";
            List<Column> cols = new List<Column>(mappedColumns);
            cols.RemoveAt(0);
            foreach (Column col in cols)
            {
                script += $"\t\t    {col.name} = @{col.name},\n";
            }
            string result = script.Substring(0, script.Length - 2);
            result += $@" WHERE {columnName} = @{columnName};";
            return result;
        }

        public static string addWithValueAllScript(bool withoutFirst = true)
        {
            List<Column> newColumns = new List<Column>(mappedColumns);
            if(withoutFirst) newColumns.RemoveAt(0);
            string script = "";
            string tabs = "\n\t\t    ";
            foreach (Column col in newColumns)
            {
                script += tabs;
                if (col.isNullable == "NO") script += $@"command.Parameters.AddWithValue(@""{col.name}"", {col.name});";
                else if (col.isNullable == "YES")
                {
                    switch (col.type)
                    {
                        case "string": script += $@"command.Parameters.AddWithValue(@""{col.name}"", string.IsNullOrEmpty({col.name}) ? DBNull.Value : (object){col.name});"; break;
                        case "byte": script += $@"command.Parameters.AddWithValue(@""{col.name}"", ({col.name} == 0) ? DBNull.Value : (object){col.name});"; break;
                        case "decimal":
                        case "int": script += $@"command.Parameters.AddWithValue(@""{col.name}"", ({col.name} == -1) ? DBNull.Value : (object){col.name});"; break;
                        case "DateTime": script += $@"command.Parameters.AddWithValue(@""{col.name}"", ({col.name} == DateTime.MinValue) ? DBNull.Value : (object){col.name});"; break;
                        default: break;
                    }
                }
            }
            return script;
        }

        public static string addScript()
        {
            List<Column> newColumns = new List<Column>(mappedColumns);
            newColumns.RemoveAt(0);
            string script = $"INSERT INTO {tableName} (";
            foreach (Column col in newColumns)
            {
                script += $@"{col.name}, ";
            }
            string script2 = script.Substring(0, script.Length - 2) + ")\nVALUES (";
            foreach (Column col in newColumns)
            {
                script2 += $@"@{col.name}, ";
            }
            string result = script2.Substring(0, script2.Length - 2) + ");\nSELECT SCOPE_IDENTITY();";
            return result;
        }

        // Actual Functions

        public static string getRecordByColumnFunc(string columnName)
        {
            int columnIndex = getColumnIndex(columnName);
            string FunctionName = "";
            if (columnIndex == 0)
            {
                FunctionName = $@"get{objectName}ByID";
            }
            else FunctionName = $@"get{objectName}By{columnName}";
            if (Columns.Count == 0) return "Error in the lists";
            string Function =
                $@"public static bool {FunctionName}({writeParameters(columnIndex)})
                {{
                    bool isFound = false;
                    using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                    string query = ""SELECT * FROM {tableName} WHERE {Columns[columnIndex].name} = @{Columns[columnIndex].name}"";
                    using SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue(""@{Columns[columnIndex].name}"", {Columns[columnIndex].name});
                    try
                    {{
                        connection.Open();
                        using SqlDataReader reader = command.ExecuteReader();


                        if (reader.Read())
                        {{
                            isFound = true;
{updateParametersValue(columnIndex)}
                        }}
                    }}

                     catch (Exception ex) {{ throw;}}

                    return isFound;
                }}";

            return Function;
        }

        public static string updateFunc(string columnName)
        {
            if (Columns.Count == 0) return "Error in the lists";
            string Function =
                $@"public static bool update{objectName}({writeParameters(byRef: false)})
                {{
                    int rowsAffected = 0;
                    using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                    string query = @""{updateScript(columnName)}"";
                    using SqlCommand command = new SqlCommand(query, connection);
                    {addWithValueAllScript(false)}
                    try
                    {{
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }}
                    catch (Exception) {{ throw; }}
                    return (rowsAffected > 0);
                }}";
            return Function;
        }

        public static string addFunc(string columnName)
        {
            if (Columns.Count == 0) return "Error in the lists";
            string Function =
                $@"public static int add{objectName}({writeParameters(0, false, false)})
                {{
                    int {objectName}ID = -1;
                    using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                    string query = @""{addScript()}"";
                    using SqlCommand command = new SqlCommand(query, connection);
                    {addWithValueAllScript()}
                    try
                    {{
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        {{
                            {objectName}ID = insertedID;
                        }}
                    }}
                    catch (Exception) {{ throw; }}

                    return {objectName}ID;
                }}";
            return Function;
        }

        public static string deleteFunc(string columnName)
        {
            if (Columns.Count == 0) return "Error in the lists";
            int columnIndex = getColumnIndex(columnName);
            Column c = Columns[columnIndex];

            string Function = $@"public static bool delete{objectName}({c.type} {c.name})
            {{
                int rowsAffected = 0;
                using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                string query = ""DELETE FROM {tableName} WHERE {columnName} = @{columnName}"";
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue(""@{columnName}"", {columnName});
                try
                {{
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }}
                catch (Exception) {{ throw; }}
                return (rowsAffected > 0);
            }}";
            return Function;
        }

        public static string getAllFunc()
        {

            string Function = $@"
                    public static DataTable getAll()
                    {{
                        DataTable dt = new DataTable();
                        using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                        string query = ""SELECT * FROM {tableName}"";
                        using SqlCommand command = new SqlCommand(query, connection);

                        try
                        {{
                            connection.Open();
                            using SqlDataReader reader = command.ExecuteReader();
                            if (reader.HasRows) dt.Load(reader);
                        }}
                        catch (Exception) {{ throw; }}

                        return dt;
                    }}";
            return Function;
        }


        public static string getAllByColumnFunc(string columnName)
        {
            int columnIndex = getColumnIndex(columnName);
            string FunctionName = "";
            if (columnIndex == 0)
            {
                FunctionName = $@"getAllByID";
            }
            else FunctionName = $@"getAllBy{columnName}";
            if (Columns.Count == 0) return "Error in the lists";

            string Function = $@"
                    public static DataTable {FunctionName}({Columns[columnIndex].type} {columnName})
                    {{
                        DataTable dt = new DataTable();
                        using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                        string query = ""SELECT * FROM {tableName} WHERE {Columns[columnIndex].name} = @{Columns[columnIndex].name}"";
                        using SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue(""@{Columns[columnIndex].name}"", {Columns[columnIndex].name});

                        try
                        {{
                            connection.Open();
                            using SqlDataReader reader = command.ExecuteReader();
                            if (reader.HasRows) dt.Load(reader);
                        }}
                        catch (Exception) {{ throw; }}

                        return dt;
                    }}";
            return Function;
        }


        public static string isExistsFunc(string columnName)
        {
            int columnIndex = getColumnIndex(columnName);
            string FunctionName = "";
            if (columnIndex == 0)
            {
                FunctionName = $@"is{objectName}ExistByID";
            }
            else FunctionName = $@"is{objectName}ExistBy{columnName}";
            string Function = $@"
            public static bool {FunctionName}({mappedColumns[columnIndex].type} {mappedColumns[columnIndex].name})
            {{
                bool isFound = false;
                using SqlConnection connection = new SqlConnection(clsDataSettings.connectionString);
                string query = ""SELECT Found=1 FROM {tableName} WHERE {columnName} = @{columnName}"";
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue(""@{columnName}"", {columnName});

                try
                {{
                    connection.Open();
                    object result = command.ExecuteScalar();
                    isFound = (result != null);
                }}
                catch (Exception) {{ throw; }}

                return isFound;
            }}";
            return Function;
        }


        public static string classStructure(string injectedString)
        {
            string structure = $@"using Microsoft.Data.SqlClient;\r\nusing System;\r\nusing System.Collections.Generic;\r\nusing System.Data;\r\n\r\nnamespace DAL\r\n{{\r\n    public class cls{objectName}DAL\r\n    {{  {injectedString}  }}\r\n}}";
            return structure;
        }
       
    }
}