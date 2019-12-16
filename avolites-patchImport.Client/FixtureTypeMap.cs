using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace avolites_patchImport
{
    public class FixtureTypeMap
    {
        public string FixtureName { get; set; }

        public string ModeName { get; set; }

        public TitanFixtureLink SelectedMatch { get; set; }

        public IEnumerable<TitanFixtureLink> Matches { get; set; }

        public string Key
        {
            get
            {
                return GetKey(FixtureName, ModeName);
            }
        }

        public static string GetKey(string fixtureName, string modeName)
        {
            return fixtureName + "/" + modeName;
        }

        public override bool Equals(object obj)
        {
            return Key == ((FixtureTypeMap)obj).Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
