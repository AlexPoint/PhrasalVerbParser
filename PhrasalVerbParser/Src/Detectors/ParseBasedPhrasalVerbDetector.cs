using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LemmaSharp.Classes;
using OpenNLP.Tools;
using OpenNLP.Tools.Parser;
using OpenNLP.Tools.PosTagger;
using OpenNLP.Tools.Tokenize;
using OpenNLP.Tools.Trees;

namespace PhrasalVerbParser.Src.Detectors
{
    class ParseBasedPhrasalVerbDetector
    {
        private readonly EnglishTreebankParser parser;
        private readonly Lemmatizer lemmatizer;
        private readonly EnglishMaximumEntropyTokenizer tokenizer;
        private readonly EnglishMaximumEntropyPosTagger tagger;


        public ParseBasedPhrasalVerbDetector(EnglishTreebankParser parser, Lemmatizer lemmatizer, 
            EnglishMaximumEntropyTokenizer tokenizer, EnglishMaximumEntropyPosTagger tagger)
        {
            this.parser = parser;
            this.lemmatizer = lemmatizer;
            this.tokenizer = tokenizer;
            this.tagger = tagger;
        }

        public bool IsMatch(string sentence, PhrasalVerb phrasalVerb)
        {
            var pv = MatchingPhrasalVerbs(sentence, new List<PhrasalVerb>() {phrasalVerb});
            return pv.Any();
        }

        public List<PhrasalVerb> MatchingPhrasalVerbs(string sentence, List<PhrasalVerb> phrasalVerbs)
        {
            // tokenize sentence
            var tokens = tokenizer.Tokenize(sentence);
            var taggedWords = tagger.Tag(tokens)/*.Where(t => Regex.IsMatch(t, "[A-Z]+")).ToList()*/;
            // create parse tree
            var parse = parser.DoParse(tokens);
            // retrieve dependencies
            var dependencies = ComputeDependencies(parse).ToList();

            // compute matching phrasal verbs
            var matchingPhrasalVerbs = new List<PhrasalVerb>();
            foreach (var phrasalVerb in phrasalVerbs)
            {
                // get relevant dependencies found
                var parts = phrasalVerb.Name.Split(' ');
                var root = parts.First();
                // find dependencies for this root
                var relevantDepedencies = dependencies
                    .Where(d => string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                    && d.Gov().Index() >= 1 && IsVerb(taggedWords[d.Gov().Index() - 1]))
                    .ToList();

                // We take only the 2nd part
                // For phrasal verbs with several particles, that's a good approximation for now
                // (we could check that all the particles are also linked)
                if (relevantDepedencies.Any() && parts.Count() > 1)
                {
                    var particle1 = parts[1];
                    /*var firstDep = relevantDepedencies
                        .Where(d => d.Dep().Index() > d.Gov().Index())
                        .OrderBy(d => d.Reln().GetShortName() == "prt")
                        /*.ThenBy(d => d.Reln().GetShortName() == "prep")
                        .ThenBy(d => d.Reln().GetShortName() == "advmod")#1#
                        .ThenBy(d => d.Dep().Index())
                        //.OrderBy(d => d.Dep().Index())
                        .FirstOrDefault();
                    if (firstDep != null &&
                        string.Equals(firstDep.Dep().GetWord(), particle1, StringComparison.InvariantCultureIgnoreCase))
                    {
                        matchingPhrasalVerbs.Add(phrasalVerb);
                    }*/
                    var prtDependencies = relevantDepedencies.Where(d => d.Reln().GetShortName() == "prt").ToList();
                    if (prtDependencies.Any())
                    {
                        // if root has a prt dependency, don't look at other relations
                        if (prtDependencies.Any(d =>
                                    string.Equals(particle1, d.Dep().GetWord(),
                                        StringComparison.InvariantCultureIgnoreCase)))
                        {
                            matchingPhrasalVerbs.Add(phrasalVerb);
                        }
                    }
                    else
                    {
                        // otherwise, look at all the other relations
                        var relevantRelationships = dependencies
                        .Where(d => (string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                            && string.Equals(particle1, d.Dep().GetWord(), StringComparison.InvariantCultureIgnoreCase))
                            //|| (root == lemmatizer.Lemmatize(d.Dep().GetWord()) && last == d.Gov().GetWord())
                                    )
                                    .ToList();
                        if (relevantRelationships.Any())
                        {
                            matchingPhrasalVerbs.Add(phrasalVerb);
                        }
                    }
                }
            }
            return matchingPhrasalVerbs;
        }

        private bool IsVerb(string tag)
        {
            return tag == "VB" || tag == "VBD" || tag == "VBG" || tag == "VBN"
                   || tag == "VBP" || tag == "VBZ";
        }

        private IEnumerable<TypedDependency> ComputeDependencies(Parse parse)
        {
            // Extract dependencies from lexical tree
            var tlp = new PennTreebankLanguagePack();
            var gsf = tlp.GrammaticalStructureFactory();
            var tree = new ParseTree(parse);
            try
            {
                var gs = gsf.NewGrammaticalStructure(tree);
                return gs.TypedDependencies();
            }
            catch (Exception)
            {
                Console.WriteLine("Exception when computing deps for {0}", parse);
                return new List<TypedDependency>();
            }
        }
    }
}
