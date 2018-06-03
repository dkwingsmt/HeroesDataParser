﻿using CASCLib;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace HeroesData.Parser.XmlGameData
{
    public abstract class GameData
    {
        private readonly Dictionary<(string Catalog, string Entry, string Field), double> ScaleValueByLookupId = new Dictionary<(string Catalog, string Entry, string Field), double>();

        protected GameData(string modsFolderPath)
        {
            ModsFolderPath = modsFolderPath;
        }

        /// <summary>
        /// Gets the number of xml files that were to added <see cref="XmlGameData"/>.
        /// </summary>
        public int XmlFileCount { get; protected set; } = 0;

        /// <summary>
        /// Gets a XDocument of all the combined xml game data.
        /// </summary>
        public XDocument XmlGameData { get; protected set; } = new XDocument();

        protected string ModsFolderPath { get; }
        protected string CoreStormModFolderPath { get; set; }
        protected string HeroesdataStormModFolderPath { get; set; }
        protected string OldHeroesFolderPath { get; set; }
        protected string NewHeroesFolderPath { get; set; }

        /// <summary>
        /// Loads all the required game files.
        /// </summary>
        /// <param name="modsFolderPath">The file path of the mods folder.</param>
        /// <returns></returns>
        public static GameData Load(string modsFolderPath)
        {
            return new FileGameData(modsFolderPath);
        }

        /// <summary>
        /// Loads all the required game files.
        /// </summary>
        /// <param name="cascFolder"></param>
        /// <param name="modsFolderPath">The root folder of the heroes data.</param>
        /// <returns></returns>
        public static GameData Load(CASCHandler cascHandler, CASCFolder cascFolder, string modsFolderPath = "mods")
        {
            return new CASCGameData(cascHandler, cascFolder, modsFolderPath);
        }

        /// <summary>
        /// Gets the scale value of the given lookup id.
        /// </summary>
        /// <param name="lookupId">lookupId.</param>
        /// <returns></returns>
        public double? ScaleValue((string Catalog, string Entry, string Field) lookupId)
        {
            if (ScaleValueByLookupId.TryGetValue(lookupId, out double value))
                return value;
            else
                return null;
        }

        protected void Initialize()
        {
            CoreStormModFolderPath = Path.Combine(ModsFolderPath, @"core.stormmod\base.stormdata\GameData\");
            HeroesdataStormModFolderPath = Path.Combine(ModsFolderPath, @"heroesdata.stormmod\base.stormdata\GameData\");
            OldHeroesFolderPath = Path.Combine(HeroesdataStormModFolderPath, "Heroes");
            NewHeroesFolderPath = Path.Combine(ModsFolderPath, "heromods");

            LoadFiles();
            GetLevelScalingData();
        }

        protected abstract void LoadCoreStormMod();
        protected abstract void LoadHeroesDataStormMod();
        protected abstract void LoadOldHeroes();
        protected abstract void LoadNewHeroes();

        private void LoadFiles()
        {
            LoadCoreStormMod(); // must come first
            LoadHeroesDataStormMod();
            LoadOldHeroes();
            LoadNewHeroes();
        }

        private void GetLevelScalingData()
        {
            IEnumerable<XElement> levelScalingArrays = XmlGameData.Root.Descendants("LevelScalingArray");

            foreach (XElement scalingArray in levelScalingArrays)
            {
                foreach (XElement modification in scalingArray.Elements("Modifications"))
                {
                    string catalog = modification.Element("Catalog")?.Attribute("value")?.Value;
                    string entry = modification.Element("Entry")?.Attribute("value")?.Value;
                    string field = modification.Element("Field")?.Attribute("value")?.Value;
                    string value = modification.Element("Value")?.Attribute("value")?.Value;

                    if (string.IsNullOrEmpty(value))
                        continue;

                    if (ScaleValueByLookupId.ContainsKey((catalog, entry, field)))
                        ScaleValueByLookupId[(catalog, entry, field)] = double.Parse(value); // replace
                    else
                        ScaleValueByLookupId.Add((catalog, entry, field), double.Parse(value));
                }
            }
        }
    }
}