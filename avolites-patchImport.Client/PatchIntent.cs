using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace avolites_patchImport
{
    public class CSVPatchIntent
    {
        public string FixtureName { get; set; }

        public string ModeName { get; set; }

        public CSVDmxAssignment DMXAddress { get; set; }

        public int ChannelCount { get; set; }

        public int UnitNumber { get; set; }

        public static CSVPatchIntent FromCSV(string csvLine,string[] header)
        {
            string[] csvParts = csvLine.Split('\t');

            CSVPatchIntent intent = new CSVPatchIntent()
            {
                FixtureName = csvParts[0],
                ModeName = csvParts[8],
                DMXAddress = CSVDmxAssignment.Parse(csvParts[7])
            };
            return intent;
        }
    }

    [DataContract]
    public class CSVDmxAssignment
    {
        [DataMember(Name ="Address")]
        public int Address { get; set; }

        [DataMember(Name = "Universe")]
        public int Universe { get; set; }

        public static CSVDmxAssignment Parse(string address)
        {
            string[] parts = address.Split('.');

            if (parts.Length != 2)
                return null;

            return new CSVDmxAssignment()
            {
                Universe = (int)(parts[0][0] - 'A'),
                Address = int.Parse(parts[1])-1
            };
        }

        public override string ToString()
        {
            return $"{Universe + 1}.{Address + 1}";
        }
    }

}
