using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace PhrasalVerbParser.src
{
    [Serializable]
    public class Usage
    {
        public string Definition { get; set; }
        public string Example { get; set; }
        public bool Inseparable { get; set; }
        public bool SeparableMandatory { get; set; }
        public bool SeparableOptional { get; set; }
        public byte EnglishType { get; set; }
        public bool Intransitive { get; set; }

        public Usage() { }

        public Usage(string definition, string example, List<string> notes)
        {
            this.Definition = System.Web.HttpUtility.HtmlDecode(definition.Substring("Meaning:".Length).Trim());
            this.Example = System.Web.HttpUtility.HtmlDecode(example.Substring("Example:".Length).Trim());

            foreach (var note in notes)
            {
                SetExtraProperties(note);
            }
        }

        private void SetExtraProperties(string note)
        {
            if (string.IsNullOrEmpty(note)){ return; }

            // try to detect english type
            var englishType = src.EnglishType.TryDetectEnglishType(note);
            if (englishType != src.EnglishType.Unsupported)
            {
                this.EnglishType = englishType;
                return;
            }

            if (note == "Inseparable")
            {
                this.Inseparable = true;
                return;
            }

            if (note == "Intransitive")
            {
                this.Intransitive = true;
                return;
            }

            if (note == "Separable [obligatory]")
            {
                this.SeparableMandatory = true;
                return;
            }

            if (note == "Separable [optional]")
            {
                this.SeparableOptional = true;
                return;
            }

            Console.WriteLine("Unsupported phrasal verb note: " + note);
        }


        public void Print()
        {
            Console.WriteLine(this.Definition);
            Console.WriteLine(this.Example);
            if (this.Inseparable)
	        {
		        Console.WriteLine("inseparable");
	        }
            if (this.SeparableMandatory)
            {
                Console.WriteLine("separable mandatory");
            }
            if (this.SeparableOptional)
            {
                Console.WriteLine("separable optional");
            }
            if (this.Intransitive)
            {
                Console.WriteLine("intransitive");
            }
        }
    }


    public static class EnglishType
    {
        public const byte Unsupported = 0;
        public const byte International = 1;
        public const byte British = 2;
        public const byte American = 3;
        public const byte Australian = 3;

        public static byte TryDetectEnglishType(string note)
        {
            switch (note)
            {
                case "International English":
                    return EnglishType.International;
                case "British English":
                    return EnglishType.British;
                case "American English":
                    return EnglishType.American;
                case "Australian English":
                    return EnglishType.Australian;
                default:
                    return EnglishType.Unsupported;
            }
        }
    };
}
