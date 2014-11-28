using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhrasalVerbParser.src
{
    public class PhrasalVerb
    {
        public string Name { get; set; }

        public List<Usage> Usages { get; set; }

        public void Print()
        {
            Console.WriteLine(this.Name);
            foreach (var usage in Usages)
            {
                Console.WriteLine("------");
                usage.Print();
            }
        }
    }

}
