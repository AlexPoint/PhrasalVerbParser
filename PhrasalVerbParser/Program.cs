using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PhrasalVerbParser.src;

namespace PhrasalVerbParser
{
    class Program
    {
        private static string PathToApplication = Directory.GetCurrentDirectory() + "/../../";
        
        static void Main(string[] args)
        {
            var phrasalVerbFilePath = PathToApplication + "Resources/phrasalVerbs";

            // parsing of usingenglish phrasal verbs
            var parser = new UsingEnglishParser();
            var phrasalVerbs = parser.ParseAllPhrasalVerbs();
            Console.WriteLine("============");
            Console.WriteLine("Found {0} phrasal verbs", phrasalVerbs.Count);

            // persisting list of phrasal verbs
            Console.WriteLine("============");
            Console.WriteLine("Persisting phrasal verbs to {0}", phrasalVerbFilePath);
            PersistPhrasalVerbs(phrasalVerbs, phrasalVerbFilePath);
            Console.WriteLine("Persisted phrasal verbs");

            Console.WriteLine("============");
            Console.WriteLine("Reading phrasal verbs from file");
            var verbs = ReadPhrasalVerbs(phrasalVerbFilePath);
            Console.WriteLine("Retrieved {0} verbs", verbs.Count);

            var fleexPhrasalVerbs = ReadFleexPhrasalVerbs();

            var fleexOnlyPv = fleexPhrasalVerbs.Except(verbs.Select(v => v.Name)).ToList();
            Console.WriteLine("{0} phrasal verbs missing on using english:", fleexOnlyPv.Count);
            foreach (var pv in fleexOnlyPv)
            {
                Console.WriteLine(pv);
            }

            Console.WriteLine("-------------------------");
            var missingPvOnFleex = verbs.Select(v => v.Name).Except(fleexPhrasalVerbs);
            Console.WriteLine("{0} phrasal verbs missing on fleex", missingPvOnFleex.Count());
            foreach (var pv in missingPvOnFleex)
            {
                Console.WriteLine(pv);
            }

            Console.ReadKey();
        }

        private static List<string> ReadFleexPhrasalVerbs()
        {
            var pathToFile = PathToApplication + "Resources/fleexPhrasalVerbs.txt";
            var allLines = File.ReadAllLines(pathToFile).ToList();
            return allLines;
        }

        
        private static void PersistPhrasalVerbs(List<PhrasalVerb> verbs, string filePath)
        {
            using (var fs = File.OpenWrite(filePath))
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(fs, verbs);
            }
        }

        private static List<PhrasalVerb> ReadPhrasalVerbs(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                var serializer = new BinaryFormatter();
                var phrasalVerbs = (List<PhrasalVerb>) serializer.Deserialize(fs);
                return phrasalVerbs;
            }
        }
    }
}
