﻿using Heroes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroesData.Parser.Tests.AnnouncerParserTests
{
    [TestClass]
    public class AnnouncerParserBaseTest : ParserBase
    {
        public AnnouncerParserBaseTest()
        {
            Parse();
        }

        protected Announcer AdjutantAnnouncer { get; set; }
        protected Announcer OrpheaAnnouncer { get; set; }
        protected Announcer JunkratAnnouncer { get; set; }

        [TestMethod]
        public void GetItemsTest()
        {
            AnnouncerParser announcerParser = new AnnouncerParser(XmlDataService);
            Assert.IsTrue(announcerParser.Items.Count > 0);
        }

        private void Parse()
        {
            AnnouncerParser announcerParser = new AnnouncerParser(XmlDataService);
            AdjutantAnnouncer = announcerParser.Parse("Adjutant");
            OrpheaAnnouncer = announcerParser.Parse("OrpheaA");
            JunkratAnnouncer = announcerParser.Parse("JunkratA");
        }
    }
}
