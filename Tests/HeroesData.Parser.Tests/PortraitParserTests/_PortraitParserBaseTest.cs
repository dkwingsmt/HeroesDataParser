﻿using Heroes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroesData.Parser.Tests.PortraitParserTests
{
    [TestClass]
    public class PortraitParserBaseTest : ParserBase
    {
        public PortraitParserBaseTest()
        {
            Parse();
        }

        protected Portrait StitchesPortraitSummer { get; set; }

        [TestMethod]
        public void GetItemsTest()
        {
            PortraitParser portraitParser = new PortraitParser(XmlDataService);
            Assert.IsTrue(portraitParser.Items.Count > 0);
        }

        private void Parse()
        {
            PortraitParser portraitParser = new PortraitParser(XmlDataService);
            StitchesPortraitSummer = portraitParser.Parse("StitchesPortraitSummer");
        }
    }
}
