using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole;
using Newtonsoft.Json;
using TroutDash.EntityFramework.Models;
using TroutStreamMangler.MN.Models;
using TroutStreamMangler.US.Models;

namespace TroutStreamMangler.US
{
    public class ExportUsData : ConsoleCommand
    {
        private readonly Lazy<ILookup<int, FipsCode>> _fips;
        private readonly Lazy<Dictionary<string, Abbreviation>> _abbrevs;

        public ExportUsData()
        {
            _abbrevs = new Lazy<Dictionary<string, Abbreviation>>(CreateAbbreviations);
            _fips = new Lazy<ILookup<int, FipsCode>>(CreateFipCode);
        }

        private static Dictionary<string, Abbreviation> CreateAbbreviations()
        {
            var abbreviationsFile = new FileInfo(@"US\Data\US_State_abbreviations.json");
            return JsonConvert.DeserializeObject<IEnumerable<Abbreviation>>(File.ReadAllText(abbreviationsFile.FullName))
                .ToDictionary(i => i.State_name);
        }

        private ILookup<int, FipsCode> CreateFipCode()
        {
            var fipsCodesFile = new FileInfo(@"US\Data\US_FIPS_CODES.json");

            var fipsCodes = JsonConvert.DeserializeObject<IEnumerable<CensusFipsCodes>>(
                File.ReadAllText(fipsCodesFile.FullName))
                .Select(OnSelector).ToLookup(i => i.StateNumber);

            return fipsCodes;
        }

        private FipsCode OnSelector(CensusFipsCodes i)
        {
            return new FipsCode
            {
                CountyName = i.County_Name,
                CountyNumber = i.FIPS_County,
                StateName = i.State,
                StateNumber = i.FIPS_State,
                StateAbbreviation = _abbrevs.Value[i.State].State_abbreviation
            };
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                ImportStatesAndCounties();
            }
            catch (Exception)
            {
                throw;
            }

            return 0;
        }

        private void ImportStatesAndCounties()
        {
            using (var context = new UsShapeDataContext())
            using (var troutDashContext = new TroutDashPrototypeContext())
            {
                var states = troutDashContext.states.ToList();
                troutDashContext.states.RemoveRange(states);
                troutDashContext.SaveChanges();

                var counties = context.counties.ToLookup(c => c.statefp);
                var bigFatList = _fips.Value.Select(i => i.Key).ToList();
                foreach (var state in context.states)
                {
                    try
                    {
                        var actualState = _fips.Value[Convert.ToInt32(state.statefp)].FirstOrDefault();
                        if (actualState == null)
                        {
                            continue;
                        }

                        Console.WriteLine("Adding state " + actualState.StateName);
                        var stateCounties = counties[state.statefp];
                        var countyModels = stateCounties.Select(c => new TroutDash.EntityFramework.Models.county
                        {
                            Geom = c.Geom_3857,
                            countyfp = c.countyfp,
                            statefp = c.statefp,
                            name = c.name,
                            lsad = c.lsad
                        }).ToList();


                        var stateAbbreviation = actualState.StateAbbreviation;
                        var stateName = actualState.StateName;

                        var newState = new TroutDash.EntityFramework.Models.state
                        {
                            counties = countyModels,
                            Name = stateName,
                            short_name = stateAbbreviation,
                            statefp = state.statefp,
                            Geom = state.Geom_3857,
                        };

                        troutDashContext.states.Add(newState);
                        troutDashContext.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }
}