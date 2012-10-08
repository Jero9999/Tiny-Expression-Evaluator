using System.Globalization;

namespace Formy.Evaluation.Test
{
    public struct Person
    {
        public string Name;
        public int Age;
        public bool Married;

        public string this[string key]
        {
            get { return key; }
        }

        public string this[long index]
        {
            get { return index.ToString(CultureInfo.InvariantCulture); }
        }

        public override string ToString()
        {
            return Name + "(" + Age + ")";
        }
    }
}