using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ParsingTelNumbers.Config;
using ParsingTelNumbers.XmlWorker;

namespace ParsingTelNumbers.Sites
{
    internal static class Motosale
    {
        public static async Task<IEnumerable<InfoHolder>> GetSpare()
        {
            return await Task.Run(
                () =>
                {
                    var holdersList = new List<InfoHolder>();
                    var date = DateXmlWorker.GetDate(SiteEnum.motosale, DirectionEnum.spare);

                    var htmlByte =
                        new WebClient().DownloadData("http://www.motosale.com.ua/index.php?showall=zap&offset=show_all");
                    var html = Encoding.GetEncoding("windows-1251").GetString(htmlByte);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var urls =
                        doc.DocumentNode.Descendants("div")
                            .First(
                                x =>
                                    x.Attributes.Contains("style") &&
                                    x.Attributes["style"].Value == "margin: 10px 5px 10px 5px; width:100%")
                            .Descendants("tr")
                            .Where(x => x.ChildNodes
                                .Count(y => y.Name == "td" &&
                                            y.InnerText.Contains("UIN:")
                                            && y.InnerText.Contains("Дата публикации:")) != 0)
                            .Select(x =>
                            {
                                if (date != string.Empty)
                                {
                                    var adsDate =
                                        Regex.Match(x.ChildNodes.First(y => y.Name == "td").InnerText,
                                            @"\d{2}\.\d{2}\.\d{4}")
                                            .Value.Split('.')
                                            .Select(int.Parse).ToArray();

                                    var trueDate = date.Split('.').Select(int.Parse).ToArray();

                                    if (trueDate[2] > adsDate[2] || trueDate[1] > adsDate[1] || trueDate[0] > adsDate[0])
                                        return null;
                                }

                                return "http://www.motosale.com.ua" +
                                       x.ChildNodes.Last(y => y.Name == "td")
                                           .Descendants("a")
                                           .First(
                                               y =>
                                                   y.Attributes.Contains("href") &&
                                                   Regex.IsMatch(y.Attributes["href"].Value, @".+\.htm.?")).Attributes[
                                                       "href"]
                                           .Value;
                            }).Where(x => !string.IsNullOrEmpty(x));

                    var enumerable = urls as IList<string> ?? urls.ToList();
                    holdersList.AddRange(enumerable.Select(url =>
                    {
                        string h;
                        try
                        {
                            var hb = new WebClient().DownloadData(url.Replace("&#", ""));
                            h = Encoding.GetEncoding("windows-1251").GetString(hb);
                        }
                        catch
                        {
                            return new InfoHolder();
                        }


                        var docHtml = new HtmlDocument();
                        docHtml.LoadHtml(h);

                        var phone = docHtml.DocumentNode

                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .InnerText;
                        phone = Regex.Match(phone, @"(?<=тел.: )\.?\d{10}").Value;
                        if (!Regex.IsMatch(phone, @"\d*"))
                            return new InfoHolder();

                        var name = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open")).InnerText;

                        var city = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .ChildNodes
                            .First(y => y.InnerText.Contains("Регион:"))
                            .ChildNodes
                            .Last(x => x.Name == "td")
                            .InnerText;

                        return new InfoHolder
                        {
                            Site = SiteEnum.motosale,
                            Direction = DirectionEnum.spare,
                            Name = name.Contains("сообщ. писать здесь") ? "-" : name,
                            Phone = phone.StartsWith("0") ? "38" + phone : phone,
                            City = city
                        };
                    }).Where(infoHolder => infoHolder != null));

                    DateXmlWorker.SetDate(SiteEnum.motosale, DirectionEnum.spare, DateTime.Now.ToString("dd.MM.yyyy"));

                    return holdersList
                        .Where(x => !string.IsNullOrEmpty(x.Phone))
                        .GroupBy(holder => holder.Phone)
                        .Select(x => x.First())
                        .ToList();
                });
        }

        public static async Task<IEnumerable<InfoHolder>> GetEquip()
        {
            return await Task.Run(
                () =>
                {
                    var holdersList = new List<InfoHolder>();
                    var date = DateXmlWorker.GetDate(SiteEnum.motosale, DirectionEnum.equip);

                    var htmlByte =
                        new WebClient().DownloadData("http://www.motosale.com.ua/index.php?showall=equ&offset=show_all");
                    var html = Encoding.GetEncoding("windows-1251").GetString(htmlByte);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var urls =
                        doc.DocumentNode.Descendants("div")
                            .First(
                                x =>
                                    x.Attributes.Contains("style") &&
                                    x.Attributes["style"].Value == "margin: 10px 5px 10px 5px; width:100%")
                            .Descendants("tr")
                            .Where(x => x.ChildNodes
                                .Count(y => y.Name == "td" &&
                                            y.InnerText.Contains("UIN:")
                                            && y.InnerText.Contains("Дата публикации:")) != 0)
                            .Select(x =>
                            {
                                if (date != string.Empty)
                                {
                                    var adsDate =
                                        Regex.Match(x.ChildNodes.First(y => y.Name == "td").InnerText,
                                            @"\d{2}\.\d{2}\.\d{4}")
                                            .Value.Split('.')
                                            .Select(int.Parse).ToArray();

                                    var trueDate = date.Split('.').Select(int.Parse).ToArray();

                                    if (trueDate[2] > adsDate[2] || trueDate[1] > adsDate[1] || trueDate[0] > adsDate[0])
                                        return null;
                                }

                                return "http://www.motosale.com.ua" +
                                       x.ChildNodes.Last(y => y.Name == "td")
                                           .Descendants("a")
                                           .First(
                                               y =>
                                                   y.Attributes.Contains("href") &&
                                                   Regex.IsMatch(y.Attributes["href"].Value, @".+\.htm.?")).Attributes[
                                                       "href"]
                                           .Value;
                            }).Where(x => !string.IsNullOrEmpty(x));

                    var enumerable = urls as IList<string> ?? urls.ToList();
                    holdersList.AddRange(enumerable.Select(url =>
                    {
                        string h;
                        try
                        {
                            var hb = new WebClient().DownloadData(url.Replace("&#", ""));
                            h = Encoding.GetEncoding("windows-1251").GetString(hb);
                        }
                        catch
                        {
                            return new InfoHolder();
                        }


                        var docHtml = new HtmlDocument();
                        docHtml.LoadHtml(h);

                        var phone = docHtml.DocumentNode

                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .InnerText;
                        phone = Regex.Match(phone, @"(?<=тел.: )\.?\d{10}").Value;
                        if (!Regex.IsMatch(phone, @"\d*"))
                            return new InfoHolder();

                        var name = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open")).InnerText;

                        var city = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .ChildNodes
                            .First(y => y.InnerText.Contains("Регион:"))
                            .ChildNodes
                            .Last(x => x.Name == "td")
                            .InnerText;

                        return new InfoHolder
                        {
                            Site = SiteEnum.motosale,
                            Direction = DirectionEnum.equip,
                            Name = name.Contains("сообщ. писать здесь") ? "-" : name,
                            Phone = phone.StartsWith("0") ? "38" + phone : phone,
                            City = city
                        };
                    }).Where(infoHolder => infoHolder != null));

                    DateXmlWorker.SetDate(SiteEnum.motosale, DirectionEnum.equip, DateTime.Now.ToString("dd.MM.yyyy"));

                    return holdersList
                        .Where(x => !string.IsNullOrEmpty(x.Phone))
                        .GroupBy(holder => holder.Phone)
                        .Select(x => x.First())
                        .ToList();
                });
        }

        public static async Task<IEnumerable<InfoHolder>> GetMoto()
        {
            return await Task.Run(
                () =>
                {
                    var holdersList = new List<InfoHolder>();
                    var date = DateXmlWorker.GetDate(SiteEnum.motosale, DirectionEnum.moto);

                    var htmlByte =
                        new WebClient().DownloadData(
                            "http://www.motosale.com.ua/index.php?search=moto&model=&price[min]=&price[max]=&city=&in[min]=&in[max]=&run=&v=&type_obj=1&offset=show_all");
                    var html = Encoding.GetEncoding("windows-1251").GetString(htmlByte);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var urls =
                        doc.DocumentNode.Descendants("div")
                            .First(
                                x =>
                                    x.Attributes.Contains("style") &&
                                    x.Attributes["style"].Value == "margin: 10px 5px 10px 5px; width:100%")
                            .Descendants("tr")
                            .Where(x => x.ChildNodes
                                .Count(y => y.Name == "td" &&
                                            y.InnerText.Contains("UIN:")
                                            && y.InnerText.Contains("Дата публикации:")) != 0)
                            .Select(x =>
                            {
                                if (date != string.Empty)
                                {
                                    var adsDate =
                                        Regex.Match(x.ChildNodes.First(y => y.Name == "td").InnerText,
                                            @"\d{2}\.\d{2}\.\d{4}")
                                            .Value.Split('.')
                                            .Select(int.Parse).ToArray();

                                    var trueDate = date.Split('.').Select(int.Parse).ToArray();

                                    if (trueDate[2] > adsDate[2] || trueDate[1] > adsDate[1] || trueDate[0] > adsDate[0])
                                        return null;
                                }

                                return "http://www.motosale.com.ua/" +
                                       x.ChildNodes.Last(y => y.Name == "td")
                                           .Descendants("a")
                                           .First(
                                               y =>
                                                   y.Attributes.Contains("href") &&
                                                   Regex.IsMatch(y.Attributes["href"].Value, @".+\.htm.?")).Attributes[
                                                       "href"]
                                           .Value;
                            }).Where(x => !string.IsNullOrEmpty(x));

                    var enumerable = urls as IList<string> ?? urls.ToList();
                    holdersList.AddRange(enumerable.Select(url =>
                    {
                        string h;
                        try
                        {
                            var hb = new WebClient().DownloadData(url.Replace("&#", ""));
                            h = Encoding.GetEncoding("windows-1251").GetString(hb);
                        }
                        catch
                        {
                            return new InfoHolder();
                        }


                        var docHtml = new HtmlDocument();
                        docHtml.LoadHtml(h);

                        var phone = docHtml.DocumentNode

                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .InnerText;
                        phone = Regex.Match(phone, @"(?<=тел.: )\.?\d{10}").Value;
                        if (!Regex.IsMatch(phone, @"\d*"))
                            return new InfoHolder();

                        var name = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open")).InnerText;

                        var city = docHtml.DocumentNode
                            .Descendants("a")
                            .First(
                                x =>
                                    x.Attributes.Contains("href") &&
                                    x.Attributes["href"].Value.Contains("javascript:window.open"))
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .ParentNode
                            .ChildNodes
                            .First(y => y.InnerText.Contains("Регион:"))
                            .ChildNodes
                            .Last(x => x.Name == "td")
                            .InnerText;

                        return new InfoHolder
                        {
                            Site = SiteEnum.motosale,
                            Direction = DirectionEnum.moto,
                            Name = name.Contains("сообщ. писать здесь") ? "-" : name,
                            Phone = phone.StartsWith("0") ? "38" + phone : phone,
                            City = city
                        };
                    }).Where(infoHolder => infoHolder != null));

                    DateXmlWorker.SetDate(SiteEnum.motosale, DirectionEnum.moto, DateTime.Now.ToString("dd.MM.yyyy"));

                    return holdersList
                        .Where(x => !string.IsNullOrEmpty(x.Phone))
                        .GroupBy(holder => holder.Phone)
                        .Select(x => x.First())
                        .ToList();
                });
        }
    }
}