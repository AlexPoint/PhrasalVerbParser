using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhrasalVerbParser.Src
{
    [Serializable]
    public class PhrasalVerb
    {
        public string Name { get; set; }
        public List<Usage> Usages { get; set; }

        public string Root
        {
            get { return string.IsNullOrEmpty(this.Name) ? "" : this.Name.Split(' ').First(); }
        }
        public string Particle1
        {
            get { return string.IsNullOrEmpty(this.Name) && this.Name.Split(' ').Length > 1 ? 
                "" : this.Name.Split(' ')[1]; }
        }
        public string Particle2
        {
            get { return string.IsNullOrEmpty(this.Name) && this.Name.Split(' ').Length > 2 ? 
                "" : this.Name.Split(' ')[2]; }
        }
        public string Particle3
        {
            get
            {
                return string.IsNullOrEmpty(this.Name) && this.Name.Split(' ').Length > 3 ?
                    "" : this.Name.Split(' ')[3];
            }
        }
        public List<string> Particles
        {
            get
            {
                return string.IsNullOrEmpty(this.Name) ? 
                    new List<string>() : this.Name.Split(' ').Skip(1).ToList();
            }
        }

        public virtual bool? Inseparable
        {
            get { return this.Usages.All(u => u.Inseparable); }
        }

        public virtual bool? SeparableMandatory
        {
            get { return this.Usages.All(u => u.SeparableMandatory); }
        }
        

        public void Print()
        {
            Console.WriteLine(this.Name);
            foreach (var usage in Usages)
            {
                Console.WriteLine("--");
                usage.Print();
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

}
