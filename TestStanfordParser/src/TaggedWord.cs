using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStanfordParser.src
{
    public class TaggedWord
    {
        public string Word { get; set; }
        public string Pos { get; set; }

        public TaggedWord(string word, string pos)
        {
            this.Word = word;
            this.Pos = pos;
        }
    }
}
