using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStanfordParser.src
{
    public class DependencyRelationship
    {
        public TaggedWord Dep { get; set; }
        public TaggedWord Gov { get; set; }
        public string RelationshipType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1}, {2})", this.RelationshipType, this.Gov.Word, this.Dep.Word);
        }
    }
}
