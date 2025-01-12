﻿using HeroesData.Loader.XmlGameData;
using HeroesData.Parser.GameStrings;
using HeroesData.Parser.XmlData;
using System.Globalization;
using System.IO;

namespace HeroesData.Parser.Tests
{
    public class ParserBase
    {
        private const string TestDataFolder = "TestData";
        private readonly string ModsTestFolder = Path.Combine(TestDataFolder, "mods");

        public ParserBase()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            LoadTestData();
        }

        protected GameData GameData { get; set; }
        protected DefaultData DefaultData { get; set; }
        protected GameStringParser GameStringParser { get; set; }
        protected Configuration Configuration { get; set; }
        protected IXmlDataService XmlDataService { get; set; }

        private void LoadTestData()
        {
            GameData = new FileGameData(ModsTestFolder);
            GameData.LoadAllData();

            DefaultData = new DefaultData(GameData);
            DefaultData.Load();

            Configuration = new Configuration();
            Configuration.Load();

            GameStringParser = new GameStringParser(Configuration, GameData);
            ParseGameStrings();

            XmlDataService = new XmlDataService(Configuration, GameData, DefaultData);
        }

        private void ParseGameStrings()
        {
            foreach (string id in GameData.GameStringIds)
            {
                if (GameStringParser.TryParseRawTooltip(id, GameData.GetGameString(id), out string parsedGamestring))
                    GameData.AddGameString(id, parsedGamestring);
            }
        }
    }
}
