﻿using Heroes.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroesData.Parser.Tests.HeroParserTests
{
    [TestClass]
    public class AbathurTests : HeroDataBaseTest
    {
        [TestMethod]
        public void BasicPropertiesTests()
        {
            Assert.AreEqual(0, HeroAbathur.Energy.EnergyMax);
            Assert.AreEqual(HeroFranchise.Starcraft, HeroAbathur.Franchise);
            Assert.AreEqual(HeroGender.Neutral, HeroAbathur.Gender);
            Assert.AreEqual("storm_ui_ingame_heroselect_btn_infestor.dds", HeroAbathur.HeroPortrait.HeroSelectPortraitFileName);
        }

        [TestMethod]
        public void HeroUnitTests()
        {
            Assert.AreEqual(1, HeroAbathur.HeroUnits.Count);

            Unit unit = HeroAbathur.HeroUnits[0];
            Assert.AreEqual("AbathurSymbiote", unit.CUnitId);
            Assert.AreEqual("AbathurSymbiote", unit.ShortName);
            Assert.AreEqual("Symbiote", unit.Name);
            Assert.AreEqual(0.0117, unit.Speed);
            Assert.AreEqual(4, unit.Sight);
        }
    }
}
