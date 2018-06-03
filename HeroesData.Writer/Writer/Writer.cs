﻿using HeroesData.FileWriter.Settings;
using HeroesData.Parser.Models;
using HeroesData.Parser.Models.AbilityTalents;
using HeroesData.Parser.Models.AbilityTalents.Tooltip;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeroesData.FileWriter.Writer
{
    internal abstract class Writer<T, TU>
        where T : class
        where TU : class
    {
        protected Writer()
        {
            Directory.CreateDirectory(XmlOutputFolder);
            Directory.CreateDirectory(JsonOutputFolder);
        }

        protected FileSettings FileSettings { get; set; }
        protected string XmlOutputFolder => Path.Combine("output", "xml");
        protected string JsonOutputFolder => Path.Combine("output", "json");
        protected string RootNode => "Heroes";
        protected string HeroUnits => "HeroUnits";

        protected string StripInvalidChars(string text)
        {
            return new string(text.Where(c => !char.IsPunctuation(c)).ToArray());
        }

        protected abstract void CreateMultipleFiles(List<Hero> heroes);
        protected abstract void CreateSingleFile(List<Hero> heroes);
        protected abstract T HeroElement(Hero hero);
        protected abstract T UnitElement(Unit unit);
        protected abstract T GetLifeObject(Unit unit);
        protected abstract T GetEnergyObject(Unit unit);
        protected abstract T GetRatingsObject(Hero hero);
        protected abstract T GetWeaponsObject(Unit unit);
        protected abstract T GetAbilitiesObject(Unit unit, bool isUnitAbilities);
        protected abstract T GetSubAbilitiesObject(ILookup<string, Ability> linkedAbilities);
        protected abstract T GetTalentsObject(Hero hero);
        protected abstract T GetUnitsObject(Hero hero);
        protected abstract TU AbilityTalentInfoElement(AbilityTalentBase abilityTalentBase);
        protected abstract TU TalentInfoElement(Talent talent);
        protected abstract T GetAbilityLifeCostObject(TooltipLife tooltipLife);
        protected abstract T GetAbilityEnergyCostObject(TooltipEnergy tooltipEnergy);
        protected abstract T GetAbilityCooldownObject(TooltipCooldown tooltipCooldown);
        protected abstract T GetAbilityChargesObject(TooltipCharges tooltipCharges);

        protected T UnitLife(Unit unit)
        {
            if (unit.Life.LifeMax > 0)
            {
                return GetLifeObject(unit);
            }

            return null;
        }

        protected T UnitEnergy(Unit unit)
        {
            if (unit.Energy.EnergyMax > 0)
            {
                return GetEnergyObject(unit);
            }

            return null;
        }

        protected T HeroRatings(Hero hero)
        {
            if (hero.Ratings != null)
            {
                return GetRatingsObject(hero);
            }

            return null;
        }

        protected T UnitWeapons(Unit unit)
        {
            if (FileSettings.IncludeWeapons && unit.Weapons?.Count > 0)
            {
                return GetWeaponsObject(unit);
            }

            return null;
        }

        protected T UnitAbilities(Unit unit, bool isSubAbilities)
        {
            if (FileSettings.IncludeAbilities && unit.Abilities?.Count > 0)
            {
                return GetAbilitiesObject(unit, isSubAbilities);
            }

            return null;
        }

        protected T UnitSubAbilities(Unit unit)
        {
            if (FileSettings.IncludeSubAbilities && unit.Abilities?.Count > 0)
            {
                ILookup<string, Ability> linkedAbilities = unit.ParentLinkedAbilities();
                if (linkedAbilities.Count > 0)
                {
                    return GetSubAbilitiesObject(linkedAbilities);
                }
            }

            return null;
        }

        protected T UnitAbilityLifeCost(TooltipLife tooltipLife)
        {
            if (tooltipLife.LifeCost.HasValue)
            {
                return GetAbilityLifeCostObject(tooltipLife);
            }

            return null;
        }

        protected T UnitAbilityEnergyCost(TooltipEnergy tooltipEnergy)
        {
            if (tooltipEnergy.EnergyCost.HasValue)
            {
                return GetAbilityEnergyCostObject(tooltipEnergy);
            }

            return null;
        }

        protected T UnitAbilityCooldown(TooltipCooldown tooltipCooldown)
        {
            if (tooltipCooldown.CooldownValue.HasValue)
            {
                return GetAbilityCooldownObject(tooltipCooldown);
            }

            return null;
        }

        protected T UnitAbilityCharges(TooltipCharges tooltipCharges)
        {
            if (tooltipCharges.HasCharges)
            {
                return GetAbilityChargesObject(tooltipCharges);
            }

            return null;
        }

        protected T HeroTalents(Hero hero)
        {
            if (FileSettings.IncludeTalents && hero.Talents?.Count > 0)
            {
                return GetTalentsObject(hero);
            }

            return null;
        }

        protected T Units(Hero hero)
        {
            if (FileSettings.IncludeHeroUnits && hero.HeroUnits?.Count > 0)
            {
                return GetUnitsObject(hero);
            }

            return null;
        }

        protected string GetTooltip(TooltipDescription tooltipDescription, int setting)
        {
            if (setting == 0)
                return tooltipDescription.RawDescription;
            else if (setting == 1)
                return tooltipDescription.PlainText;
            else if (setting == 2)
                return tooltipDescription.PlainTextWithNewlines;
            else if (setting == 3)
                return tooltipDescription.PlainTextWithScaling;
            else if (setting == 4)
                return tooltipDescription.PlainTextWithScalingWithNewlines;
            else if (setting == 6)
                return tooltipDescription.ColoredTextWithScaling;
            else
                return tooltipDescription.ColoredText;
        }
    }
}