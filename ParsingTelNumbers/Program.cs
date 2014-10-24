using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ParsingTelNumbers.Config;
using ParsingTelNumbers.XmlWorker;

namespace ParsingTelNumbers
{
    internal static class Program
    {
        private static void Main()
        {
            var getAllDataTask = AllDataGetter.GetData();
            Task.WaitAll(getAllDataTask);
            var resultData = getAllDataTask.Result;

            var tels = DataXmlWorker.GetTels();

            DataXmlWorker.SetTels(resultData
                .Where(x => x != null &&
                            !string.IsNullOrEmpty(x.Phone) &&
                            Regex.IsMatch(x.Phone, @"^380\d{9}$"))
                .GroupBy(holder => holder.Phone)
                .Select(x => !tels.Contains(x.Key) ? x.First() : null)
                .Where(x => x != null));

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}