using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
// Part 1: 107430936
// Part 2: 98162531 is too high
// Part 2: 23738616

namespace AdventOfCode2023
{
    enum MappingType
    {
        None,
        SeedRange,
        SeedToSoil,
        SoilToFertilizer,
        FertilizerToWater,
        WaterToLight,
        LightToTemperature,
        TemperatureToHumidity,
        HumidityToLocation
    }

    class Day5Map
    {
        Int64 _srcRangeSt;
        Int64 _destRangeSt;
        Int64 _rangeLength;

        internal Int64 RangeSt => _srcRangeSt;
        internal Int64 RangeEndExcl => _srcRangeSt + _rangeLength;

        internal Day5Map(Int64 srcRangeSt, Int64 destRangeSt, Int64 rangeLength)
        {
            _srcRangeSt = srcRangeSt;
            _destRangeSt = destRangeSt;
            _rangeLength = rangeLength;
        }

        internal Int64? GetDestinationFromSource(Int64 source)
        {
            if (source>=_srcRangeSt && source< (_srcRangeSt+_rangeLength))
            {
                return _destRangeSt + source - _srcRangeSt;
            }
            return null;
        }

        internal Int64? GetSourceFromDestination(Int64 dest)
        {
            if (dest >= _destRangeSt && dest < (_destRangeSt + _rangeLength))
            {
                return _srcRangeSt + dest- _destRangeSt;
            }
            return null;
        }

        internal bool ContainsSourceValue(Int64 source)
        {
            return source >= _srcRangeSt && source < (_srcRangeSt + _rangeLength);
        }
    }

    internal class Day5
    {
        int _lineNumber;
        List<string> _data = new List<string>();
        List<Int64> _seeds = new List<Int64>();

        Dictionary<MappingType, List<Day5Map>> _maps = new Dictionary<MappingType, List<Day5Map>>();
        Dictionary<string, MappingType> _mappingHeadingToType = new Dictionary<string, MappingType>();

        internal void Run(string filepath)
        {
            _data = ReadTextData(filepath);
            bool parseSeedNumbersAsRange = true;
            ParseData(parseSeedNumbersAsRange);
            Int64 nearestLocationNumber = 0;
            if (parseSeedNumbersAsRange)
            {
                nearestLocationNumber = GetNearestLocationNumberForSeedRanges_ReverseSearch();
            }
            else
            {
                nearestLocationNumber = GetNearestLocationNumberForSeeds();
            }
            Console.WriteLine($"Nearest location is {nearestLocationNumber}");
        }

        Int64 GetNearestLocationNumberForSeedRanges_ReverseSearch()
        {
            for (Int64 location = 0; location < Int64.MaxValue; ++location)
            {
                Int64? seedNumber = GetSeedNumberFromLocation(location);
                if (seedNumber.HasValue)
                {
                    return location;
                }
            }
            return -1;
        }

        Int64 GetNearestLocationNumberForSeedRanges_BruteForce()
        {
            Int64? nearestLocationNumber = null;
            List<Day5Map>? seedRangeMap = GetMapListForMappingType(MappingType.SeedRange, false);
            if (seedRangeMap == null)
            {
                throw new Exception("No map returned for seed ranges");
            }
            int mapIndex = -1;
            foreach (Day5Map map in seedRangeMap)
            {
                ++mapIndex;
                Int64 rangeStart = map.RangeSt;
                Int64 rangeEnd = map.RangeEndExcl;
                for (Int64 seedNumber = rangeStart; seedNumber < rangeEnd; ++seedNumber)
                {
                    Int64 seedLocation = GetNearestLocationNumberForSeed(seedNumber);
                    if (nearestLocationNumber == null || seedLocation < nearestLocationNumber.Value)
                    {
                        nearestLocationNumber = seedLocation;
                    }
                }
                Console.WriteLine($"Processed map {mapIndex} out of {seedRangeMap.Count}");
            }

            return nearestLocationNumber.GetValueOrDefault();

        }

        Int64 GetNearestLocationNumberForSeeds()
        {
            Int64? nearestLocationNumber = null;
            foreach (Int64 seedNumber in _seeds)
            {
                Int64 seedLocation = GetNearestLocationNumberForSeed(seedNumber);
                if (nearestLocationNumber == null || seedLocation < nearestLocationNumber.Value)
                {
                    nearestLocationNumber = seedLocation;
                }
            }

            return nearestLocationNumber.GetValueOrDefault();
        }

        Int64? GetSeedNumberFromLocation(Int64 location)
        {
            List<Day5Map> maps = GetMapListForMappingType_MustExist(MappingType.HumidityToLocation);

            Int64 humidityNumber = GetSourceNumberFromDest(location, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.TemperatureToHumidity);
            Int64 temperatureNumber = GetSourceNumberFromDest(humidityNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.LightToTemperature);
            Int64 lightNumber = GetSourceNumberFromDest(temperatureNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.WaterToLight);
            Int64 waterNumber = GetSourceNumberFromDest(lightNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.FertilizerToWater);
            Int64 fertilizerNumber = GetSourceNumberFromDest(waterNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.SoilToFertilizer);
            Int64 soilNumber = GetSourceNumberFromDest(fertilizerNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.SeedToSoil);
            Int64 seedNumber = GetSourceNumberFromDest(soilNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.SeedRange);
            foreach (Day5Map map in maps)
            {
                if (map.ContainsSourceValue(seedNumber))
                {
                    return seedNumber;
                }
            }

            return null;
        }

        Int64 GetNearestLocationNumberForSeed(Int64 seedNumber)
        {
            List<Day5Map>? maps = GetMapListForMappingType_MustExist(MappingType.SeedToSoil);
            Int64 soilNumber = GetDestNumberFromSource(seedNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.SoilToFertilizer);
            Int64 fertilizerNumber = GetDestNumberFromSource(soilNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.FertilizerToWater);
            Int64 waterNumber = GetDestNumberFromSource(fertilizerNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.WaterToLight);
            Int64 lightNumber = GetDestNumberFromSource(waterNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.LightToTemperature);
            Int64 temperatureNumber = GetDestNumberFromSource(lightNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.TemperatureToHumidity);
            Int64 humidityNumber = GetDestNumberFromSource(temperatureNumber, maps);

            maps = GetMapListForMappingType_MustExist(MappingType.HumidityToLocation);
            Int64 locationNumber = GetDestNumberFromSource(humidityNumber, maps);

            return locationNumber;
        }

        Int64 GetDestNumberFromSource(Int64 sourceNumber, List<Day5Map> maps)
        {
            foreach (Day5Map map in maps)
            {
                Int64? destNumber = map.GetDestinationFromSource(sourceNumber);
                if (destNumber != null)
                {
                    return destNumber.Value;
                }
            }
            return sourceNumber;
        }

        Int64 GetSourceNumberFromDest(Int64 destNumber, List<Day5Map> maps)
        {
            foreach (Day5Map map in maps)
            {
                Int64? source = map.GetSourceFromDestination(destNumber);
                if (source != null)
                {
                    return source.Value;
                }
            }
            return destNumber;
        }

        List<Day5Map> GetMapListForMappingType_MustExist(MappingType mappingType)
        {
            List<Day5Map>? toReturn = GetMapListForMappingType(mappingType, false);
            if (toReturn == null)
            {
                throw new Exception($"Could not find mappings for type {mappingType}");
            }
            return toReturn;
        }

        List<Day5Map>? GetMapListForMappingType(MappingType mappingType, bool createIfNeeded)
        {
            List<Day5Map>? toReturn = null;
            if (!_maps.TryGetValue(mappingType, out toReturn) &&
                createIfNeeded)
            {
                toReturn = new List<Day5Map>();
                _maps[mappingType] = toReturn;
            }
            return toReturn;
        }

        List<string> ReadTextData(string filepath, int maxLines = 0)
        {
            List<string> toReturn = new List<string>();
            string? line;

            StreamReader sr = new StreamReader(filepath);
            line = sr.ReadLine();
            while (line != null)
            {
                toReturn.Add(line.ToLower());
                line = sr.ReadLine();
                if (maxLines > 0 && toReturn.Count == maxLines)
                {
                    break;
                }
            }
            sr.Close();
            return toReturn;
        }

        void ParseData(bool parseSeedNumbersAsRange)
        {
            InitialiseMappingHeadingDictionary();
            MappingType state= MappingType.None;
            _lineNumber = -1;
            foreach (string line in _data)
            {
                ++_lineNumber;
                if (state == MappingType.None)
                {
                    if (line.StartsWith("seeds:"))
                    {
                        ProcessSeedsLine(line);
                        state = MappingType.None;
                    }
                    else
                    {
                        state = DetermineStateFromHeading(line);
                    }
                }
                else 
                {
                    state = ProcessMappingLine(line, state);
                }
            }

            if (parseSeedNumbersAsRange)
            {
                CreateSeedRangesFromSeedNumbers();
            }
        }
        void InitialiseMappingHeadingDictionary()
        {
            _mappingHeadingToType["seed-to-soil map:"] = MappingType.SeedToSoil;
            _mappingHeadingToType["soil-to-fertilizer map:"] = MappingType.SoilToFertilizer;
            _mappingHeadingToType["fertilizer-to-water map:"] = MappingType.FertilizerToWater;
            _mappingHeadingToType["water-to-light map:"] = MappingType.WaterToLight;
            _mappingHeadingToType["light-to-temperature map:"] = MappingType.LightToTemperature;
            _mappingHeadingToType["temperature-to-humidity map:"] = MappingType.TemperatureToHumidity;
            _mappingHeadingToType["humidity-to-location map:"] = MappingType.HumidityToLocation;
        }

        MappingType DetermineStateFromHeading(string line)
        {
            line = line.Trim();
            if (String.IsNullOrEmpty(line))
            {
                return MappingType.None;
            }
            MappingType toReturn;
            if (_mappingHeadingToType.TryGetValue(line, out toReturn) == false)
            {
                throw new Exception($"Could not determine type on line {_lineNumber}: {line}");
            }
            return toReturn;
        }

        void ProcessSeedsLine(string line)
        {
            int indexOfColon = line.IndexOf(':');
            if (indexOfColon == -1)
            {
                return;
            }
            string seedsNumbers = line.Substring(indexOfColon + 2);
            string[] seedsNumbersAsArray = seedsNumbers.Split(' ');
            foreach(string num in seedsNumbersAsArray)
            {
                string numToProcessAsString = num.Trim();
                if (numToProcessAsString.Length>0)
                {
                    Int64 seedNumber = Int64.Parse(numToProcessAsString);
                    _seeds.Add(seedNumber);
                }
            }
        }

        void CreateSeedRangesFromSeedNumbers()
        {
            List<Day5Map>? mapToAddTo = GetMapListForMappingType(MappingType.SeedRange, true);
            if (mapToAddTo == null)
            {
                throw new Exception("No map returned even though requested to create one for seed range");
            }
            for (int i=0;i< _seeds.Count;i+=2)
            {
                Int64 seedStart = _seeds[i];
                Int64 seedRange = _seeds[i + 1];

                Day5Map seedMap = new Day5Map(seedStart,0,seedRange);

                mapToAddTo.Add(seedMap);
            }
        }

        MappingType ProcessMappingLine(string line, MappingType currentType)
        {
            line = line.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return MappingType.None;
            }

            List<Int64> numbers = ParseThreeNumbersFromString(line);
            Day5Map newMap = new Day5Map(numbers[1], numbers[0], numbers[2]);

            List<Day5Map>? mapToAddTo = GetMapListForMappingType(currentType, true);
            if (mapToAddTo == null)
            {
                throw new Exception("No map returned even though requested to create one");
            }
            mapToAddTo.Add(newMap);

            return currentType;
        }

        private List<Int64> ParseThreeNumbersFromString(string line)
        {
            string[] numbersAsStrings = line.Split(" ");
            List<Int64> numbers = new List<Int64>();
            foreach (string potentialNumberString in numbersAsStrings)
            {
                string numberString = potentialNumberString.Trim();
                if (numberString.Length > 0)
                {
                    Int64 number = Int64.Parse(numberString);
                    numbers.Add(number);
                }
            }
            if (numbers.Count != 3)
            {
                throw new Exception($"Line {_lineNumber} does not have three numbers, it is has {numbers.Count}.");
            }

            return numbers;
        }
    }
}
