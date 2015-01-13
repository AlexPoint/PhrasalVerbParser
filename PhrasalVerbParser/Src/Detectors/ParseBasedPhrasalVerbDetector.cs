using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LemmaSharp.Classes;
using OpenNLP.Tools.Parser;
using OpenNLP.Tools.Trees;

namespace PhrasalVerbParser.Src.Detectors
{
    class ParseBasedPhrasalVerbDetector
    {
        private readonly EnglishTreebankParser parser;
        private readonly Lemmatizer lemmatizer;

        public ParseBasedPhrasalVerbDetector(EnglishTreebankParser parser, Lemmatizer lemmatizer)
        {
            this.parser = parser;
            this.lemmatizer = lemmatizer;
        }

        public bool IsMatch(string sentence, PhrasalVerb phrasalVerb)
        {
            var pv = MatchingPhrasalVerbs(sentence, new List<PhrasalVerb>() {phrasalVerb});
            return pv.Any();
        }

        public List<PhrasalVerb> MatchingPhrasalVerbs(string sentence, List<PhrasalVerb> phrasalVerbs)
        {
            var dependencies = ComputeDependencies(sentence).ToList();
            var matchingPhrasalVerbs = new List<PhrasalVerb>();

            foreach (var phrasalVerb in phrasalVerbs)
            {
                // get relevant dependencies found
                var parts = phrasalVerb.Name.Split(' ');
                var root = parts.First();
                // find dependencies for this root
                var relevantDepedencies = dependencies
                    .Where(d => string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()), StringComparison.InvariantCultureIgnoreCase))
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

        private IEnumerable<TypedDependency> ComputeDependencies(string sentence)
        {
            var parse = parser.DoParse(sentence);
            // Extract dependencies from lexical tree
            var tlp = new PennTreebankLanguagePack();
            var gsf = tlp.GrammaticalStructureFactory();
            var tree = new ParseTree(parse);
            //Console.WriteLine(tree);
            try
            {
                var gs = gsf.NewGrammaticalStructure(tree);
                return gs.TypedDependencies();
            }
            catch (Exception)
            {
                Console.WriteLine("Exception when computing deps for {0}", sentence);
                return new List<TypedDependency>();
            }
        }
    }
}
