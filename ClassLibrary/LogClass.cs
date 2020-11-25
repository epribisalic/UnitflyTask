using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary
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
            string final = this.logDate.ToString(df) + $" [{this.logType}] {{{this.mVault}}} [{this.sqlIntegration}]\n{message}\n";
            Console.WriteLine(final);
        }
    }

    public class LogClass
    {
        public List<LogFile> LogList;
        public List<LogFile> searchList;

        public LogClass(string s)
        {
            this.LogList = new List<LogFile>();

            String pattern = @"(\d+[\-]\d+[\-]\d+\s+\d+\:\d+\:\d+\.\d+\s+[\+-]?\d+\:\d+)\s+\[(\w+)\]\s+\{(.*?)\}\s+\[(.*?)\]\s+(.*)";
            var rx = new Regex(pattern, RegexOptions.Compiled);
            var matches = rx.Matches(s);

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
                this.LogList.Add(lf);

                this.searchList = new List<LogFile>();

                this.Reset();
            }
        }

        // reset search results
        public void Reset()
        {
            // clear search list and add all the unfiltered results back in
            this.searchList.Clear();
            foreach(LogFile lf in this.LogList)
            {
                searchList.Add(lf);
            }
        }
        
        // print all logs
        public void PrintLogs()
        {
            Console.WriteLine("\n\n");
            foreach (LogFile lf in this.LogList)
                lf.PrintLog();
            Console.WriteLine("\n\n");
        }
        // print filtered logs
        public void PrintFiltered()
        {
            Console.WriteLine("\n\n");
            foreach (LogFile lf in this.searchList)
                lf.PrintLog();
            Console.WriteLine("\n\n");
        }

        // filtering
        public void TimeInterval(string s, string e)
        {
            // list is sorted (logs added in order due to txt file)
            // using binary search we can find the index of the first and the index of the last element in desired segment

            // parse strings into dates
            DateTimeOffset startDate = DateTimeOffset.Parse(s);
            DateTimeOffset endDate = DateTimeOffset.Parse(e);

            Console.WriteLine("\n\nLogs between " + startDate.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") + " and " + endDate.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") + " are: \n\n");

            // check if segment is valid
            if (startDate > this.searchList[searchList.Count - 1].logDate || endDate < this.searchList[0].logDate || endDate < startDate)
            {
                Console.WriteLine("\nNo results found with given segment borders.\n");
            }
            else
            {
                // if we went out of range, reset the index
                int startInd = this.BS(startDate);
                if (startInd == -1) startInd = 0;

                int endInd = this.BS(endDate);
                if (endInd == -1) endInd = this.searchList.Count - 1;

                //Console.WriteLine("Indexes: " + startInd + " and " + endInd);

                // get filtered logs
                List<LogFile> temp = new List<LogFile>();
                for (int i = startInd; i < endInd; i++)
                    temp.Add(this.searchList[i]);

                searchList = temp;

                this.PrintFiltered();
            }

        }

        public int BS(DateTimeOffset border)
        {
            // simple binary search with modification for segments on search list
            int min = 0;
            int max = this.searchList.Count - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                if (border == this.searchList[mid].logDate)
                {
                    return mid;
                }
                else if (border < this.searchList[mid].logDate)
                {
                    if (mid - 1 >= 0)
                    {
                        // for segments -> check the left element (if it exists)
                        if (border > this.searchList[mid - 1].logDate) { return mid; }
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


        public void LogTypeSearch(string[] args)
        {
            // using FindAll for filtering
            List<LogFile> res = this.searchList.FindAll(f => (args.Contains(f.logType)));
            this.searchList = res;
            this.PrintFiltered();
        }

        public void MVaultSearch(string[] args)
        {
            List<LogFile> res = this.searchList.FindAll(f => (args.Contains(f.mVault)));
            this.searchList = res;
            this.PrintFiltered();
        }
        public void SQLSearch(string[] args)
        {
            List<LogFile> res = this.searchList.FindAll(f => (args.Contains(f.sqlIntegration)));
            this.searchList = res;
            this.PrintFiltered();
        }


    }

}
