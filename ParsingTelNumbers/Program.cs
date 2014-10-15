using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ParsingTelNumbers.Config;

namespace ParsingTelNumbers
{
    internal static class Program
    {
        private static void Main()
        {
            var getAllDataTask = AllDataGetter.GetData();
            Task.WaitAll(getAllDataTask);
            var resultData = getAllDataTask.Result;

            using (var sw = new StreamWriter("data.xls", true, Encoding.GetEncoding("windows-1251")))
            {
                sw.WriteLine(DateTime.Now.Date.ToString("d"));
                sw.WriteLine("САЙТ\tКАТЕГОРИЯ\tГОРОД\tИМЯ\tТЕЛЕФОН");
                foreach (var i in resultData)
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", i.Site, i.Direction, i.City, i.Name, i.Phone);
            }

            Console.ReadKey();
        }
    }
}