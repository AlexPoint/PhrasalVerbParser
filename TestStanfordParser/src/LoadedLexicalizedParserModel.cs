using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.parser.lexparser;

namespace TestStanfordParser.src
{
    public class LoadedLexicalizedParserModel
    {
        private static string PathToApplication = Directory.GetCurrentDirectory() + "/../../";
        private static string modelsDirectory = PathToApplication + @"Resources\models";

        private static LexicalizedParser instance;

        public static LexicalizedParser Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LexicalizedParser.loadModel(modelsDirectory + @"\lexparser\englishPCFG.ser.gz");
                }
                return instance;
            }
        }
    }
}
