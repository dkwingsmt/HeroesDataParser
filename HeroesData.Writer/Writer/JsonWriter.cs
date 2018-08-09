﻿using Heroes.Models;
using Heroes.Models.AbilityTalents;
using Heroes.Models.AbilityTalents.Tooltip;
using HeroesData.FileWriter.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeroesData.FileWriter.Writer
{
    internal class JsonWriter : Writer<JProperty, JObject>
    {
        public JsonWriter()
        { }

        protected override void SetSingleFileName()
        {
            if (HotsBuild.HasValue)
            {
                SingleFileName = $"heroesdata_{HotsBuild.Value}.json";
                SingleFileNameNoIndentation = $"heroesdata_{HotsBuild.Value}.min.json";
            }
            else
            {
                SingleFileName = "heroesdata.json";
                SingleFileNameNoIndentation = $"heroesdata.min.json";
            }
        }

        protected override void CreateSingleFile(List<Hero> heroes)
        {
            JObject jObject = new JObject(heroes.Select(hero => HeroElement(hero)));

            // has formatting
            using (StreamWriter file = File.CreateText(Path.Combine(JsonOutputFolder, SingleFileName)))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.Indented;
                jObject.WriteTo(writer);
            }

            // no formatting
            using (StreamWriter file = File.CreateText(Path.Combine(JsonOutputFolder, SingleFileNameNoIndentation)))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                writer.Formatting = Formatting.None;
                jObject.WriteTo(writer);
            }
        }

        protected override void CreateMultipleFiles(List<Hero> heroes)
        {
            foreach (Hero hero in heroes)
            {
                JObject jObject = new JObject(HeroElement(hero));

                // has formatting
                using (StreamWriter file = File.CreateText(Path.Combine(JsonOutputFolder, $"{hero.ShortName}.json")))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    jObject.WriteTo(writer);
                }

                // no formatting
                using (StreamWriter file = File.CreateText(Path.Combine(SplitFileJsonNoIndentationFolder, $"{hero.ShortName}.min.json")))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.None;
                    jObject.WriteTo(writer);
                }
            }
        }

        protected override JProperty HeroElement(Hero hero)
        {
            JObject heroObject = new JObject
            {
                { "name", hero.Name },
            };

            if (!string.IsNullOrEmpty(hero.CHeroId))
                heroObject.Add("cHeroId", hero.CHeroId);
            if (!string.IsNullOrEmpty(hero.CUnitId))
                heroObject.Add("cUnitId", hero.CUnitId);
            if (!string.IsNullOrEmpty(hero.AttributeId))
                heroObject.Add("attributeId", hero.AttributeId);

            heroObject.Add("difficulty", hero.Difficulty.ToString());
            heroObject.Add("franchise", hero.Franchise.ToString());

            if (hero.Gender.HasValue)
                heroObject.Add("gender", hero.Gender.Value.ToString());
            if (hero.InnerRadius > 0)
                heroObject.Add("innerRadius", hero.InnerRadius);
            if (hero.Radius > 0)
                heroObject.Add("radius", hero.Radius);
            if (hero.ReleaseDate.HasValue)
                heroObject.Add("releaseDate", hero.ReleaseDate.Value.ToString("yyyy-MM-dd"));
            if (hero.Sight > 0)
                heroObject.Add("sight", hero.Sight);
            if (hero.Speed > 0)
                heroObject.Add("speed", hero.Speed);
            if (hero.Type.HasValue)
                heroObject.Add("type", hero.Type.Value.ToString());
            if (hero.Rarity.HasValue)
                heroObject.Add("rarity", hero.Rarity.Value.ToString());
            if (!string.IsNullOrEmpty(hero.Description?.RawDescription))
                heroObject.Add("description", GetTooltip(hero.Description, FileSettings.Description));

            JProperty portraits = HeroPortraits(hero);
            if (portraits != null)
                heroObject.Add(portraits);

            JProperty life = UnitLife(hero);
            if (life != null)
                heroObject.Add(life);

            JProperty energy = UnitEnergy(hero);
            if (energy != null)
                heroObject.Add(energy);

            JProperty armor = UnitArmor(hero);
            if (armor != null)
                heroObject.Add(armor);

            if (hero.Roles?.Count > 0)
                heroObject.Add(new JProperty("roles", new JArray(from r in hero.Roles select new JValue(r.ToString()))));

            JProperty ratings = HeroRatings(hero);
            if (ratings != null)
                heroObject.Add(ratings);

            JProperty weapons = UnitWeapons(hero);
            if (weapons != null)
                heroObject.Add(weapons);

            JProperty abilities = UnitAbilities(hero, false);
            if (abilities != null)
                heroObject.Add(abilities);

            JProperty subAbilities = UnitSubAbilities(hero);
            if (subAbilities != null)
                heroObject.Add(subAbilities);

            JProperty talents = HeroTalents(hero);
            if (talents != null)
                heroObject.Add(talents);

            JProperty units = Units(hero);
            if (units != null)
                heroObject.Add(units);

            return new JProperty(hero.ShortName, heroObject);
        }

        protected override JProperty UnitElement(Unit unit)
        {
            JObject heroObject = new JObject
            {
                { "name", unit.Name },
            };

            if (!string.IsNullOrEmpty(unit.CUnitId))
                heroObject.Add("cUnitId", unit.CUnitId);
            if (unit.InnerRadius > 0)
                heroObject.Add("innerRadius", unit.InnerRadius);
            if (unit.Radius > 0)
                heroObject.Add("radius", unit.Radius);
            if (unit.Sight > 0)
                heroObject.Add("sight", unit.Sight);
            if (unit.Speed > 0)
                heroObject.Add("speed", unit.Speed);
            if (unit.Type.HasValue)
                heroObject.Add("type", unit.Type.Value.ToString());
            if (!string.IsNullOrEmpty(unit.Description?.RawDescription))
                heroObject.Add("description", GetTooltip(unit.Description, FileSettings.Description));

            JProperty life = UnitLife(unit);
            if (life != null)
                heroObject.Add(life);

            JProperty armor = UnitArmor(unit);
            if (armor != null)
                heroObject.Add(armor);

            JProperty energy = UnitEnergy(unit);
            if (energy != null)
                heroObject.Add(energy);

            JProperty weapons = UnitWeapons(unit);
            if (weapons != null)
                heroObject.Add(weapons);

            JProperty abilities = UnitAbilities(unit, true);
            if (abilities != null)
                heroObject.Add(abilities);

            return new JProperty(unit.ShortName, heroObject);
        }

        protected override JProperty GetArmorObject(Unit unit)
        {
            return new JProperty(
                "armor",
                new JObject(
                    new JProperty("Physical", unit.Armor.PhysicalArmor),
                    new JProperty("Spell", unit.Armor.SpellArmor)));
        }

        protected override JProperty GetLifeObject(Unit unit)
        {
            return new JProperty(
                "life",
                new JObject(
                    new JProperty("lifeAmount", unit.Life.LifeMax),
                    new JProperty("lifeScale", unit.Life.LifeScaling),
                    new JProperty("lifeRegenRate", unit.Life.LifeRegenerationRate),
                    new JProperty("regenScale", unit.Life.LifeRegenerationRateScaling)));
        }

        protected override JProperty GetEnergyObject(Unit unit)
        {
            return new JProperty(
                "energy",
                new JObject(
                    new JProperty("energyAmount", unit.Energy.EnergyMax),
                    new JProperty("type", unit.Energy.EnergyType.ToString()),
                    new JProperty("energyRegenRate", unit.Energy.EnergyRegenerationRate)));
        }

        protected override JProperty GetAbilitiesObject(Unit unit, bool isSubAbilities)
        {
            JObject abilityObject = new JObject();

            if (isSubAbilities)
            {
                ICollection<Ability> basicAbilities = unit.SubAbilities(AbilityTier.Basic);
                if (basicAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "basic",
                        new JArray(
                            from abil in basicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> heroicAbilities = unit.SubAbilities(AbilityTier.Heroic);
                if (heroicAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "heroic",
                        new JArray(
                            from abil in heroicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> traitAbilities = unit.SubAbilities(AbilityTier.Trait);
                if (traitAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "trait",
                        new JArray(
                            from abil in traitAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> mountAbilities = unit.SubAbilities(AbilityTier.Mount);
                if (mountAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "mount",
                        new JArray(
                            from abil in mountAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> activableAbilities = unit.SubAbilities(AbilityTier.Activable);
                if (activableAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "activable",
                        new JArray(
                            from abil in activableAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }
            }
            else
            {
                ICollection<Ability> basicAbilities = unit.PrimaryAbilities(AbilityTier.Basic);
                if (basicAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "basic",
                        new JArray(
                            from abil in basicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> heroicAbilities = unit.PrimaryAbilities(AbilityTier.Heroic);
                if (heroicAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "heroic",
                        new JArray(
                            from abil in heroicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> traitAbilities = unit.PrimaryAbilities(AbilityTier.Trait);
                if (traitAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "trait",
                        new JArray(
                            from abil in traitAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> mountAbilities = unit.PrimaryAbilities(AbilityTier.Mount);
                if (mountAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "mount",
                        new JArray(
                            from abil in mountAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                ICollection<Ability> activableAbilities = unit.PrimaryAbilities(AbilityTier.Activable);
                if (activableAbilities?.Count > 0)
                {
                    abilityObject.Add(new JProperty(
                        "activable",
                        new JArray(
                            from abil in activableAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }
            }

            return new JProperty("abilities", abilityObject);
        }

        protected override JProperty GetAbilityChargesObject(TooltipCharges tooltipCharges)
        {
            JObject charges = new JObject
            {
                { "countMax", tooltipCharges.CountMax },
            };

            if (tooltipCharges.CountUse.HasValue)
                charges.Add("countUse", tooltipCharges.CountUse.Value);

            if (tooltipCharges.CountStart.HasValue)
                charges.Add("countStart", tooltipCharges.CountStart.Value);

            if (tooltipCharges.IsHideCount.HasValue)
                charges.Add("hideCount", tooltipCharges.IsHideCount.Value);

            if (tooltipCharges.RecastCoodown.HasValue)
                charges.Add("recastCooldown", tooltipCharges.RecastCoodown.Value);

            return new JProperty("charges", charges);
        }

        protected override JProperty GetAbilityCooldownObject(TooltipCooldown tooltipCooldown)
        {
            return new JProperty("cooldownTooltip", GetTooltip(tooltipCooldown.CooldownText, FileSettings.Description));
        }

        protected override JProperty GetAbilityEnergyCostObject(TooltipEnergy tooltipEnergy)
        {
            return new JProperty("energyTooltip", GetTooltip(tooltipEnergy.EnergyText, FileSettings.Description));
        }

        protected override JProperty GetAbilityLifeCostObject(TooltipLife tooltipLife)
        {
            return new JProperty("lifeTooltip", GetTooltip(tooltipLife.LifeCostText, FileSettings.Description));
        }

        protected override JObject AbilityTalentInfoElement(AbilityTalentBase abilityTalentBase)
        {
            JObject info = new JObject
            {
                { "nameId", abilityTalentBase.ReferenceNameId },
                { "name", abilityTalentBase.Name },
            };

            if (!string.IsNullOrEmpty(abilityTalentBase.ShortTooltipNameId))
                info.Add("shortTooltipId", abilityTalentBase.ShortTooltipNameId);

            if (!string.IsNullOrEmpty(abilityTalentBase.FullTooltipNameId))
                info.Add("fullTooltipId", abilityTalentBase.FullTooltipNameId);

            info.Add("icon", Path.ChangeExtension(abilityTalentBase.IconFileName, FileSettings.ImageExtension));

            JProperty life = UnitAbilityLifeCost(abilityTalentBase.Tooltip.Life);
            if (life != null)
                info.Add(life);

            JProperty energy = UnitAbilityEnergyCost(abilityTalentBase.Tooltip.Energy);
            if (energy != null)
                info.Add(energy);

            JProperty charges = UnitAbilityCharges(abilityTalentBase.Tooltip.Charges);
            if (charges != null)
                info.Add(charges);

            JProperty cooldown = UnitAbilityCooldown(abilityTalentBase.Tooltip.Cooldown);
            if (cooldown != null)
                info.Add(cooldown);

            if (!string.IsNullOrEmpty(abilityTalentBase.Tooltip.ShortTooltip?.RawDescription))
                info.Add("shortTooltip", GetTooltip(abilityTalentBase.Tooltip.ShortTooltip, FileSettings.ShortTooltip));

            if (!string.IsNullOrEmpty(abilityTalentBase.Tooltip.FullTooltip?.RawDescription))
                info.Add("fullTooltip", GetTooltip(abilityTalentBase.Tooltip.FullTooltip, FileSettings.FullTooltip));

            info.Add("abilityType", abilityTalentBase.AbilityType.ToString());

            if (abilityTalentBase.IsActive)
                info.Add("isActive", abilityTalentBase.IsActive);

            if (abilityTalentBase.IsQuest)
                info.Add("isQuest", abilityTalentBase.IsQuest);

            return info;
        }

        protected override JProperty GetSubAbilitiesObject(ILookup<string, Ability> linkedAbilities)
        {
            JObject parentLink = null;

            IEnumerable<string> parentLinks = linkedAbilities.Select(x => x.Key).ToList();
            foreach (string parent in parentLinks)
            {
                JObject abilities = new JObject();

                IEnumerable<Ability> basicAbilities = linkedAbilities[parent].Where(x => x.Tier == AbilityTier.Basic);
                if (basicAbilities.Count() > 0)
                {
                    abilities.Add(new JProperty(
                        "basic",
                        new JArray(
                            from abil in basicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                IEnumerable<Ability> heroicAbilities = linkedAbilities[parent].Where(x => x.Tier == AbilityTier.Heroic);
                if (heroicAbilities.Count() > 0)
                {
                    abilities.Add(new JProperty(
                        "heroic",
                        new JArray(
                            from abil in heroicAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                IEnumerable<Ability> traitAbilities = linkedAbilities[parent].Where(x => x.Tier == AbilityTier.Trait);
                if (traitAbilities.Count() > 0)
                {
                    abilities.Add(new JProperty(
                        "trait",
                        new JArray(
                            from abil in traitAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                IEnumerable<Ability> mountAbilities = linkedAbilities[parent].Where(x => x.Tier == AbilityTier.Mount);
                if (mountAbilities.Count() > 0)
                {
                    abilities.Add(new JProperty(
                        "mount",
                        new JArray(
                            from abil in mountAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                IEnumerable<Ability> activableAbilities = linkedAbilities[parent].Where(x => x.Tier == AbilityTier.Activable);
                if (activableAbilities.Count() > 0)
                {
                    abilities.Add(new JProperty(
                        "activable",
                        new JArray(
                            from abil in activableAbilities
                            select new JObject(AbilityTalentInfoElement(abil)))));
                }

                if (parentLink == null)
                    parentLink = new JObject();
                parentLink.Add(new JProperty(parent, abilities));
            }

            return new JProperty("subAbilities", new JArray(new JObject(parentLink)));
        }

        protected override JProperty GetUnitsObject(Hero hero)
        {
            return new JProperty(
                HeroUnits,
                new JArray(
                    from heroUnit in hero.HeroUnits
                    select new JObject(UnitElement(heroUnit))));
        }

        protected override JProperty GetRatingsObject(Hero hero)
        {
            return new JProperty(
                "ratings",
                new JObject(
                    new JProperty("complexity", hero.Ratings.Complexity),
                    new JProperty("damage", hero.Ratings.Damage),
                    new JProperty("survivability", hero.Ratings.Survivability),
                    new JProperty("utility", hero.Ratings.Utility)));
        }

        protected override JObject TalentInfoElement(Talent talent)
        {
            JObject jObject = AbilityTalentInfoElement(talent);
            jObject.Add(new JProperty("sort", talent.Column));

            return jObject;
        }

        protected override JProperty GetTalentsObject(Hero hero)
        {
            JObject talantObject = new JObject();

            ICollection<Talent> level1Talents = hero.TierTalents(TalentTier.Level1);
            if (level1Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level1",
                    new JArray(
                        from talent in level1Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level4Talents = hero.TierTalents(TalentTier.Level4);
            if (level4Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level4",
                    new JArray(
                        from talent in level4Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level7Talents = hero.TierTalents(TalentTier.Level7);
            if (level7Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level7",
                    new JArray(
                        from talent in level7Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level10Talents = hero.TierTalents(TalentTier.Level10);
            if (level10Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level10",
                    new JArray(
                        from talent in level10Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level13Talents = hero.TierTalents(TalentTier.Level13);
            if (level13Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level13",
                    new JArray(
                        from talent in level13Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level16Talents = hero.TierTalents(TalentTier.Level16);
            if (level13Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level16",
                    new JArray(
                        from talent in level16Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            ICollection<Talent> level20Talents = hero.TierTalents(TalentTier.Level20);
            if (level20Talents?.Count > 0)
            {
                talantObject.Add(new JProperty(
                    "level20",
                    new JArray(
                        from talent in level20Talents
                        select new JObject(TalentInfoElement(talent)))));
            }

            return new JProperty("talents", talantObject);
        }

        protected override JProperty GetWeaponsObject(Unit unit)
        {
            return new JProperty(
                "weapons",
                new JArray(
                    from w in unit.Weapons
                    select new JObject(
                        new JProperty("nameId", w.WeaponNameId),
                        new JProperty("range", w.Range),
                        new JProperty("period", w.Period),
                        new JProperty("damage", w.Damage),
                        new JProperty("damageScale", w.DamageScaling))));
        }

        protected override JProperty GetPortraitObject(Hero hero)
        {
            JObject portrait = new JObject();

            if (FileSettings.HeroSelectPortrait && !string.IsNullOrEmpty(hero.HeroPortrait.HeroSelectPortraitFileName))
                portrait.Add("heroSelect", Path.ChangeExtension(hero.HeroPortrait.HeroSelectPortraitFileName, FileSettings.ImageExtension));
            if (FileSettings.LeaderboardPortrait && !string.IsNullOrEmpty(hero.HeroPortrait.LeaderboardPortraitFileName))
                portrait.Add("leaderboard", Path.ChangeExtension(hero.HeroPortrait.LeaderboardPortraitFileName, FileSettings.ImageExtension));
            if (FileSettings.LoadingPortraitPortrait && !string.IsNullOrEmpty(hero.HeroPortrait.LoadingScreenPortraitFileName))
                portrait.Add("loading", Path.ChangeExtension(hero.HeroPortrait.LoadingScreenPortraitFileName, FileSettings.ImageExtension));
            if (FileSettings.PartyPanelPortrait && !string.IsNullOrEmpty(hero.HeroPortrait.PartyPanelPortraitFileName))
                portrait.Add("partyPanel", Path.ChangeExtension(hero.HeroPortrait.PartyPanelPortraitFileName, FileSettings.ImageExtension));
            if (FileSettings.TargetPortrait && !string.IsNullOrEmpty(hero.HeroPortrait.TargetPortraitFileName))
                portrait.Add("target", Path.ChangeExtension(hero.HeroPortrait.TargetPortraitFileName, FileSettings.ImageExtension));

            return new JProperty("portraits", portrait);
        }
    }
}
