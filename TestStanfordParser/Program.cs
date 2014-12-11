using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.parser.lexparser;
using edu.stanford.nlp.process;
using edu.stanford.nlp.trees;
using java.util;
using TestStanfordParser.src;
using Console = System.Console;
using StringReader = java.io.StringReader;
using TaggedWord = TestStanfordParser.src.TaggedWord;

namespace TestStanfordParser
{
    class Program
    {
        private static string PathToApplication = Directory.GetCurrentDirectory() + "/../../";

        static void Main(string[] args)
        {
            // Path to models extracted from `stanford-parser-3.5.0-models.jar`
            //var jarRoot = @"c:\models\stanford-parser-full-2014-10-31\stanford-parser-3.5.0-models";
            //var modelsDirectory = jarRoot + @"\edu\stanford\nlp\models";
            var modelsDirectory = PathToApplication + @"Resources\models";

            // Loading english PCFG parser from file
            //var lp = LexicalizedParser.loadModel(modelsDirectory + @"\lexparser\englishPCFG.ser.gz");

            // This sample shows parsing a list of correctly tokenized words
            /*var sent = new[] { "This", "is", "an", "easy", "sentence", "." };
            var rawWords = Sentence.toCoreLabelList(sent);
            var tree = lp.apply(rawWords);
            tree.pennPrint();*/

            // This option shows loading and using an explicit tokenizer
            
            //Console.WriteLine("\n{0}\n", tdl);

            // Extract collapsed dependencies from parsed tree
            /*var tp = new TreePrint("typedDependenciesCollapsed");
            tp.printTree(tree2);*/


            var sentences = new List<string>()
            {
                "He keeps barging in and asking stupid questions when I'm trying to work.",
                "If you bash your monitor about like that, it won't last long."
            };
            foreach (var sentence in sentences)
            {
                var deps = DependencyParser.ParseDepencyRelationshipsInSentence(sentence);
                Console.WriteLine(sentence);
                foreach (var dep in deps)
                {
                    Console.WriteLine(dep);
                }
                Console.WriteLine("----");
            }

            Console.WriteLine("======== END ==========");
            Console.ReadKey();
        }

        
    }
}
