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
            clsHelper.Columns = clsHelper.getColumnsNameAndType();
            Run();

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
            string DALFuncs = "";
            string BLLFuncs = clsPresentation.initiateBLL();
            string SPs = "";
            Console.Write("\nFor DAL, BLL, And Stored Procedures:\n");

            // Get By:

            List<string> getByColumns = clsPresentation.getBy();
            foreach (string colName in getByColumns)
            {
                clsHelper.Column C = clsHelper.makeMappedColumnByName(colName);
                clsHelper.Column column = clsHelper.makeColumnByName(colName);
                SPs += clsSPs.selectByColumnSP(column);
                DALFuncs += clsDAL.getRecordByColumnFunc(C);
                BLLFuncs += clsBLL.getByFunc(C);
            }

            // Update:

            Console.Write("update? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.updateFunc(clsHelper.mappedColumns[0]);
                BLLFuncs += clsBLL.updateFunc();
                SPs += clsSPs.updateSP();


            }

            // Delete:

            Console.Write("delete? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                Column C = clsHelper.mappedColumns[0];
                DALFuncs += clsDAL.deleteFunc(C);
                BLLFuncs += clsBLL.deleteFunc(C);
                SPs += clsSPs.deleteSP();
            }

            // Add:

            Console.Write("add? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.addFunc();
                BLLFuncs += clsBLL.addFunc();
                SPs += clsSPs.addSP();

            }

            // isExist:

            Console.Write("isExist? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                List<string> Columns = clsPresentation.existBy();
                foreach (string colName in Columns)
                {
                    clsHelper.Column C = clsHelper.makeMappedColumnByName(colName);
                    clsHelper.Column column = clsHelper.makeColumnByName(colName);
                    
                    DALFuncs += clsDAL.isExistsFunc(C);
                    BLLFuncs += clsBLL.isExistsFunc(C);
                    SPs += clsSPs.isExistByColumnSP(C);

                }
            }

            // getAll:


            Console.Write("getAll? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                DALFuncs += clsDAL.getAllFunc();
                BLLFuncs += clsBLL.getAllFunc();
                SPs += clsSPs.selectAllSP();
                Console.WriteLine("GetAll Method Generated, do you want 'GetAll By' ? (yes/no): ");
                answer = Console.ReadLine();
                if (answer.ToLower() == "yes" || answer.ToLower() == "y")
                {
                    List<string> Columns = clsPresentation.getAllBy();
                    foreach (string colName in Columns)
                    {
                        clsHelper.Column C = clsHelper.makeMappedColumnByName(colName);
                        clsHelper.Column column = clsHelper.makeColumnByName(colName);
                        SPs += clsSPs.selectAllBySP(column);
                        BLLFuncs += clsBLL.getAllByFunc(C);
                        DALFuncs += clsDAL.getAllByColumnFunc(C);

                    }
                }
            }
            Console.Write("For BLL: Save Method? yes/no: ");
            answer = Console.ReadLine();
            if (answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                BLLFuncs += clsBLL.saveFunc();


            }

            Console.WriteLine("Stored Procedures:");
            Console.WriteLine("\n\n\n" + SPs + "\n\n\n");

            Console.WriteLine("\nDAL:");
            Console.WriteLine("\n\n\n" + clsDAL.classStructure(DALFuncs) + "\n\n\n");
            Console.WriteLine("\nBLL:");
            Console.WriteLine("\n\n\n" + clsBLL.classStructure(BLLFuncs) + "\n\n\n");
            
            }


    }

}
