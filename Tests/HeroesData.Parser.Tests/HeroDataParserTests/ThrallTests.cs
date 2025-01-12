﻿using Heroes.Models.AbilityTalents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeroesData.Parser.Tests.HeroDataParserTests
{
    [TestClass]
    public class ThrallTests : HeroDataParserBaseTest
    {
        [TestMethod]
        public void AbilityTalentLinkIdsTests()
        {
            Talent talent = HeroThrall.GetTalent("ThrallMasteryManaTide");
            Assert.IsTrue(talent.AbilityTalentLinkIdsCount == 2);
            Assert.IsTrue(talent.ContainsAbilityTalentLinkId("ThrallFrostwolfResilience"));
            Assert.AreEqual(AbilityType.Trait, talent.AbilityType);

            talent = HeroThrall.GetTalent("ThrallMasteryFrostwolfsGrace");
            Assert.IsTrue(talent.AbilityTalentLinkIdsCount == 2);
            Assert.IsTrue(talent.ContainsAbilityTalentLinkId("ThrallFrostwolfResilience"));
            Assert.AreEqual(AbilityType.Trait, talent.AbilityType);
        }

        [TestMethod]
        public void AbilitiesListTests()
        {
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallSundering"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallEarthquake"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallChainLightning"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallFeralSpirit"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallWindfury"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallFrostwolfResilience"));

            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallCancelSundering"));
            Assert.IsTrue(HeroThrall.ContainsAbility("ThrallFrostwolfsGrace"));
        }

        [TestMethod]
        public void ThrallMasteryGraceOfAirTalentAbilityTypeTest()
        {
            Talent talent = HeroThrall.GetTalent("ThrallMasteryGraceOfAir");
            Assert.AreEqual(AbilityType.E, talent.AbilityType);
        }
    }
}
