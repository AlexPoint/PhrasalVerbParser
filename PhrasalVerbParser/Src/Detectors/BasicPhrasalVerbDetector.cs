using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemmaSharp.Classes;
using OpenNLP.Tools.Tokenize;

namespace PhrasalVerbParser.Src.Detectors
{
    /// <summary>
    /// A basic detector which tests if the root and the particles and present in the phrase
    /// (in the right order)
    /// </summary>
    class BasicPhrasalVerbDetector
    {
        private readonly EnglishMaximumEntropyTokenizer tokenizer;
        private readonly Lemmatizer lemmatizer;

        public BasicPhrasalVerbDetector(EnglishMaximumEntropyTokenizer tokenizer, Lemmatizer lemmatizer)
        {
            this.tokenizer = tokenizer;
            this.lemmatizer = lemmatizer;
        }

        public bool IsMatch(string sentence, PhrasalVerb phrasalVerb)
        {
            var tokens = tokenizer.Tokenize(sentence);
            var matchRoot = false;
            var particleToMatch = 0;
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (!matchRoot)
                {
                    // try to match the root first
                    matchRoot = string.Equals(token, phrasalVerb.Root, StringComparison.InvariantCultureIgnoreCase)
                                ||
                                string.Equals(lemmatizer.Lemmatize(token), phrasalVerb.Root,
                                    StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    // match all particles
                    if (phrasalVerb.Particles.Count > particleToMatch)
                    {
                        var particle = phrasalVerb.Particles[particleToMatch];
                        var isMatch = string.Equals(token, particle, StringComparison.InvariantCultureIgnoreCase);
                        if (isMatch)
                        {
                            particleToMatch++;
                            if (particleToMatch >= phrasalVerb.Particles.Count)
                            {
                                // we matched all particles
                                return true;
                            }
                        } 
                    }
                }
            }
            // if we get here, matching failed
            return false;
        }
    }
}
