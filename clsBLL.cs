using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeGenarator.clsHelper;

namespace CodeGenarator
{
    public class clsBLL
    {
        public static string DALName = $@"cls{objectName}DAL";
        public static string tabs = "\t\t";
        // Helpers Functions:

        public static string giveInitialValue(Column c, bool newVar = false)
        {
            string line = "";
            if(newVar)
            {
                switch (c.type)
                {
                    case "byte": line += $@"{c.type} {c.name} = 0;"; break;
                    case "bool": line += $@"{c.type} {c.name} = false;"; break;
                    case "decimal":
                    case "int": line += $@"{c.type} {c.name} = -1;"; break;
                    case "string": line += $@"{c.type} {c.name} = """";"; break;
                    case "DateTime": line += $@"{c.type} {c.name} = DateTime.Now;"; break;
                }
            }
            else
            {
                switch (c.type)
                {
                    case "byte": line += $@"this.{c.name} = 0;"; break;
                    case "bool": line += $@"this.{c.name} = false;"; break;
                    case "decimal":
                    case "int": line += $@"this.{c.name} = -1;"; break;
                    case "string": line += $@"this.{c.name} = """";"; break;
                    case "DateTime": line += $@"this.{c.name} = DateTime.Now;"; break;
                }

            }
            line += "\n";
            return line;
        }

        public static string getByValuesHelper(Column col)
        {
            string values = "";
            List<Column> columns = new List<Column>(Columns);
            int columnIndex = getColumnIndex(col.name);
            values += $@"{col.type} ";
        
            return values;
        }

        public static string initalVars(int withoutColumnIndex = -1)
        {
            string values = "";
            List<Column> cols = new List<Column>(mappedColumns);
            if (withoutColumnIndex > -1)
            {

                cols.RemoveAt(withoutColumnIndex);

            }
            foreach (Column c in cols)
            {
                
                values += tabs + giveInitialValue(c, true);
            }
            values += "\n";
            return values;
        }

        public static string initialThisValues()
        {
            string values = "";
            foreach (Column c in mappedColumns)
            {
                values += tabs + giveInitialValue(c);
            }
            values += tabs + "Mode = enMode.AddNew;\n" + tabs + "}\n";
            return values;
        }

        public static string thisValues()
        {
            string values = "";
            foreach (Column c in Columns)
            {
                values += tabs + $@"this.{c.name} = {c.name};" + "\n";
            }
            values += tabs + "Mode = enMode.Update;\n";
        
            return values;
        }


        // Actual Functions:

        public static string writeProperties()
        {

            string Properties = "";
            foreach (Column c in mappedColumns)
            {
                Properties += $@"{tabs}public {c.type} {c.name} {{get; set;}}" + "\n";
            }
            Properties += tabs + "public enum enMode {AddNew, Update}\n" + tabs + "public enMode Mode;\n";
            return Properties;
        }

        public static string writeConstructors(bool isEmpty)
        {
            string Constructors = "";
            if (isEmpty)
            {
                Constructors = $@"public {className}()
            {{
{initialThisValues()}";
            }
            else
            {
                Constructors = $@"private {className}({writeParameters(0, false, true)})
                {{
{thisValues()}
                }}";
            }
            return Constructors;
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
            string Function = $@"public static bool {FunctionName}({Columns[columnIndex].type} {Columns[columnIndex].name})
            {{
                return {DALName}.{FunctionName}({Columns[columnIndex].name});
                
            }}
";
            return Function;
        }

        public static string getByFunc(Column col)
        {
            int columnIndex = getColumnIndex(col.name);
            string FunctionName = "";
            if (columnIndex == 0)
            {
                FunctionName = $@"get{objectName}ByID";
            }
            else FunctionName = $@"get{objectName}By{col.name}";
            string Function = $@"
            public static cls{objectName} {FunctionName}({col.type} {col.name})
            {{
{initalVars(columnIndex)}
                if({DALName}.{FunctionName}({writeParametersToSend(true, columnIndex)}))
                {{
                    return new cls{objectName}({writeParametersToSend()});
                }}
                    else return null;
            }}
";
            return Function;
        }
         
        public static string addFunc()
        {
            string FunctionName = $@"_add{objectName}";
            string Function = $@"private bool {FunctionName}()
            {{
                this.{Columns[0].name} = {DALName}.add{objectName}({writeParametersToSend(false, 0)});
                return this.{Columns[0].name} > 0;
            }}";

            return Function;
        }

        public static string updateFunc()
        {
            string FunctionName = $@"_update{objectName}";

            string Function = $@"private bool {FunctionName}()
            {{
                return {DALName}.update{objectName}({writeParametersToSend(false)});
            }}";

            return Function;
        }
        
        public static string deleteFunc(Column col)
        {
            string FunctionName = $@"delete{objectName}";
            string secondFuncName = "";

            if (getColumnIndex(col.name) == 0) secondFuncName = $@"is{objectName}ExistByID";
            else secondFuncName = $@"is{objectName}ExistBy{col.name}";

            string Function = $@"
        public static bool {FunctionName}({col.type} {col.name})
        {{
            if({secondFuncName}({col.name}))
            {{
                return {DALName}.delete{objectName}({col.name});
            }}
            else return false;
        }}
";
            return Function;
        }

        public static string getAllFunc()
        {
            string FunctionName = "getAll";
            string Function = $@"
            public static DataTable {FunctionName}()
            {{
                return {DALName}.{FunctionName}();
            }}

";
            return Function;
        }

        public static string getAllByFunc(Column col)
        {
            string FunctionName = "getAllBy" + col.name;
            string secondFuncName = "";
            if (getColumnIndex(col.name) == 0) secondFuncName = $@"is{objectName}ExistByID";
            else secondFuncName = $@"is{objectName}ExistBy{col.name}";

            string Function = $@"
            public static DataTable {FunctionName}({col.type} {col.name})
            {{
                if({secondFuncName}({col.name}))
                {{
                    return {DALName}.{FunctionName}({col.name});
                }}
                return new DataTable();
            }}

";
            return Function;
        }


        public static string saveFunc()
        {
            string FunctionName = "Save";
            string Function = $@"
            public bool {FunctionName}()
            {{
                switch (Mode)
                    {{
                        case enMode.AddNew:

                            if (_add{objectName}())
                            {{

                                Mode = enMode.Update;
                                return true;
                            }}
                            else
                            {{
                                return false;
                            }}

                        case enMode.Update: return _update{objectName}();
                    }}
                return true;        
            }}";
            return Function;
        }

        public static string classStructure(string injectedString)
        {
            string classStructure = $@"using DAL;
using System;
using System.Data;
using DAL;
namespace BLL
{{
    public class cls{objectName}
    {{

    {injectedString}

    }}
}}
";
            return classStructure;
        }
       
    }
}




