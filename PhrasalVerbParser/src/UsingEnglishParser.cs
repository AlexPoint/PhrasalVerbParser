using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PhrasalVerbParser.src
{
    public class UsingEnglishParser
    {
        public const string RootUrl = "http://www.usingenglish.com/reference/phrasal-verbs/";
        public const string StartUrl = RootUrl + "a-list.html";

        private readonly HtmlWeb _web = new HtmlWeb();


        public List<PhrasalVerb> ParsePhrasalVerbs(int nb)
        {
            var counter = 0;
            var phrasalVerbs = new List<PhrasalVerb>();
            var letterPageUrls = ParseLetterPagesUrls();
            foreach (var url in letterPageUrls)
            {
                var phrasalVerbPagesUrls = ParsePhrasalVerbPageUrls(url);
                foreach (var phrasalVerbPageUrl in phrasalVerbPagesUrls)
                {
                    var pv = ParsePhrasalVerb(phrasalVerbPageUrl);
                    phrasalVerbs.Add(pv);
                    Console.WriteLine();
                    pv.Print();
                    counter++;
                    if (counter > nb)
                    {
                        return phrasalVerbs;
                    }
                }
            }
            return phrasalVerbs;
        }

        public List<PhrasalVerb> ParseAllPhrasalVerbs()
        {
            var phrasalVerbs = new List<PhrasalVerb>();
            var letterPageUrls = ParseLetterPagesUrls();
            foreach (var url in letterPageUrls)
            {
                var phrasalVerbPagesUrls = ParsePhrasalVerbPageUrls(url);
                foreach (var phrasalVerbPageUrl in phrasalVerbPagesUrls)
                {
                    var pv = ParsePhrasalVerb(phrasalVerbPageUrl);
                    phrasalVerbs.Add(pv);
                    Console.WriteLine();
                    pv.Print();
                }
            }
            return phrasalVerbs;
        }

        private PhrasalVerb ParsePhrasalVerb(string url)
        {
            var doc = _web.Load(url);

            var title = doc.DocumentNode
                .Descendants("h2")
                .Select(n => n.InnerText)
                .First();

            var usages = doc.DocumentNode
                .Descendants("div")
                .Where(d => d.HasAttributes && d.Attributes.Contains("class") && d.Attributes["class"].Value == "rounded-box")
                .Select(n => new Usage(
                    n.Descendants("strong").First(node => node.InnerText == "Meaning:").ParentNode.InnerText.Trim(),
                    n.Descendants("strong").First(node => node.InnerText == "Example:").ParentNode.InnerText.Trim(),
                    n.Descendants("div").First(d => d.HasAttributes && d.Attributes.Contains("class") && d.Attributes["class"].Value == "indent")
                            .ChildNodes.Select(note => note.InnerText.Trim(new char[] {' ', '-'}).Trim()).ToList())
                ).ToList();

            var pv = new PhrasalVerb()
            {
                Name = title.Split(':').Last().Trim().ToLower().Replace("  ", " "),
                Usages = usages
            };
            return pv;
        }

        private List<string> ParsePhrasalVerbPageUrls(string url)
        {
            var doc = _web.Load(url);
            var urls = doc.DocumentNode
                .Descendants("blockquote")
                .Select(
                    e =>
                        e.ChildNodes.Where(n => n.Name == "a" && n.HasAttributes && n.Attributes.Contains("href"))
                            .Select(n => n.Attributes["href"].Value)
                            .FirstOrDefault())
                .Where(l => !string.IsNullOrEmpty(l))
                .Select(l => RootUrl + l)
                .ToList();
            return urls;
        }

        private List<string> ParseLetterPagesUrls()
        {
            var doc = _web.Load(StartUrl);
            var urls = doc.DocumentNode
                .Descendants("div").First(n => n.HasAttributes && n.Attributes.Contains("class") && n.Attributes["class"].Value == "az")
                .ChildNodes
                .Where(n => n.Name == "a" && n.HasAttributes && n.Attributes.Contains("href"))
                .Select(n => RootUrl + n.Attributes.First(a => a.Name == "href").Value)
                .ToList();
            return urls;
        }
    }
}
