using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.process;
using edu.stanford.nlp.trees;
using java.io;
using java.util;

namespace TestStanfordParser.src
{
    public class DependencyParser
    {

        public static List<DependencyRelationship> ParseDepencyRelationshipsCollapsedInSentence(string sentence)
        {
            var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
            var sent2Reader = new StringReader(sentence);
            var rawWords2 = tokenizerFactory.getTokenizer(sent2Reader).tokenize();
            sent2Reader.close();
            var tree2 = LoadedLexicalizedParserModel.Instance.apply(rawWords2);

            // Extract dependencies from lexical tree
            var tlp = new PennTreebankLanguagePack();
            var gsf = tlp.grammaticalStructureFactory();
            var gs = gsf.newGrammaticalStructure(tree2);
            var tdl = gs.typedDependenciesCollapsed();
            return ParseJavaDependecyRelationships(tdl);
        }

        public static List<DependencyRelationship> ParseDepencyRelationshipsInSentence(string sentence)
        {
            var tokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(), "");
            var sent2Reader = new StringReader(sentence);
            var rawWords2 = tokenizerFactory.getTokenizer(sent2Reader).tokenize();
            sent2Reader.close();
            var tree2 = LoadedLexicalizedParserModel.Instance.apply(rawWords2);

            // Extract dependencies from lexical tree
            var tlp = new PennTreebankLanguagePack();
            var gsf = tlp.grammaticalStructureFactory();
            var gs = gsf.newGrammaticalStructure(tree2);
            var tdl = gs.typedDependencies();
            return ParseJavaDependecyRelationships(tdl);
        }

        static List<DependencyRelationship> ParseJavaDependecyRelationships(List javaList)
        {
            return ParseJavaDependecyRelationships(javaList.toArray());
        }

        static List<DependencyRelationship> ParseJavaDependecyRelationships(Collection colection)
        {
            return ParseJavaDependecyRelationships(colection.toArray());
        }

        static List<DependencyRelationship> ParseJavaDependecyRelationships(object[] list)
        {
            var relationships = new List<DependencyRelationship>();
            foreach (var dep in list)
            {
                var dep2 = (TypedDependency)dep;
                relationships.Add(new DependencyRelationship()
                {
                    Dep = new TaggedWord(dep2.dep().word(), dep2.dep().value()),
                    Gov = new TaggedWord(dep2.gov().word(), dep2.gov().value()),
                    RelationshipType = dep2.reln().getShortName()
                });
            }
            return relationships;
        }
    }
}
