using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using LemmaSharp.Classes;
using OpenNLP.Tools.Parser;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.SentenceDetect;
using OpenNLP.Tools.Tokenize;
using OpenNLP.Tools.Trees;
using PhrasalVerbParser.Src;
using PhrasalVerbParser.Src.Detectors;

namespace PhrasalVerbParser
{
    class Program
    {
        private static readonly string PathToApplication = Directory.GetCurrentDirectory() + "/../../";

        
        static void Main(string[] args)
        {
            // parse phrasal verb on usingEnglish.com
            /*var usingEnglishParser = new UsingEnglishParser();
            var allPhrasalVerbs = usingEnglishParser.ParseAllPhrasalVerbs();
            Console.Write("Parsed {0} phrasal verbs on using english", allPhrasalVerbs);*/

            // Persist phrasal verbs
            /*var phrasalVerbFilePath = PathToApplication + "Resources/phrasalVerbs";
            PersistPhrasalVerbs(allPhrasalVerbs, phrasalVerbFilePath);
            Console.WriteLine("Phrasal verbs persisted");*/

            // Lemmatizer
            var fullPathToSrcData = PathToApplication + "Resources/lemmatizer/en_lemmatizer_data.lem";
            var stream = File.OpenRead(fullPathToSrcData);
            var lemmatizer = new Lemmatizer(stream);
            
            // load phrasal verbs & examples
            var phrasalVerbFilePath = PathToApplication + "Resources/phrasalVerbs";
            var phrasalVerbs = ReadFleexPhrasalVerbs()
                .Where(pv => !pv.IsMissingOnWordreference && pv.Name != "go to")
                .ToList();

            //
            var tokenizerModelPaths = PathToApplication + "Resources/OpenNlp/Models/EnglishTok.nbin";
            var tokenizer = new EnglishMaximumEntropyTokenizer(tokenizerModelPaths);
            var englishPosPath = PathToApplication + "Resources/OpenNlp/Models/EnglishPOS.nbin";
            var tagDictPath = PathToApplication + "Resources/OpenNlp/Models/Parser/tagdict";
            var tagger = new EnglishMaximumEntropyPosTagger(englishPosPath, tagDictPath);
            var basicDetector = new BasicPhrasalVerbDetector(tokenizer, lemmatizer);
            var parseBasedDetector = new ParseBasedPhrasalVerbDetector(GetParser(), lemmatizer, tokenizer, tagger);

            var pathToManuallyValidatedPhrasalVerbs = PathToApplication + "Resources/manual/good.txt";
            var pathToManuallyUnvalidatedPhrasalVerbs = PathToApplication + "Resources/manual/bad.txt";


            var sent = "A very silly, trivial thing to do, but think of the difference on a team that didn't do that at all, that got 15 euro, put it in their pocket, maybe bought themselves a coffee, or teams that had this prosocial experience where they all bonded together to buy something and do a group activity.";
            var pvs = parseBasedDetector.MatchingPhrasalVerbs(sent, phrasalVerbs.ConvertAll(pv => (PhrasalVerb)pv));

            // missing pv detections
            var manuallyValidatedExamples = File.ReadAllLines(pathToManuallyValidatedPhrasalVerbs)
                .Where(line => phrasalVerbs.Select(pv => pv.Name).Contains(line.Split('|').First()))
                .ToList();
            Console.WriteLine("Phrasal verbs not detected:");
            var notDetected = new List<Tuple<string, string>>();
            foreach (var example in manuallyValidatedExamples)
            {
                var sentence = example.Split('|').Last();
                var phrasalVerb = example.Split('|').First();
                var matchingPvs = parseBasedDetector.MatchingPhrasalVerbs(sentence, phrasalVerbs.ConvertAll(pv => (PhrasalVerb)pv));
                if (!matchingPvs.Any(p => p.Name == phrasalVerb))
                {
                    notDetected.Add(new Tuple<string, string>(sentence, phrasalVerb));
                }
            }
            Console.WriteLine("{0}% phrasal verbs not detected", (float)(notDetected.Count * 100)/ manuallyValidatedExamples.Count());
            foreach (var tuple in notDetected)
            {
                Console.WriteLine("{0}; {1}", tuple.Item2, tuple.Item1);
            }
            Console.WriteLine("----------");

            // false positive detection
            var manuallyUnvalidatedExamples = File.ReadAllLines(pathToManuallyUnvalidatedPhrasalVerbs)
                .Where(line => phrasalVerbs.Select(pv => pv.Name).Contains(line.Split('|').First()))
                .ToList();
            Console.WriteLine("Wrongly detected PV ");
            var wronglyDetected = new List<Tuple<string, string>>();
            foreach (var example in manuallyUnvalidatedExamples)
            {
                var sentence = example.Split('|').Last();
                var phrasalVerb = example.Split('|').First();
                var matchingPvs = parseBasedDetector.MatchingPhrasalVerbs(sentence, phrasalVerbs.ConvertAll(pv => (PhrasalVerb)pv));
                if (matchingPvs.Any(p => p.Name == phrasalVerb))
                {
                    wronglyDetected.Add(new Tuple<string, string>(sentence, phrasalVerb));
                }
            }
            Console.WriteLine("{0}% of wrongly detected examples:", (float)(wronglyDetected.Count * 100)/manuallyUnvalidatedExamples.Count());
            foreach (var tuple in wronglyDetected)
            {
                Console.WriteLine("'{0}'; {1}", tuple.Item2, tuple.Item1);
            }
            Console.WriteLine("----------");


            // manual input for loosely detected phrasal verb
            /*var pathToSentenceFile = PathToApplication + "Resources/fleex_sentences.txt";
            var sentences = File.ReadAllLines(pathToSentenceFile);
            foreach (var sentence in sentences)
            {
                // detect all other phrasal verbs
                foreach (var pv in phrasalVerbs)
                {
                    var isMatch = basicDetector.IsMatch(sentence, pv);
                    if (isMatch)
                    {
                        var capitalizedRoot = Regex.Replace(sentence, "\\b" + pv.Root, pv.Root.ToUpper());
                        var capitalizedParticle = Regex.Replace(capitalizedRoot, "\\b" + pv.Particle1 + "\\b", pv.Particle1.ToUpper());
                        Console.WriteLine("{0} --> '{1}'; 'y' for OK, n otherwise", pv.Name, capitalizedParticle);
                        var key = Console.ReadKey();
                        while (key.KeyChar != 'y' && key.KeyChar != 'n')
                        {
                            Console.WriteLine("'y' / 'n' only");
                            key = Console.ReadKey();
                        }
                        Console.WriteLine();
                        string filePathToWrite = key.KeyChar == 'y'
                            ? pathToManuallyValidatedPhrasalVerbs
                            : pathToManuallyUnvalidatedPhrasalVerbs;
                        using (var writer = new StreamWriter(filePathToWrite, true))
                        {
                            writer.WriteLine("{0}|{1}", pv.Name, sentence);
                        }
                    }
                }
            }*/

            
            /*// persisting list of phrasal verbs
            Console.WriteLine("============");
            Console.WriteLine("Persisting phrasal verbs to {0}", phrasalVerbFilePath);
            PersistPhrasalVerbs(phrasalVerbs, phrasalVerbFilePath);
            Console.WriteLine("Persisted phrasal verbs");*/

            // persisting examples
            /*var pvExamplesFilePath = PathToApplication + "Resources/phrasalVerbsExamples.txt";
            PersistPhrasalVerbsAndExamples(phrasalVerbs, pvExamplesFilePath);
            Console.WriteLine("Persisted examples");
            Console.WriteLine("-------------------------");*/
            
            // stats on usingenglish phrasal verbs
            /*var verbs = ReadPhrasalVerbs(phrasalVerbFilePath);
            var nbOfSeparableVerbs = verbs.Count(v => v.Usages.All(u => u.SeparableMandatory));
            var nbOfInseparableVerbs = verbs.Count(v => v.Usages.All(u => u.Inseparable));
            Console.WriteLine("{0} separable verbs", nbOfSeparableVerbs);
            Console.WriteLine("{0} inseparable verbs", nbOfInseparableVerbs);
            Console.WriteLine("{0} verbs", verbs.Count);*/

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

        private static EnglishTreebankParser _parser;
        private static EnglishTreebankParser GetParser()
        {
            if (_parser == null)
            {
                var modelPath = PathToApplication + "Resources/OpenNlp/Models/";
                _parser = new EnglishTreebankParser(modelPath, true, false);
            }
            return _parser;
        }

        private static string LowerCaseAllUpperCasedWords(string sentence)
        {
            var replacedSentence = Regex.Replace(sentence, "[A-Z]{2,}", s => s.Value.ToLower());
            return replacedSentence;
        }

        
        // Reading / Writing phrasal verbs ----------------------------

        private static List<FleexPhrasalVerb> ReadFleexPhrasalVerbs()
        {
            var pathToFile = PathToApplication + "Resources/fleexPhrasalVerbs.txt";
            var allLines = File.ReadAllLines(pathToFile)
                .Select(l => FleexPhrasalVerb.ParseLine(l))
                .Where(pv => pv != null)
                .ToList();
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
