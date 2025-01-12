﻿using Heroes.Models;
using System;

namespace HeroesData.FileWriter.Tests.HeroSkinData
{
    public class HeroSkinDataOutputBase : FileOutputTestBase<HeroSkin>
    {
        public HeroSkinDataOutputBase()
            : base(nameof(HeroSkinData))
        {
        }

        protected override void SetTestData()
        {
            HeroSkin heroSkin = new HeroSkin()
            {
                Name = "Bone Abathur",
                Id = "AbathurBone",
                HyperlinkId = "AbathurBone",
                SortName = "xxAbathurBone",
                Rarity = Rarity.None,
                AttributeId = "Aba1",
                Description = new TooltipDescription("Evolution Master of Kerrigan's Swarm"),
                SearchText = "White Pink",
                ReleaseDate = new DateTime(2014, 3, 13),
            };

            heroSkin.AddFeature("ThemedAbilities");
            heroSkin.AddFeature("ThemedAbilities");
            heroSkin.AddFeature("ThemedAnimations");

            TestData.Add(heroSkin);

            HeroSkin heroSkin2 = new HeroSkin()
            {
                Name = "Mecha Abathur",
                Id = "AbathurMechaVar1",
                HyperlinkId = "AbathurMecha",
                SortName = "xxyAbathurMecha",
                Rarity = Rarity.Legendary,
                AttributeId = "Aba2",
                ReleaseDate = new DateTime(2014, 3, 1),
            };

            TestData.Add(heroSkin2);
        }
    }
}
