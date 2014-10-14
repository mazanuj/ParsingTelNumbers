using System;
using ParsingTelNumbers.Sites;

namespace ParsingTelNumbers
{
    internal static class Program
    {
        private static void Main()
        {
            //var t = Motosale.GetSpare();
            //var t = Motosale.GetEquip();
            var t = Motosale.GetMoto();

            Console.ReadKey();
        }
    }
}