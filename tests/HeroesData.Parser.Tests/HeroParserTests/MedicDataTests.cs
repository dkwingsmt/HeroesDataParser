﻿using Heroes.Models.AbilityTalents;
using Xunit;

namespace HeroesData.Parser.Tests.HeroParserTests
{
    public class MedicDataTests : HeroParserBaseTest
    {
        [Fact]
        public void AbilityEnergyTooltipTextTest()
        {
            Ability ability = HeroMedic.Abilities["MedicHealingBeam"];
            Assert.Equal("<s val=\"StandardTooltipDetails\">Energy: 6 per second</s>", ability.Tooltip.Energy?.EnergyText.RawDescription);
        }
    }
}
