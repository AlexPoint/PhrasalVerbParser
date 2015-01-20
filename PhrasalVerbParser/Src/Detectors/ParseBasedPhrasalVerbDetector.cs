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

        /*public bool IsMatch(string sentence, PhrasalVerb phrasalVerb)
        {
            var tokens = tokenizer.Tokenize(sentence);
            var pv = MatchingPhrasalVerbs(sentence, new List<PhrasalVerb>() {phrasalVerb});
            return pv.Any();
        }*/

        /*public List<PhrasalVerb> MatchingPhrasalVerbs(string sentence, List<PhrasalVerb> phrasalVerbs)
        {
            // tokenize sentence
            var tokens = tokenizer.Tokenize(sentence);
            var taggedWords = tagger.Tag(tokens)/*.Where(t => Regex.IsMatch(t, "[A-Z]+")).ToList()#1#;
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
                    .Where(
                        d =>
                            ((string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()),
                                StringComparison.InvariantCultureIgnoreCase) && d.Gov().Index() < d.Dep().Index())
                             ||
                             (string.Equals(root, lemmatizer.Lemmatize(d.Dep().GetWord()),
                                 StringComparison.InvariantCultureIgnoreCase) && d.Dep().Index() < d.Gov().Index()))
                            && (!phrasalVerb.Inseparable || Math.Abs(d.Dep().Index() - d.Gov().Index()) == 1)
                                // for non separable verbs
                            && (!phrasalVerb.SeparableMandatory || Math.Abs(d.Dep().Index() - d.Gov().Index()) > 1)
                    // for separable mandatory verbs
                    //&& d.Gov().Index() >= 1 && IsVerb(taggedWords[d.Gov().Index() - 1])
                    )
                    .ToList();

                // We take only the 2nd part
                // For phrasal verbs with several particles, that's a good approximation for now
                // (we could check that all the particles are also linked)
                if (relevantDepedencies.Any() && parts.Count() > 1)
                {
                    var particle1 = parts[1];
                    var prtDependencies = relevantDepedencies.Where(d => d.Reln().GetShortName() == "prt").ToList();
                    if (prtDependencies.Any())
                    {
                        // if root has a prt dependency, don't look at other relations
                        if (prtDependencies
                            .Any(d => string.Equals(particle1, d.Dep().GetWord(),StringComparison.InvariantCultureIgnoreCase)
                                || string.Equals(particle1, d.Gov().GetWord(), StringComparison.InvariantCultureIgnoreCase)))
                        {
                            matchingPhrasalVerbs.Add(phrasalVerb);
                        }
                    }
                    else
                    {
                        // otherwise, look at all the other relations
                        var relevantRelationships = relevantDepedencies
                            .Where(d => string.Equals(particle1, d.Dep().GetWord(), StringComparison.InvariantCultureIgnoreCase)
                                || string.Equals(particle1, d.Gov().GetWord(), StringComparison.InvariantCultureIgnoreCase))
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
        }*/

        public List<PhrasalVerb> MatchingPhrasalVerbs(string sentence, List<PhrasalVerb> phrasalVerbs)
        {
            // tokenize sentence
            var tokens = tokenizer.Tokenize(sentence);
            // create parse tree
            var parse = parser.DoParse(tokens);
            // retrieve dependencies
            var dependencies = ComputeDependencies(parse).ToList();

            var matchingPhrasalVerbs = new List<PhrasalVerb>();
            foreach (var phrasalVerb in phrasalVerbs)
            {
                // get relevant dependencies found
                var parts = phrasalVerb.Name.Split(' ').ToList();
                var root = parts.First();
                // find dependencies for this root
                var rootRelatedDependencies = dependencies
                    .Where(d => // the (lemmatized) token must be equal to the gov/dep of the dependency
                        ((string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                            && d.Gov().Index() < d.Dep().Index())
                        || (string.Equals(root, lemmatizer.Lemmatize(d.Dep().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                            && d.Dep().Index() < d.Gov().Index()))
                            // if the phrasal verb is inseparable, no word must be between the root and the particle
                        && (!phrasalVerb.Inseparable.HasValue || (!phrasalVerb.Inseparable.Value || Math.Abs(d.Dep().Index() - d.Gov().Index()) == 1))
                            // if the phrasal verb is mandatory seprable, at least one word must be between the root and the particle
                        && (!phrasalVerb.SeparableMandatory.HasValue || (!phrasalVerb.SeparableMandatory.Value || Math.Abs(d.Dep().Index() - d.Gov().Index()) > 1))
                    )
                    .ToList();

                // We take only the 2nd part
                // For phrasal verbs with several particles, that's a good approximation for now
                // (we could check that all the particles are also linked)
                if (rootRelatedDependencies.Any() && parts.Count() > 1)
                {
                    var particle1 = parts[1];
                    var relevantDependencies = rootRelatedDependencies.Where(d => d.Reln().GetShortName() == "prt").ToList();
                    if (!relevantDependencies.Any())
                    {
                        // if no "prt" relation, take all relations whatsoever.
                        relevantDependencies = rootRelatedDependencies;
                    }

                    // if one of relevant dependencies have the particle as gov/dep, it's good!
                    var rootParticle1Dependency = relevantDependencies
                        .FirstOrDefault(d => string.Equals(particle1, d.Dep().GetWord(), StringComparison.InvariantCultureIgnoreCase)
                            || string.Equals(particle1, d.Gov().GetWord(), StringComparison.InvariantCultureIgnoreCase));
                    if (rootParticle1Dependency != null)
                    {
                        if (parts.Count <= 2)
                        {
                            // phrasal verb has 1 particle only; we're done
                            matchingPhrasalVerbs.Add(phrasalVerb);
                        }
                        else
                        {
                            // otherwise, check that the other particles are in the sentence (approximation)
                            var lastTokenIndex = Math.Max(rootParticle1Dependency.Gov().Index(), rootParticle1Dependency.Dep().Index()) - 1;
                            var endOfSentenceTokens = tokens.Skip(lastTokenIndex).ToList();
                            if (parts.Skip(2).All(endOfSentenceTokens.Contains))
                            {
                                matchingPhrasalVerbs.Add(phrasalVerb);
                            }
                        }
                    }
                }
            }

            return matchingPhrasalVerbs;
        }

        public bool IsMatch(string sentence, PhrasalVerb pv)
        {
            var tokens = tokenizer.Tokenize(sentence).ToList();
            return IsMatch(tokens, pv);
        }

        public bool IsMatch(List<string> tokens, PhrasalVerb pv)
        {
            try
            {
                // create parse tree
                var parse = parser.DoParse(tokens);
                // compute dependencies between words for this sentence
                var dependencies = ComputeDependencies(parse).ToList();

                // get relevant dependencies found
                var parts = pv.Name.Split(' ').ToList();
                var root = parts.First();
                // find dependencies for this root
                var rootRelatedDependencies = dependencies
                    .Where(d => // the (lemmatized) token must be equal to the gov/dep of the dependency
                        ((string.Equals(root, lemmatizer.Lemmatize(d.Gov().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                            && d.Gov().Index() < d.Dep().Index())
                        || (string.Equals(root, lemmatizer.Lemmatize(d.Dep().GetWord()), StringComparison.InvariantCultureIgnoreCase)
                            && d.Dep().Index() < d.Gov().Index()))
                            // if the phrasal verb is inseparable, no word must be between the root and the particle
                        && (!pv.Inseparable.HasValue || (!pv.Inseparable.Value || Math.Abs(d.Dep().Index() - d.Gov().Index()) == 1))
                            // if the phrasal verb is mandatory seprable, at least one word must be between the root and the particle
                        && (!pv.SeparableMandatory.HasValue || (!pv.SeparableMandatory.Value || Math.Abs(d.Dep().Index() - d.Gov().Index()) > 1))
                    )
                    .ToList();

                // We take only the 2nd part
                // For phrasal verbs with several particles, that's a good approximation for now
                // (we could check that all the particles are also linked)
                if (rootRelatedDependencies.Any() && parts.Count() > 1)
                {
                    var particle1 = parts[1];
                    var relevantDependencies = rootRelatedDependencies.Where(d => d.Reln().GetShortName() == "prt").ToList();
                    if (!relevantDependencies.Any())
                    {
                        // if no "prt" relation, take all relations whatsoever.
                        relevantDependencies = rootRelatedDependencies;
                    }

                    // if one of relevant dependencies have the particle as gov/dep, it's good!
                    var rootParticle1Dependency = relevantDependencies
                        .FirstOrDefault(d => string.Equals(particle1, d.Dep().GetWord(), StringComparison.InvariantCultureIgnoreCase)
                            || string.Equals(particle1, d.Gov().GetWord(), StringComparison.InvariantCultureIgnoreCase));
                    if (rootParticle1Dependency != null)
                    {
                        if (parts.Count <= 2)
                        {
                            // phrasal verb has 1 particle only; we're done
                            return true;
                        }
                        else
                        {
                            // otherwise, check that the other particles are in the sentence (approximation)
                            var lastTokenIndex = Math.Max(rootParticle1Dependency.Gov().Index(),
                            rootParticle1Dependency.Dep().Index()) - 1;
                            var endOfSentenceTokens = tokens.Skip(lastTokenIndex).ToList();
                            return parts.Skip(2).All(endOfSentenceTokens.Contains);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // fail silently
                Console.WriteLine("Exception raised when trying to match '{0}' in '{1}'", pv, string.Join(" ", tokens));
            }

            // if we get here, matching failed
            return false;
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
