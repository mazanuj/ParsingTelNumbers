using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var infoHolders = resultData as IList<InfoHolder> ?? resultData.ToList();
            infoHolders = infoHolders
                .Where(x => x != null &&
                            !string.IsNullOrEmpty(x.Phone))
                .GroupBy(holder => holder.Phone)
                .Select(x => x.First())
                .ToList();

            using (var sw = new StreamWriter("data.txt", true, Encoding.GetEncoding("windows-1251")))
            {
                sw.WriteLine(DateTime.Now.Date.ToString("d"));
                sw.WriteLine("САЙТ\tКАТЕГОРИЯ\tГОРОД\tИМЯ\tТЕЛЕФОН");
                foreach (var j in infoHolders)
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", j.Site, j.Direction, j.City, j.Name, j.Phone);
            }

            Console.ReadKey();
        }
    }
}