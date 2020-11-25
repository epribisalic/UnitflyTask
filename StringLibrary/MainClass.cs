using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace StringClass
{
    public class LogFile
    {
        // this class represents a single line in txt file - one Log
        // in order to shorten the code, all fields are public

        public DateTimeOffset logDate;
        public string logType;
        public string mVault;
        public string sqlIntegration;
        public string message;
        
        // constructor - the only one we use
        public LogFile(DateTimeOffset logDate, string logType, string mVault, string sqlIntegration, string message)
        {
            this.logDate = logDate;
            this.logType = logType;
            this.mVault = mVault;
            this.sqlIntegration = sqlIntegration;
            this.message = message;
        }
        // printing the Log - message is printed in the new line and there is a new line at the end of the message for easier preview
        public void PrintLog()
        {
            string df = "yyyy-MM-dd HH:mm:ss.fff zzz"; // date and time format we use
            string final = this.logDate.ToString(df) + $" [{this.logType}] {{{this.mVault}}} [{this.sqlIntegration}]\n{message}\n" ;
            Console.WriteLine(final);
        }
    }
    public class LogFileCollection
    {
        // in order to filter through result efficiently, we can use dictionaries 
        // the list contains all of the logs from txt file, while dictionaries contain keys which respond to provided tags in brackets and curly braces
        // there aren't many different tags for each category 

        // again, fields are public for simplicity
        public List<LogFile> lfList;
        public Dictionary<string, List<LogFile>> logTypeDict;
        public Dictionary<string, List<LogFile>> mVDict;
        public Dictionary<string, List<LogFile>> sqlDict;

        // default constructor
        public LogFileCollection() 
        {
            this.lfList = new List<LogFile>();
            this.logTypeDict = new Dictionary<string, List<LogFile>>();
            this.mVDict = new Dictionary<string, List<LogFile>>();
            this.sqlDict = new Dictionary<string, List<LogFile>>();
        }

        // function which prints the entire list (all of the logs)
        public void PrintList()
        {
            foreach (LogFile lf in this.lfList)
                lf.PrintLog();
        }
        // function which prints logs of a given type
        public void PrintLog(string arg)
        {
            foreach (LogFile lf in this.logTypeDict[arg])
                    lf.PrintLog();
        }
        // function which prints logs of a specific M-Files vault identifier
        public void PrintMV(string arg)
        {
            foreach (LogFile lf in this.mVDict[arg])
                lf.PrintLog();
        }
        // function which prints logs of a specific SQL integration
        public void PrintSQL(string arg)
        {
            foreach (LogFile lf in this.sqlDict[arg])
                lf.PrintLog();
        }

        // function which filters through logs in list and printing them depending on the given segment
        public void FilterTime(string starting, string ending)
        {
            // lfList is sorted (logs added in order due to txt file), if not, sort it using dates
            // using binary search we can find the index of the first and the index of the last element in desired segment

            // parse strings into dates
            DateTimeOffset startDate = DateTimeOffset.Parse(starting);
            DateTimeOffset endDate = DateTimeOffset.Parse(ending);

            Console.WriteLine("\n\nLogs between " + startDate.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") + " and " + endDate.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") + " are: \n\n");

            // check if segment is valid
            if(startDate > this.lfList[lfList.Count-1].logDate || endDate < this.lfList[0].logDate || endDate < startDate)
            {
                Console.WriteLine("\nNo results found with given segment borders.\n");
            }
            else
            {
                // if we went out of range, reset the index
                int startInd = this.BSLogFile(startDate);
                if (startInd == -1) startInd = 0;

                int endInd = this.BSLogFile(endDate);
                if (endInd == -1) endInd = this.lfList.Count - 1;

                // print filtered logs
                for (int i = startInd; i < endInd; i++)
                    this.lfList[i].PrintLog();

                // perhaps a better solution -> red black tree?
            }

        }
        // helper function for FilterTime which uses Binary Search algorithm
        public int BSLogFile(DateTimeOffset border)
        {
            // simple binary search with modification for segments
            int min = 0;
            int max = this.lfList.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (border == this.lfList[mid].logDate)
                {
                    return mid;
                }
                else if (border < this.lfList[mid].logDate)
                {
                    if (mid - 1 >= 0)
                    {
                        // for segments -> check the left element (if it exists)
                        if (border > this.lfList[mid - 1].logDate) { return mid; }
                    }
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }
            return -1;
        }

        // function which handles filtering logs (1 or more given log types)
        public void FilterLog(string s)
        {
            Console.WriteLine("\n\nListing the selected logs... \n");
            string[] res = s.Split(" ").Distinct().ToArray();
            foreach (string r in res)
            {
                // print only the types that are valid
                if (this.logTypeDict.ContainsKey(r.ToUpper()))
                {
                    PrintLog(r.ToUpper());
                }

            }
        }
        // function which handles filtering logs (1 or more given M-Files vault identifiers)
        public void FilterMV(string s)
        {
            Console.WriteLine("\n\nListing the selected logs... \n");
            string[] res = s.Split(" ").Distinct().ToArray();
            foreach (string r in res)
            {
                // print only the types that are valid
                if (this.mVDict.ContainsKey(r.ToUpper()))
                {
                    PrintMV(r.ToUpper());
                }
            }
        }
        // function which handles filtering logs (1 or more SQL integrations)
        // if there are spaces in the tag names, we can use comma as a separator
        public void FilterSQL(string s)
        {
            Console.WriteLine("\n\nListing the selected logs... \n");
            string[] res = s.Split(",").Distinct().ToArray();
            foreach (string r in res)
            {
                // print only the types that are valid
                if (this.sqlDict.ContainsKey(r))
                {
                    PrintSQL(r);
                }
            }
        }
    }



    public static class StringLibrary
    {
        public static LogFileCollection ParseLog(this string str)
        {
            LogFileCollection collectLogFiles = new LogFileCollection();

            // string df = "yyyy-MM-dd HH:mm:ss.fff zzz"; // date format

            // use Regex to parse each line in txt file
            // date, time and offset (1), log type(2), M-files vault(3), SQL integration(4), message(5)

            String pattern = @"(\d+[\-]\d+[\-]\d+\s+\d+\:\d+\:\d+\.\d+\s+[\+-]?\d+\:\d+)\s+\[(\w+)\]\s+\{(.*?)\}\s+\[(.*?)\]\s+(.*)";
            var rx = new Regex(pattern, RegexOptions.Compiled);
            var matches = rx.Matches(str);

            LogFile lf; // form the strings into LogFile
            string logDate, logType, mVault, sqlInt, mess; // strings that we read from regex groups

            foreach (Match match in matches)
            {
                // get the data for a specific line
                logDate = match.Groups[1].Value;
                logType = match.Groups[2].Value;
                mVault = match.Groups[3].Value;
                sqlInt = match.Groups[4].Value;
                mess = match.Groups[5].Value;

                // string to date -> we can easily manipulate dates (comparison etc)
                DateTimeOffset offsetDate = DateTimeOffset.Parse(logDate);
                // Console.WriteLine(offsetDate.ToString(df));

                // create object and add it to the list
                lf = new LogFile(offsetDate, logType, mVault, sqlInt, mess);
                collectLogFiles.lfList.Add(lf);

                // add to logType dictionary 
                if(!collectLogFiles.logTypeDict.ContainsKey(logType))
                {
                    collectLogFiles.logTypeDict[logType] = new List<LogFile>();
                }
                collectLogFiles.logTypeDict[logType].Add(lf);

                // add to M-Files dictionary
                if (!collectLogFiles.mVDict.ContainsKey(mVault))
                {
                    collectLogFiles.mVDict[mVault] = new List<LogFile>();
                }
                collectLogFiles.mVDict[mVault].Add(lf);

                // add to SQL integration dictionary
                if (!collectLogFiles.sqlDict.ContainsKey(sqlInt))
                {
                    collectLogFiles.sqlDict[sqlInt] = new List<LogFile>();
                }
                collectLogFiles.sqlDict[sqlInt].Add(lf);
                
                // perhaps make keys all lower/upper case and resolve case sensitivity?
            }

            return collectLogFiles;
            // return the entire object containing everything we need
        }
    }
}

