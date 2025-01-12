﻿using Heroes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroesData.Parser.Tests.EmoticonParserTests
{
    [TestClass]
    public class EmoticonParserBaseTest : ParserBase
    {
        public EmoticonParserBaseTest()
        {
            Parse();
        }

        protected Emoticon LunaraAngry { get; set; }

        [TestMethod]
        public void GetItemsTest()
        {
            EmoticonParser emoticonParser = new EmoticonParser(XmlDataService);
            Assert.IsTrue(emoticonParser.Items.Count > 0);
        }

        private void Parse()
        {
            EmoticonParser emoticonParser = new EmoticonParser(XmlDataService);
            LunaraAngry = emoticonParser.Parse("lunara_angry");
        }
    }
}
