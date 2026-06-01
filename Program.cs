using System;
using System.Collections.Generic;
using Spectre.Console;
using static CodeGenarator.clsHelper;

namespace CodeGenarator
{
    internal class Program
    {

        static void Main(string[] args)
        {






            AnsiConsole.Write(
                                    new FigletText("ADO Gen Code")
                                        .Centered()
                                        .Color(Color.Green));

            AnsiConsole.Write(new Rule("[yellow]Welcome in Joudi's Code Generator[/]").Justify(Justify.Left));
            AnsiConsole.MarkupLine("[grey]This tool generates DAL and BLL CRUD ADO.NET for you.[/]");
            AnsiConsole.MarkupLine("[red]Notice:[/] Please ensure database settings are configured in [cyan]clsHelper.connectionString[/].\n");

            Console.Write("Enter Table Name: ");
            clsHelper.tableName = Console.ReadLine();

            int count = 0;
            do
            {
                clsHelper.Columns = clsHelper.getColumnsNameAndType();
                if(clsHelper.Columns.Count == 0)
                {
                    Console.WriteLine("No Columns Found, Please Enter a Valid Table Name: ");
                    clsHelper.tableName = Console.ReadLine();

                }
                else
                {
                    count = clsHelper.Columns.Count;
                }

            } while (count == 0);
            clsHelper.mappedColumns = clsHelper.mappingTheColumns(clsHelper.Columns);

            Console.Write("Enter The Class Name For Both DAL And BLL (cls First will be added on it): ");
            clsHelper.objectName = Console.ReadLine() ;

            string answer = "yes";
            string DALFuncs = "";
            string BLLFuncs = initiateBLL();

            Console.Write("\nFor Both DAL And BLL:\n");
            List<string> getByColumns = getBy();
            foreach (string col in getByColumns)
            {
                DALFuncs += clsDAL.getRecordByColumnFunc(col);
                clsHelper.Column c = clsHelper.mappedColumns[clsHelper.getColumnIndex(col)];
                BLLFuncs += clsBLL.getByFunc(c);
            }


            Console.Write("update? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.updateFunc(clsHelper.Columns[0].name);
                BLLFuncs += clsBLL.updateFunc();

            }

            Console.Write("delete? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.deleteFunc(clsHelper.Columns[0].name);
                BLLFuncs += clsBLL.deleteFunc(clsHelper.mappedColumns[0]);

            }

            Console.Write("add? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.addFunc(clsHelper.Columns[0].name);
                BLLFuncs += clsBLL.addFunc();


            }

            Console.Write("isExist? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                List<string> Columns = existBy();
                foreach (string col in Columns)
                {
                    DALFuncs += clsDAL.isExistsFunc(col);
                    BLLFuncs += clsBLL.isExistsFunc(col);
                }
            }



            Console.Write("getAll? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.getAllFunc();
                BLLFuncs += clsBLL.getAllFunc();
                Console.WriteLine("GetAll Method Generated, do you want 'GetAll By' ? (yes/no): ");
                answer = Console.ReadLine();
                if(answer.ToLower() == "yes" || answer.ToLower() == "y")
                {
                    List<string> Columns = getAllBy();
                    foreach (string col in Columns)
                    {
                        DALFuncs += clsDAL.getAllByColumnFunc(col);
                        clsHelper.Column c = clsHelper.mappedColumns[clsHelper.getColumnIndex(col)];
                        BLLFuncs += clsBLL.getAllByFunc(c);
                    }
                }
            }
          
            Console.WriteLine("\nDAL:");
            Console.WriteLine("\n\n\n" + clsDAL.classStructure(DALFuncs) + "\n\n\n");
            Console.WriteLine("\nBLL:");
            Console.WriteLine("\n\n\n" + clsBLL.classStructure(BLLFuncs) + "\n\n\n");

            AnsiConsole.WriteLine();
            var signaturePanel = new Panel(
                new Markup(
                    "[bold white]Developed with ❤️ by:[/] [bold green]Joudi[/]\n" +
                    "[bold white]Telegram:[/] [blue]@Joudi_Adeeb[/]\n" +
                    "[bold white]LinkedIn:[/] [blue]linkedin.com/in/joudi-mohammad-002685283[/]"
                )
            )
            .Header("[yellow]Generation Completed![/]")
            .BorderColor(Color.Gold1)
            .Padding(2, 1)
            .RoundedBorder()
            .Expand();
            AnsiConsole.Write(signaturePanel);
            AnsiConsole.MarkupLine("\n[grey]Press any key to exit...[/]");
            Console.ReadKey();
        }

        // DAL Functions
        public static List<string> getBy()
        {
            List<string> Columns = new List<string>();
            string answer = "yes";
            do
            {
                Console.Write("Enter the column name to generate 'Get By' method for it: ");
                string columnName = Console.ReadLine();

                if (getColumnIndex(columnName) < 0)
                {
                    Console.WriteLine("Column not Found.\n");
                }
                else
                {
                    
                    Columns.Add(columnName);

                }
                Console.Write("Do you want to add another 'Get By' method? yes/no: ");
                answer = Console.ReadLine();

            } while (answer.ToLower() == "yes" || answer.ToLower() == "y");
            return Columns;
        }

        public static List<string> existBy()
        {
            List<string> Columns = new List<string>();

            string answer = "yes";
            do
            {
                Console.Write("Enter the column name to generate 'isExist By' method for it: ");
                string columnName = Console.ReadLine();

                if (getColumnIndex(columnName) < 0)
                {
                    Console.WriteLine("Column not Found.\n");
                }
                else
                {
                    Columns.Add(columnName);

                }
                Console.WriteLine("Do you want to add another 'isExist By' method? yes/no");
                answer = Console.ReadLine();

            } while (answer.ToLower() == "yes" || answer.ToLower() == "y");
            return Columns;
        }


        public static List<string> getAllBy()
        {
            List<string> Columns = new List<string>();

            string answer = "yes";
            do
            {
                Console.Write("Enter the column name to generate 'getAll By' method for it: ");
                string columnName = Console.ReadLine();

                if (getColumnIndex(columnName) < 0)
                {
                    Console.WriteLine("Column not Found.\n");
                }
                else
                {
                    Columns.Add(columnName);

                }
                Console.WriteLine("Do you want to add another 'getAll By' method? yes/no: ");
                answer = Console.ReadLine();

            } while (answer.ToLower() == "yes" || answer.ToLower() == "y");
            return Columns;
        }

        // BLL Functions

        public static string initiateBLL()
        {
            string answer = "yes";
            string BLLFuncs = "";
            Console.Write("For BLL: Generate Properties? (yes/no): ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                BLLFuncs += clsBLL.writeProperties();
            }

            Console.Write("For BLL: Generate Empty Constructor? (yes/no): ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                BLLFuncs += clsBLL.writeConstructors(true);
            }
            Console.Write("For BLL: Generate Full Constructor? (yes/no): ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                BLLFuncs += clsBLL.writeConstructors(false);
            }

            return BLLFuncs;
        }

    }
}
