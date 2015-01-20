using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace PhrasalVerbParser.Src
{
    class FleexPhrasalVerb: PhrasalVerb
    {
        private bool? _inseparable;
        public override bool? Inseparable
        {
            get { return _inseparable; }
        }

        private bool? _mandatorySeparable;

        public override bool? SeparableMandatory
        {
            get { return _mandatorySeparable; }
        }
        public bool IsMissingOnWordreference { get; set; }


        // Constructors ---------------------
        
        public FleexPhrasalVerb(string name, bool isMissingOnWr, bool? inseparable, bool? mandatorySeparable)
        {
            this.Name = name;
            this.IsMissingOnWordreference = isMissingOnWr;
            this._inseparable = inseparable;
            this._mandatorySeparable = mandatorySeparable;
        }

        // Utilities ------------------------

        public static FleexPhrasalVerb ParseLine(string line)
        {
            var parts = line.Split('\t').ToList();
            if (parts.Count == 4)
            {
                var name = parts[0];
                var isMissingOnWr = Convert.ToBoolean(int.Parse(parts[1]));
                
                bool? inseparable = null;
                if (parts[2] != "NULL")
                {
                    inseparable = Convert.ToBoolean(int.Parse(parts[2]));
                }

                bool? mandatorySeparable = null;
                if (parts[3] != "NULL")
                {
                    mandatorySeparable = Convert.ToBoolean(int.Parse(parts[3]));
                }

                return new FleexPhrasalVerb(name, isMissingOnWr, inseparable, mandatorySeparable);
            }

            Console.WriteLine("Couldn't parse line '{0}' to phrasal verb", line);
            return null;
        }
    }
}
