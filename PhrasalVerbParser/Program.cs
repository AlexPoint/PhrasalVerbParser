using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using LemmaSharp.Classes;
using PhrasalVerbParser.src;
using TestStanfordParser.src;

namespace PhrasalVerbParser
{
    class Program
    {
        private static string PathToApplication = Directory.GetCurrentDirectory() + "/../../";
        
        static void Main(string[] args)
        {
            var phrasalVerbFilePath = PathToApplication + "Resources/phrasalVerbs";

            var fullPathToSrcData = PathToApplication + "Resources/lemmatizer/en_lemmatizer_data.lem";
            var stream = File.OpenRead(fullPathToSrcData);
            var lemmatizer = new Lemmatizer(stream);

            /*var parser = new UsingEnglishParser();
            var phrasalVerbs = parser.ParseAllPhrasalVerbs();
            Console.WriteLine("============");
            Console.WriteLine("Found {0} phrasal verbs", phrasalVerbs.Count);*/


            var phrasalVerbs = ReadPhrasalVerbs(phrasalVerbFilePath);

            // parse dependencies
            var tuples = new List<Tuple<string, string, List<DependencyRelationship>>>();
            var lines = new List<string>();
            foreach (var phrasalVerb in phrasalVerbs)
            {
                foreach (var usage in phrasalVerb.Usages)
                {
                    var cleanupExample = LowerCaseAllUpperCasedWords(usage.Example);
                    var dependencies = DependencyParser.ParseDepencyRelationshipsInSentence(cleanupExample);
                    
                    Console.WriteLine(cleanupExample);
                    //var relationships = dependencies.Where(d => d.RelationshipType == "prt");
                    var parts = phrasalVerb.Name.Split(' ');
                    if (parts.Count() > 2)
                    {
                        Console.WriteLine("{0} parts for phrasal verb {1}", parts.Count(), string.Join("|", parts));
                    }
                    var root = parts.First();
                    var last = parts.Last();
                    var relevantRelationships =
                        dependencies.Where(d => (root == lemmatizer.Lemmatize(d.Dep.Word) && last == d.Gov.Word)
                        || (root == lemmatizer.Lemmatize(d.Gov.Word) && last == d.Dep.Word));
                    var secondRelevantRelationships =
                        dependencies.Where(d => last == d.Gov.Word || last == d.Dep.Word);
                    Console.WriteLine(string.Join("|", relevantRelationships));
                    var line = string.Format("{0}|{1}/{2}|{3}|{4}", cleanupExample, root, last,
                        string.Join("||", relevantRelationships), string.Join("||", secondRelevantRelationships));
                    lines.Add(line);

                    tuples.Add(new Tuple<string, string, List<DependencyRelationship>>(phrasalVerb.Name, cleanupExample, relevantRelationships.ToList()));
                    Console.WriteLine("---------");
                }
            }

            var outputFilePath = PathToApplication + "Resources/output.txt";
            File.WriteAllLines(outputFilePath, lines);

            var nbOfExamples = tuples.Count;
            Console.WriteLine("{0} sentence examples", nbOfExamples);
            var nbOfPrtDepDetected = tuples.Count(tup => tup.Item3.Any());
            Console.WriteLine("{0} sentences with prt detected", nbOfPrtDepDetected);
            var nbOfPrtNotDetected = tuples.Count(tup => !tup.Item3.Any());
            Console.WriteLine("{0} sentences with prt not detected", nbOfPrtNotDetected);

            //Console.WriteLine("===================");
            
            /*// persisting list of phrasal verbs
            Console.WriteLine("============");
            Console.WriteLine("Persisting phrasal verbs to {0}", phrasalVerbFilePath);
            PersistPhrasalVerbs(phrasalVerbs, phrasalVerbFilePath);
            Console.WriteLine("Persisted phrasal verbs");*/

            // persisting examples
            var pvExamplesFilePath = PathToApplication + "Resources/phrasalVerbsExamples.txt";
            PersistPhrasalVerbsAndExamples(phrasalVerbs, pvExamplesFilePath);
            Console.WriteLine("Persisted examples");
            Console.WriteLine("-------------------------");
            
            // stats on usingenglish phrasal verbs
            var verbs = ReadPhrasalVerbs(phrasalVerbFilePath);
            var nbOfSeparableVerbs = verbs.Count(v => v.Usages.All(u => u.SeparableMandatory));
            var nbOfInseparableVerbs = verbs.Count(v => v.Usages.All(u => u.Inseparable));
            Console.WriteLine("{0} separable verbs", nbOfSeparableVerbs);
            Console.WriteLine("{0} inseparable verbs", nbOfInseparableVerbs);
            Console.WriteLine("{0} verbs", verbs.Count);

            // write a file with examples of phrasal verbs
            /*var pathToOutputFile = PathToApplication + "Resources/phrasalVerbExamples.txt";
            var verbs = ReadPhrasalVerbs(phrasalVerbFilePath);
            var lines =
                verbs.SelectMany(v => v.Usages.Select(u => new {Usage = u, v.Name}))
                    .Select(a => string.Format("{0}|{1}", a.Name, a.Usage.Example));
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            File.WriteAllLines(pathToOutputFile, lines);*/

            Console.WriteLine("=== END ===");
            Console.ReadKey();
        }

        private static string LowerCaseAllUpperCasedWords(string sentence)
        {
            var replacedSentence = Regex.Replace(sentence, "[A-Z]{2,}", s => s.Value.ToLower());
            return replacedSentence;
        }

        private static void PersistPhrasalVerbsAndExamples(List<PhrasalVerb> verbs, string filePath)
        {
            var lines = verbs.SelectMany(v => v.Usages)
                .Select(
                    u =>
                        string.Format("{0}|{1}",
                            verbs.First(pv => pv.Usages.Select(uv => uv.Example).Contains(u.Example)).Name, u.Example))
                .ToList();
            File.WriteAllLines(filePath, lines);
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
