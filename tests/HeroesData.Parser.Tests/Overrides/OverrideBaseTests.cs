﻿using Heroes.Models;
using Heroes.Models.AbilityTalents;
using HeroesData.Parser.UnitData.Overrides;
using HeroesData.Parser.XmlGameData;
using System;
using System.Collections.Generic;
using System.IO;

namespace HeroesData.Parser.Tests.Overrides
{
    public abstract class OverrideBaseTests
    {
        private const string TestDataFolder = "TestData";
        private readonly string ModsTestFolder = Path.Combine(TestDataFolder, "mods");
        private readonly string HeroOverrideTestFolder = Path.Combine(TestDataFolder, "override", "HeroOverrideTest.xml");

        public OverrideBaseTests()
        {
            GameData gameData = GameData.Load(ModsTestFolder);
            OverrideData = OverrideData.Load(gameData, HeroOverrideTestFolder);

            HeroOverride = OverrideData.HeroOverride(CHeroId);
            LoadInitialValues();
        }

        protected abstract string CHeroId { get; }
        protected OverrideData OverrideData { get; }
        protected HeroOverride HeroOverride { get; }
        protected Ability TestAbility { get; } = new Ability();
        protected Talent TestTalent { get; } = new Talent();
        protected UnitWeapon TestWeapon { get; } = new UnitWeapon();
        protected HeroPortrait TestPortrait { get; } = new HeroPortrait();

        protected void LoadOverrideIntoTestAbility(string abilityName)
        {
            if (HeroOverride.PropertyAbilityOverrideMethodByAbilityId.TryGetValue(abilityName, out Dictionary<string, Action<Ability>> valueOverrideMethods))
            {
                foreach (var propertyOverride in valueOverrideMethods)
                {
                    // execute each property override
                    propertyOverride.Value(TestAbility);
                }
            }
        }

        protected void LoadOverrideIntoTestTalent(string talentName)
        {
            if (HeroOverride.PropertyTalentOverrideMethodByTalentId.TryGetValue(talentName, out Dictionary<string, Action<Talent>> valueOverrideMethods))
            {
                foreach (var propertyOverride in valueOverrideMethods)
                {
                    // execute each property override
                    propertyOverride.Value(TestTalent);
                }
            }
        }

        protected void LoadOverrideIntoTestWeapon(string weaponName)
        {
            if (HeroOverride.PropertyWeaponOverrideMethodByWeaponId.TryGetValue(weaponName, out Dictionary<string, Action<UnitWeapon>> valueOverrideMethods))
            {
                foreach (var propertyOverride in valueOverrideMethods)
                {
                    // execute each property override
                    propertyOverride.Value(TestWeapon);
                }
            }
        }

        protected void LoadOverrideIntoTestPortrait(string heroName)
        {
            if (HeroOverride.PropertyPortraitOverrideMethodByCHeroId.TryGetValue(heroName, out Dictionary<string, Action<HeroPortrait>> valueOverrideMethods))
            {
                foreach (var propertyOverride in valueOverrideMethods)
                {
                    // execute each property override
                    propertyOverride.Value(TestPortrait);
                }
            }
        }

        private void LoadInitialValues()
        {
            TestAbility.Tooltip.Life.LifeCost = 10;

            TestTalent.Tooltip.Energy.EnergyCost = 500;

            TestWeapon.Damage = 500;
            TestWeapon.Range = 5;
        }
    }
}
