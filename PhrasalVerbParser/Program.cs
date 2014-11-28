using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhrasalVerbParser.src;

namespace PhrasalVerbParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new UsingEnglishParser();
            var phrasalVerbs = parser.ParseAllPhrasalVerbs();

            Console.WriteLine("============");
            Console.WriteLine("Found {0} phrasal verbs", phrasalVerbs.Count);
            Console.ReadKey();
        }
    }
}
