using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace avolites_patchImport
{
    public class PatchImporter
    {
        public Dictionary<string,FixtureTypeMap> FixtureTypes { get; set; } = new Dictionary<string,FixtureTypeMap>();

        public List<CSVPatchIntent> Patch { get; set; } = new List<CSVPatchIntent>();


        public async Task ReadCSV(Stream file)
        {
            FixtureTypes.Clear();
            Patch.Clear();

            // Read into buffer and act (uses less memory)
            using (StreamReader stream = new StreamReader(file))
            {
                //Ignore Header
                string[] header = (await stream.ReadLineAsync()).Split(',');

                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    if(!string.IsNullOrEmpty(line))
                    {
                        string[] csvParts = line.Split(',');

                        var patch = CSVPatchIntent.FromCSV(line, header);

                        Patch.Add(patch);

                        var type = new FixtureTypeMap()
                        {
                            FixtureName = patch.FixtureName,
                            ModeName = patch.ModeName
                        };

                        if (!FixtureTypes.ContainsKey(type.Key))
                            FixtureTypes.Add(type.Key, type);
                    }
                }
            }
        }

        public async Task LinkPersonality(HttpClient http, string url)
        {
            foreach(var fixtureType in FixtureTypes)
            {
                TitanFixtureLink[] matches = await http.GetJsonAsync<TitanFixtureLink[]>(url + $"{fixtureType.Value.FixtureName} {fixtureType.Value.ModeName}");
                fixtureType.Value.Matches = matches;

                if(matches.Length > 0)
                {
                    fixtureType.Value.SelectedMatch = matches.First();
                }                
            }
        }

        public async Task DoPatch(HttpClient http, Func<string,string> formatUrl)
        {
            try
            {
                foreach(var intent in Patch)
                {
                
                    FixtureTypeMap mapping;
                    if (FixtureTypes.TryGetValue(FixtureTypeMap.GetKey(intent.FixtureName, intent.ModeName), out mapping))
                    {
                        if (mapping.SelectedMatch != null && intent.DMXAddress != null)
                        {
                            string addressData = JsonSerializer.Serialize(intent.DMXAddress);
                            //string addressData = JsonSerializer.Serialize(intent.DMXAddress, new JsonSerializerOptions()
                            //{
                            //    PropertyNamingPolicy = new WebAPINameingPolicy()
                            //});
                            Console.WriteLine(addressData);

                            var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post,new Uri(formatUrl("set/2/Patch/CurrentDmxAssignment")))
                            {
                                Content = new StringContent(addressData, Encoding.UTF8, "application/json")
                            });
                            //await http.PostAsync(,new StringContent(addressData, Encoding.UTF8, "application/json"));
                            await http.GetStringAsync(formatUrl($"script/2/Fixtures/PatchFixturesToVacantHandles?group=Fixtures&fixtureManufacturer={mapping.SelectedMatch.Manufacturer}&fixtureName={mapping.SelectedMatch.Model}&fixtureMode={mapping.SelectedMatch.Mode}&currentFixtureQuantity=1&currentDmxSpacing=0&patchedHandles=Expert.Patch.Fixture.PatchedFixtures"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
