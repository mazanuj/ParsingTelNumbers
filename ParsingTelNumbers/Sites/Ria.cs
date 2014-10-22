using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParsingTelNumbers.Config;
using ParsingTelNumbers.XmlWorker;

namespace ParsingTelNumbers.Sites
{
    internal static class Ria
    {
        public static async Task<IEnumerable<InfoHolder>> GetSpare()
        {
            return await Task.Run(
                () =>
                {
                    var holdersList = new List<InfoHolder>();

                    var dateTrue = DateXmlWorker.GetDate(SiteEnum.ria, DirectionEnum.spare);
                    var trueDate = dateTrue == string.Empty ? new DateTime() : DateTime.Parse(dateTrue);

                    var wc = new WebClient();
                    wc.Headers.Add(HttpRequestHeader.Cookie, "items_per_page=100");
                    var htmlByte =
                        wc.DownloadData(
                            "http://zapchasti.ria.com/advertisement/search/page/1?rubrics[0][category_id]=9&rubrics[0][subcategory_id]=81&category_id=9&subcategory_id=81&options[0][181]=2");
                    var html = Encoding.UTF8.GetString(htmlByte);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var pageLast = int.Parse(doc.DocumentNode
                        .Descendants("div")
                        .First(x => x.Attributes.Contains("class") &&
                                    x.Attributes["class"].Value == "page")
                        .ChildNodes
                        .Last(x => x.Name == "a" &&
                                   x.Attributes.Contains("class") &&
                                   x.Attributes["class"].Value == "item").InnerText);

                    var urls = new List<string>();

                    for (var i = pageLast; i > 0; i--)
                    {
                        htmlByte =
                            wc.DownloadData(
                                string.Format(
                                    "http://zapchasti.ria.com/advertisement/search/page/{0}?rubrics[0][category_id]=9&rubrics[0][subcategory_id]=81&category_id=9&subcategory_id=81&options[0][181]=2&rss=1",
                                    i));
                        html = Encoding.UTF8.GetString(htmlByte);
                        var xml = XDocument.Parse(html);

                        var items = xml.XPathSelectElements(@"//item");
                        urls.AddRange(items.Select(x =>
                        {
                            try
                            {
                                var pubDate = (IEnumerable) x.XPathEvaluate(@"pubDate");
                                var firstOrDefaultDate = pubDate.Cast<XElement>().FirstOrDefault();
                                var date = firstOrDefaultDate != null
                                    ? firstOrDefaultDate.Value
                                    : DateTime.Today.ToString("d");
                                var adsDate = DateTime.Parse(date);

                                if (trueDate.Year > adsDate.Year || trueDate.Month > adsDate.Month ||
                                    trueDate.Day > adsDate.Day)
                                    return string.Empty;

                                var link = (IEnumerable) x.XPathEvaluate(@"link");
                                var firstOrDefaultLink = link.Cast<XElement>().FirstOrDefault();
                                return firstOrDefaultLink != null ? firstOrDefaultLink.Value : string.Empty;
                            }
                            catch (Exception)
                            {
                                return null;
                            }

                        }).Where(x => !string.IsNullOrEmpty(x)));
                    }

                    urls = urls.Distinct().ToList();

                    holdersList.AddRange(urls.Select(url =>
                    {
                        try
                        {
                            doc = new HtmlWeb().Load(url);
                            var phoneElement = doc.GetElementbyId("user_phones");
                            var hash = phoneElement.Attributes["data-info"].Value;

                            if (string.IsNullOrEmpty(hash))
                                throw new Exception();

                            var byteBase64 = Convert.FromBase64String(hash);
                            var stringBase64 = Encoding.UTF8.GetString(byteBase64);
                            var phoneArray = Regex.Matches(stringBase64, @"\d+\s\(\d+\)\s\d+.?\d+.?\d+");

                            if (phoneArray.Count == 0)
                                throw new Exception();

                            var city = doc.DocumentNode
                                .Descendants("div")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "item-param")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .InnerText;

                            var name = doc.DocumentNode
                                .Descendants("div")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "name-seller")
                                .InnerText;

                            holdersList.AddRange(from object VARIABLE in phoneArray
                                select new InfoHolder
                                {
                                    Name = name,
                                    City = city,
                                    Direction = DirectionEnum.spare,
                                    Phone = Regex.Replace(VARIABLE.ToString(), @"(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                });
                        }
                        catch (Exception)
                        {
                            return new InfoHolder();
                        }
                        return new InfoHolder();
                    }).Where(x => x.Phone != null));

                    DateXmlWorker.SetDate(SiteEnum.ria, DirectionEnum.spare, DateTime.Now.ToString("dd.MM.yyyy"));

                    return holdersList;
                });
        }

        public static async Task<IEnumerable<InfoHolder>> GetMoto()
        {
            return await Task.Run(() =>
            {
                var holdersList = new List<InfoHolder>();

                var dateTrue = DateXmlWorker.GetDate(SiteEnum.ria, DirectionEnum.moto);
                var trueDate = dateTrue == string.Empty ? new DateTime() : DateTime.Parse(dateTrue);

                for (var i = 0;; i++)
                {
                    List<string> ids;
                    try
                    {
                        var json =
                            (JObject) JsonConvert.DeserializeObject(
                                new WebClient().DownloadString(
                                    string.Format(
                                        "http://auto.ria.com/blocks_search_ajax/search/?countpage=100&category_id=2&view_type_id=0&page={0}&marka=0&model=0&s_yers=0&po_yers=0&state=0&city=0&price_ot=&price_do=&currency=1&gearbox=0&type=0&drive_type=0&door=0&color=0&metallic=0&engineVolumeFrom=&engineVolumeTo=&raceFrom=&raceTo=&powerFrom=&powerTo=&power_name=1&fuelRateFrom=&fuelRateTo=&fuelRatesType=city&custom=0&damage=0&saledParam=0&under_credit=0&confiscated_car=0&auto_repairs=0&with_exchange=0&with_real_exchange=0&exchangeTypeId=0&with_photo=0&with_video=0&is_hot=0&vip=0&checked_auto_ria=0&top=0&order_by=0&hide_black_list=0&auto_id=&auth=0&deletedAutoSearch=0&user_id=0&scroll_to_auto_id=0&expand_search=0&can_be_checked=0&last_auto_id=0&matched_country=-1&seatsFrom=&seatsTo=&wheelFormulaId=0&axleId=0&carryingTo=&carryingFrom=&search_near_states=0&company_id=0&company_type=0",
                                        i)));

                        ids = json["result"].SelectToken("search_result").SelectToken("ids")
                            .Select(x => x.Value<string>())
                            .ToList();

                        if (ids.Count == 0) break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    //If needs to parse all site data
                    //Use parallel
                    if (dateTrue == "")
                    {
                        Parallel.ForEach(ids, id =>
                        {
                            HtmlDocument doc;
                            string city;
                            try
                            {
                                doc = new HtmlWeb().Load("http://auto.ria.com/blocks_search/view/auto/" + id);

                                city = doc.DocumentNode
                                    .Descendants("span")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "city")
                                    .ChildNodes
                                    .First(x => x.Name == "a")
                                    .InnerText;
                            }
                            catch
                            {
                                return;
                            }
                            var viewAll = "http://auto.ria.com" + doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "view-all")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .Attributes["href"].Value;

                            try
                            {
                                var phone = doc.DocumentNode
                                    .Descendants("span")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .InnerText;

                                holdersList.Add(new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.moto,
                                    Name = string.Empty,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                });
                            }
                            catch (Exception)
                            {
                                doc = new HtmlWeb().Load(viewAll);

                                var phoneBlock = doc.GetElementbyId("final_page__user_phone_block");
                                if (phoneBlock == null)
                                    return;

                                var phones = phoneBlock.ChildNodes
                                    .Descendants("strong")
                                    .Where(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .Select(x => x.InnerText);

                                var name = string.Empty;

                                if (
                                    doc.DocumentNode.Descendants("dt")
                                        .Any(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name"))
                                    name = doc.DocumentNode.Descendants("dt")
                                        .First(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name")
                                        .InnerText;

                                holdersList.AddRange(phones.Select(phone => new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.moto,
                                    Name = Regex.Match(name, @"\w+(\s\w+){0,2}").Value,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                }));
                            }
                        });
                    }
                        //======================================================================================================//
                    else
                    {
                        foreach (var id in ids)
                        {
                            HtmlDocument doc;
                            try
                            {
                                doc = new HtmlWeb().Load("http://auto.ria.com/blocks_search/view/auto/" + id);

                                var adsDate = DateTime.Parse(
                                    doc.DocumentNode
                                        .Descendants("span")
                                        .First(x => x.Attributes.Contains("class") &&
                                                    x.Attributes["class"].Value == "date-add")
                                        .Attributes["pvalue"].Value);

                                if (trueDate.Year > adsDate.Year || trueDate.Month > adsDate.Month ||
                                    trueDate.Day > adsDate.Day)
                                {
                                    var itemChips = doc.DocumentNode
                                        .Descendants("a")
                                        .First(x => x.Attributes.Contains("class") &&
                                                    x.Attributes["class"].Value == "item icon-chips")
                                        .InnerText;

                                    if (Regex.Match(itemChips, @"\d").Value == "0")
                                        return holdersList;
                                    continue;
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            var city = doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "city")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .InnerText;

                            var viewAll = "http://auto.ria.com" + doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "view-all")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .Attributes["href"].Value;

                            try
                            {
                                var phone = doc.DocumentNode
                                    .Descendants("span")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .InnerText;

                                holdersList.Add(new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.moto,
                                    Name = string.Empty,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                });
                            }
                            catch (Exception)
                            {
                                doc = new HtmlWeb().Load(viewAll);

                                var phoneBlock = doc.GetElementbyId("final_page__user_phone_block");
                                if (phoneBlock == null)
                                    continue;

                                var phones = phoneBlock.ChildNodes
                                    .Descendants("strong")
                                    .Where(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .Select(x => x.InnerText);

                                var name = string.Empty;

                                if (
                                    doc.DocumentNode.Descendants("dt")
                                        .Any(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name"))
                                    name = doc.DocumentNode.Descendants("dt")
                                        .First(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name")
                                        .InnerText;

                                holdersList.AddRange(phones.Select(phone => new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.moto,
                                    Name = Regex.Match(name, @"\w+(\s\w+){0,2}").Value,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                }));
                            }
                        }
                    }
                }

                DateXmlWorker.SetDate(SiteEnum.ria, DirectionEnum.moto, DateTime.Now.ToString("dd.MM.yyyy"));

                return holdersList;
            });
        }

        public static async Task<IEnumerable<InfoHolder>> GetAqua()
        {
            return await Task.Run(() =>
            {
                var holdersList = new List<InfoHolder>();

                var dateTrue = DateXmlWorker.GetDate(SiteEnum.ria, DirectionEnum.aqua);
                var trueDate = dateTrue == string.Empty ? new DateTime() : DateTime.Parse(dateTrue);

                for (var i = 0;; i++)
                {
                    List<string> ids;
                    try
                    {
                        var json =
                            (JObject) JsonConvert.DeserializeObject(
                                new WebClient().DownloadString(
                                    string.Format(
                                        "http://auto.ria.com/blocks_search_ajax/search/?countpage=100&category_id=3&view_type_id=0&page={0}&marka=0&model=0&s_yers=0&po_yers=0&state=0&city=0&price_ot=&price_do=&currency=1&gearbox=0&type=0&drive_type=0&door=0&color=0&metallic=0&engineVolumeFrom=&engineVolumeTo=&raceFrom=&raceTo=&powerFrom=&powerTo=&power_name=1&fuelRateFrom=&fuelRateTo=&fuelRatesType=city&custom=0&damage=0&saledParam=0&under_credit=0&confiscated_car=0&auto_repairs=0&with_exchange=0&with_real_exchange=0&exchangeTypeId=0&with_photo=0&with_video=0&is_hot=0&vip=0&checked_auto_ria=0&top=0&order_by=0&hide_black_list=0&auto_id=&auth=0&deletedAutoSearch=0&user_id=0&scroll_to_auto_id=0&expand_search=0&can_be_checked=0&last_auto_id=0&matched_country=-1&seatsFrom=&seatsTo=&wheelFormulaId=0&axleId=0&carryingTo=&carryingFrom=&search_near_states=0&company_id=0&company_type=0",
                                        i)));

                        ids = json["result"].SelectToken("search_result").SelectToken("ids")
                            .Select(x => x.Value<string>())
                            .ToList();

                        if (ids.Count == 0) break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    //If needs to parse all site data
                    //Use parallel
                    if (dateTrue == "")
                        Parallel.ForEach(ids, id =>
                        {
                            HtmlDocument doc;
                            try
                            {
                                doc = new HtmlWeb().Load("http://auto.ria.com/blocks_search/view/auto/" + id);
                            }
                            catch
                            {
                                return;
                            }

                            var city = doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "city")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .InnerText;

                            var viewAll = "http://auto.ria.com" + doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "view-all")
                                .ChildNodes
                                .First(x => x.Name == "a")
                                .Attributes["href"].Value;

                            try
                            {
                                var phone = doc.DocumentNode
                                    .Descendants("span")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .InnerText;

                                holdersList.Add(new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.aqua,
                                    Name = string.Empty,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                });
                            }
                            catch (Exception)
                            {
                                doc = new HtmlWeb().Load(viewAll);

                                var phoneBlock = doc.GetElementbyId("final_page__user_phone_block");
                                if (phoneBlock == null)
                                    return;

                                var phones = phoneBlock.ChildNodes
                                    .Descendants("strong")
                                    .Where(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "phone")
                                    .Select(x => x.InnerText);

                                var name = string.Empty;

                                if (
                                    doc.DocumentNode.Descendants("dt")
                                        .Any(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name"))
                                    name = doc.DocumentNode.Descendants("dt")
                                        .First(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "user-name")
                                        .InnerText;

                                holdersList.AddRange(phones.Select(phone => new InfoHolder
                                {
                                    City = city,
                                    Direction = DirectionEnum.aqua,
                                    Name = Regex.Match(name, @"\w+(\s\w+){0,2}").Value,
                                    Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                    Site = SiteEnum.ria
                                }));
                            }
                        });
                    //======================================================================================================//
                    foreach (var id in ids)
                    {
                        HtmlDocument doc;
                        try
                        {
                            doc = new HtmlWeb().Load("http://auto.ria.com/blocks_search/view/auto/" + id);

                            var adsDate = DateTime.Parse(
                                doc.DocumentNode
                                    .Descendants("span")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "date-add")
                                    .Attributes["pvalue"].Value);

                            if (trueDate.Year > adsDate.Year || trueDate.Month > adsDate.Month ||
                                trueDate.Day > adsDate.Day)
                            {
                                var itemChips = doc.DocumentNode
                                    .Descendants("a")
                                    .First(x => x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "item icon-chips")
                                    .InnerText;

                                if (Regex.Match(itemChips, @"\d").Value == "0")
                                    return holdersList;
                                continue;
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        var city = doc.DocumentNode
                            .Descendants("span")
                            .First(x => x.Attributes.Contains("class") &&
                                        x.Attributes["class"].Value == "city")
                            .ChildNodes
                            .First(x => x.Name == "a")
                            .InnerText;

                        var viewAll = "http://auto.ria.com" + doc.DocumentNode
                            .Descendants("span")
                            .First(x => x.Attributes.Contains("class") &&
                                        x.Attributes["class"].Value == "view-all")
                            .ChildNodes
                            .First(x => x.Name == "a")
                            .Attributes["href"].Value;

                        try
                        {
                            var phone = doc.DocumentNode
                                .Descendants("span")
                                .First(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "phone")
                                .InnerText;

                            holdersList.Add(new InfoHolder
                            {
                                City = city,
                                Direction = DirectionEnum.aqua,
                                Name = string.Empty,
                                Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                Site = SiteEnum.ria
                            });
                        }
                        catch (Exception)
                        {
                            doc = new HtmlWeb().Load(viewAll);

                            var phoneBlock = doc.GetElementbyId("final_page__user_phone_block");
                            if (phoneBlock == null)
                                continue;

                            var phones = phoneBlock.ChildNodes
                                .Descendants("strong")
                                .Where(x => x.Attributes.Contains("class") &&
                                            x.Attributes["class"].Value == "phone")
                                .Select(x => x.InnerText);

                            var name = string.Empty;

                            if (
                                doc.DocumentNode.Descendants("dt")
                                    .Any(
                                        x =>
                                            x.Attributes.Contains("class") && x.Attributes["class"].Value == "user-name"))
                                name = doc.DocumentNode.Descendants("dt")
                                    .First(
                                        x =>
                                            x.Attributes.Contains("class") && x.Attributes["class"].Value == "user-name")
                                    .InnerText;

                            holdersList.AddRange(phones.Select(phone => new InfoHolder
                            {
                                City = city,
                                Direction = DirectionEnum.aqua,
                                Name = Regex.Match(name, @"\w+(\s\w+){0,2}").Value,
                                Phone = "38" + Regex.Replace(phone, @"(^\+?38)?(\(|\)|\s|\-)", string.Empty),
                                Site = SiteEnum.ria
                            }));
                        }
                    }
                }

                DateXmlWorker.SetDate(SiteEnum.ria, DirectionEnum.aqua, DateTime.Now.ToString("dd.MM.yyyy"));

                return holdersList;
            });
        }
    }
}