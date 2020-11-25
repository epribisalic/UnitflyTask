using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using StringClass;
using ClassLibrary;

class Program
{   
    
    // combined options -> use the list in the LogFileColletion and brute force filter the needed results (a new list so the full list is preserved)

    static void Main(string[] args)
    {
        // uncomment SingleSearch or MultipleSearch and run

        // the SingleSearch program allows filtering through ONE of the options, not combining them
        // SingleSearch();

        // the program MultipleSearch allows filtering results using multiple options on the current search results
        MultipleSearch();
    }

    static void SingleSearch()
    {
        // for printing Croatian letters correctly using Console.WriteLine()
        Console.OutputEncoding = Encoding.UTF8;

        // retrieve the project folder (UnitflyTask) which contains ShowCase
        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
        // Console.WriteLine(projectDirectory);

        // combine the path above with the folder Data (in projectDirectory) where we saved the txt file
        // the name of the file could be a variable (in case there is more than 1)
        string path = Path.Combine(projectDirectory, @"Data\log20201104.txt");
        // Console.WriteLine(path);
        string file = File.ReadAllText(path);
        // Console.WriteLine(file);

        // parse the txt file -> retrieve all the info we need
        LogFileCollection files = StringLibrary.ParseLog(file);

        // options to choose from
        while (true)
        {
            string startMessage = "\nPlease choose one of the options listed below using the number preceeding the desired option\n" +
                "\n1. View the entire log file" +
                "\n2. Filter by date and time" +
                "\n3. Filter by log message type" +
                "\n4. Filter by M-Files vault identifier" +
                "\n5. Filter by SQL integration\n" +
                "\nType EXIT to exit the program\n";
            Console.WriteLine(startMessage);
            Console.WriteLine("Desired option: ");
            string opt = Console.ReadLine();

            if (opt.ToUpper() == "EXIT") break;

            switch (opt)
            {
                case "1": files.PrintList(); break;
                case "2":
                    Console.WriteLine("Date format is yyyy-mm-dd and time format is hh:mm:ss (timezone is set as +01:00)");
                    DateTimeOffset f = files.lfList[0].logDate;
                    DateTimeOffset l = files.lfList[files.lfList.Count - 1].logDate;

                    Console.WriteLine($"First date and time available: {f.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}");
                    Console.WriteLine($"Last date and time available: {l.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}");

                    Regex rgDate = new Regex(@"\d{4}\-(?:0[1-9]|1[0-2])\-(?:0[1-9]|[12][0-9]|3[01])\s*$");
                    Regex rgTime = new Regex(@"(?:0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](?:\.[\d]{1,3})?\s*$");

                    string sDate, sTime, eDate, eTime;
                    do
                    {
                        Console.WriteLine("Enter the starting date: (yyyy-mm-dd)");
                        sDate = Console.ReadLine();

                    } while (!rgDate.IsMatch(sDate));

                    do
                    {
                        Console.WriteLine("Enter the starting time: (hh:mm:ss or hh:mm:ss.fff)");
                        sTime = Console.ReadLine();

                    } while (!rgTime.IsMatch(sTime));

                    do
                    {
                        Console.WriteLine("Enter the ending date: (yyyy-mm-dd)");
                        eDate = Console.ReadLine();

                    } while (!rgDate.IsMatch(eDate));

                    do
                    {
                        Console.WriteLine("Enter the ending time: (hh:mm:ss or hh:mm:ss.fff)");
                        eTime = Console.ReadLine();

                    } while (!rgTime.IsMatch(eTime));

                    // combine the strings into the required format
                    files.FilterTime(sDate + " " + sTime + " +01:00", eDate + " " + eTime + " +01:00");

                    break;
                case "3":
                    Console.WriteLine("Available types are: \n");
                    // sorting the keys
                    List<String> logKeys = files.logTypeDict.Keys.ToList();
                    logKeys.Sort();
                    foreach (string s in logKeys) Console.WriteLine(s);
                    // input
                    Console.WriteLine("\nInput desired types separated by space: ");
                    string logTypes = Console.ReadLine();
                    // result
                    files.FilterLog(logTypes);
                    break;
                case "4":
                    Console.WriteLine("Available M-File vault identifiers are: \n");
                    // sorting the keys
                    List<String> mvKeys = files.mVDict.Keys.ToList();
                    mvKeys.Sort();
                    foreach (string s in mvKeys) Console.WriteLine(s);
                    // input
                    Console.WriteLine("\nInput desired vaults separated by space: ");
                    string mvTypes = Console.ReadLine();
                    // result
                    files.FilterMV(mvTypes);
                    break;
                case "5":
                    Console.WriteLine("Available SQL integrations are: \n");
                    // sorting the keys
                    List<String> sqlKeys = files.sqlDict.Keys.ToList();
                    sqlKeys.Sort();
                    foreach (string s in sqlKeys) Console.WriteLine(s);
                    // input
                    Console.WriteLine("\nInput desired integrations separated by comma: ");
                    string sqlTypes = Console.ReadLine();
                    // result
                    files.FilterSQL(sqlTypes);
                    break;
                default: break;
            }
        }
    }

    static void MultipleSearch()
    {
        // for printing Croatian letters correctly using Console.WriteLine()
        Console.OutputEncoding = Encoding.UTF8;

        // retrieve the project folder (UnitflyTask) which contains ShowCase
        string workingDirectory = Environment.CurrentDirectory;
        string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
        // Console.WriteLine(projectDirectory);

        // combine the path above with the folder Data (in projectDirectory) where we saved the txt file
        // the name of the file could be a variable (in case there is more than 1)
        string path = Path.Combine(projectDirectory, @"Data\log20201104.txt");
        // Console.WriteLine(path);
        string file = File.ReadAllText(path);
        // Console.WriteLine(file);

        // parse the txt file -> retrieve all the info we need
        LogClass logs = new LogClass(file);

        // options to choose from
        while (true)
        {
            string startMessage = "\nPlease choose one of the options listed below using the number preceeding the desired option\n" +
                "\n1. View the entire log file (does not interfere with current search results)" +
                "\n2. Filter by date and time" +
                "\n3. Filter by log message type" +
                "\n4. Filter by M-Files vault identifier" +
                "\n5. Filter by SQL integration" +
                "\n6. Undo all filtering" +
                "\n\nType EXIT to exit the program\n";
            Console.WriteLine(startMessage);
            Console.WriteLine("Desired option: ");
            string opt = Console.ReadLine();

            if (opt.ToUpper() == "EXIT") break;

            // options - filtering is done on search results that are shown until we remove all the filters and start over
            switch (opt)
            {
                case "1": logs.PrintLogs(); break;
                case "2":
                    DateTimeOffset f = logs.searchList[0].logDate;
                    DateTimeOffset l = logs.searchList[logs.searchList.Count - 1].logDate;

                    Console.WriteLine($"First date and time available: {f.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}");
                    Console.WriteLine($"Last date and time available: {l.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")}");

                    // regex for date and time - we follow the pattern in txt file
                    Regex rgDate = new Regex(@"\d{4}\-(?:0[1-9]|1[0-2])\-(?:0[1-9]|[12][0-9]|3[01])\s*$");
                    Regex rgTime = new Regex(@"(?:0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](?:\.[\d]{1,3})?\s*$");

                    string sDate, sTime, eDate, eTime;
                    do
                    {
                        Console.WriteLine("Enter the starting date: (yyyy-mm-dd)");
                        sDate = Console.ReadLine();

                    } while (!rgDate.IsMatch(sDate));

                    do
                    {
                        Console.WriteLine("Enter the starting time: (hh:mm:ss or hh:mm:ss.fff)");
                        sTime = Console.ReadLine();

                    } while (!rgTime.IsMatch(sTime));

                    do
                    {
                        Console.WriteLine("Enter the ending date: (yyyy-mm-dd)");
                        eDate = Console.ReadLine();

                    } while (!rgDate.IsMatch(eDate));

                    do
                    {
                        Console.WriteLine("Enter the ending time: (hh:mm:ss or hh:mm:ss.fff)");
                        eTime = Console.ReadLine();

                    } while (!rgTime.IsMatch(eTime));

                    // combine the strings into the required format
                    logs.TimeInterval(sDate + " " + sTime + " +01:00", eDate + " " + eTime + " +01:00");

                    break;
                case "3":
                    // input
                    Console.WriteLine("\nInput desired log types separated by space: ");
                    string[] logTypes = Console.ReadLine().Split();
                    // result
                    logs.LogTypeSearch(logTypes);
                    break;
                case "4":
                    // input
                    Console.WriteLine("\nInput desired M-Files vault identifiers separated by space: ");
                    string[] mvTypes = Console.ReadLine().Split();
                    // result
                    logs.MVaultSearch(mvTypes);
                    break;
                case "5":
                    // input
                    Console.WriteLine("\nInput desired SQL integrations separated by comma: ");
                    string[] sqlTypes = Console.ReadLine().Split(",");
                    // result
                    logs.SQLSearch(sqlTypes);
                    break;
                case "6":
                    logs.Reset();
                    Console.WriteLine("\nAll filters removed!\n");
                    break; 
                default: break;
            }
        }
    }
}