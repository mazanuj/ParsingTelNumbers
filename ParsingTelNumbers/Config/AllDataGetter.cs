using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParsingTelNumbers.Sites;

namespace ParsingTelNumbers.Config
{
    internal static class AllDataGetter
    {
        public static async Task<IEnumerable<InfoHolder>> GetData()
        {
            var tasks = new List<Task<IEnumerable<InfoHolder>>>
            {
                Motosale.GetEquip(),
                Motosale.GetMoto(),
                Motosale.GetSpare(),
                Ria.GetSpare(),
                Ria.GetMoto(),
                Ria.GetAqua()
            };

            var allDataInArray = await Task.WhenAll(tasks);

            var resultData = new List<InfoHolder>();

            foreach (var item in allDataInArray)
            {
                resultData.AddRange(item);
            }

            return resultData;
        }
    }
}