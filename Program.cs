using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
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

            bool more = false;
            do
            {
                Run();
                Console.Write("\nDo you want to generate code for another table? (yes/no): ");
                string answer = Console.ReadLine();
                if (answer.ToLower() == "yes" || answer.ToLower() == "y")
                {
                    more = true;
                    Console.Clear();
                    AnsiConsole.Write(
                                    new FigletText("ADO Gen Code")
                                        .Centered()
                                        .Color(Color.Green));
                    AnsiConsole.Write(new Rule("[yellow]Welcome in Joudi's Code Generator[/]").Justify(Justify.Left));
                    AnsiConsole.MarkupLine("[grey]This tool generates DAL and BLL CRUD ADO.NET for you.[/]");
                    AnsiConsole.MarkupLine("[red]Notice:[/] Please ensure database settings are configured in [cyan]clsHelper.connectionString[/].\n");

                }
                else
                {
                    more = false;
                }
            } while (more);

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

        public static void Run()
        {


            Console.Write("Enter Table Name: ");
            clsHelper.tableName = Console.ReadLine();

            int count = 0;
            do
            {
                clsHelper.Columns = clsHelper.getColumnsNameAndType();
                if (clsHelper.Columns.Count == 0)
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
            clsHelper.objectName = Console.ReadLine();

            string answer = "yes";
            StringBuilder DALFuncs = new StringBuilder();
            StringBuilder BLLFuncs = new StringBuilder();
            StringBuilder SPs = new StringBuilder();
            BLLFuncs.Append(clsPresentation.initiateBLL());
            Console.Write("\nFor DAL, BLL, And Stored Procedures:\n");

            // Get By:

            List<string> getByColumns = clsPresentation.getBy();
            foreach (string colName in getByColumns)
            {
                clsHelper.Column column = clsHelper.makeColumnByName(colName);
                SPs.Append(clsSPs.selectByColumnSP(column));
                DALFuncs.Append(clsDAL.getRecordByColumnFunc(column));
                BLLFuncs.Append(clsBLL.getByFunc(column));
            }

            // Update:

            Console.Write("update? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs.Append(clsDAL.updateFunc(clsHelper.mappedColumns[0]));
                BLLFuncs.Append(clsBLL.updateFunc());
                SPs.Append(clsSPs.updateSP());


            }

            // Delete:

            Console.Write("delete? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                Column C = clsHelper.mappedColumns[0];
                DALFuncs.Append(clsDAL.deleteFunc(C));
                BLLFuncs.Append(clsBLL.deleteFunc(C));
                SPs.Append(clsSPs.deleteSP());
            }

            // Add:

            Console.Write("add? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs.Append(clsDAL.addFunc());
                BLLFuncs.Append(clsBLL.addFunc());
                SPs.Append(clsSPs.addSP());

            }

            // isExist:

            Console.Write("isExist? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                List<string> Columns = clsPresentation.existBy();
                foreach (string colName in Columns)
                {
                    clsHelper.Column column = clsHelper.makeColumnByName(colName);

                    DALFuncs.Append(clsDAL.isExistsFunc(column));
                    BLLFuncs.Append(clsBLL.isExistsFunc(column));
                    SPs.Append(clsSPs.isExistByColumnSP(column));

                }
            }

            // getAll:


            Console.Write("getAll? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs.Append(clsDAL.getAllFunc());
                BLLFuncs.Append(clsBLL.getAllFunc());
                SPs.Append(clsSPs.selectAllSP());
                Console.WriteLine("GetAll Method Generated, do you want 'GetAll By' ? (yes/no): ");
                answer = Console.ReadLine();
                if (answer.ToLower() == "yes" || answer.ToLower() == "y")
                {
                    List<string> Columns = clsPresentation.getAllBy();
                    foreach (string colName in Columns)
                    {
                        clsHelper.Column column = clsHelper.makeColumnByName(colName);
                        SPs.Append(clsSPs.selectAllBySP(column));
                        BLLFuncs.Append(clsBLL.getAllByFunc(column));
                        DALFuncs.Append(clsDAL.getAllByColumnFunc(column));

                    }
                }
            }
            Console.Write("For BLL: Save Method? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                BLLFuncs.Append(clsBLL.saveFunc());


            }

            Console.WriteLine("Stored Procedures:");
            Console.WriteLine("\n\n\n" + SPs + "\n\n\n");

            Console.WriteLine("\nDAL:");
            Console.WriteLine("\n\n\n" + clsDAL.classStructure(DALFuncs) + "\n\n\n");
            Console.WriteLine("\nBLL:");
            Console.WriteLine("\n\n\n" + clsBLL.classStructure(BLLFuncs) + "\n\n\n");

            saveFiles(DALFuncs, BLLFuncs, SPs);


        }

        public static void saveFiles(StringBuilder DALFuncs, StringBuilder BLLFuncs, StringBuilder SPs)
        {
            try
            {
                string projectDirectory = ConfigurationManager.AppSettings["projectDirectory"];
                if (!Directory.Exists(projectDirectory))
                {
                    Directory.CreateDirectory(projectDirectory);
                }
                // file paths
                string spPath = Path.Combine(projectDirectory, $"{clsHelper.tableName}_SPs.sql");
                string dalPath = Path.Combine(projectDirectory, $"{clsHelper.className}DAL.cs");
                string bllPath = Path.Combine(projectDirectory, $"{clsHelper.className}.cs");

                // Save files
                File.WriteAllText(spPath, SPs.ToString());
                File.WriteAllText(dalPath, clsDAL.classStructure(DALFuncs));
                File.WriteAllText(bllPath, clsBLL.classStructure(BLLFuncs));

                AnsiConsole.MarkupLine($"\n[green]✔ Success:[/] Files generated and saved successfully!");
                AnsiConsole.MarkupLine($"[grey]Stored Procedures:[/] [cyan]{spPath}[/]");
                AnsiConsole.MarkupLine($"[grey]DAL Class:[/] [cyan]{dalPath}[/]");
                AnsiConsole.MarkupLine($"[grey]BLL Class:[/] [cyan]{bllPath}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"\n[red]❌ Error while saving files:[/] {ex.Message}");
            }

        }
    }
}
