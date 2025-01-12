﻿using Heroes.Models;
using Heroes.Models.AbilityTalents;
using HeroesData.Parser;
using HeroesData.Parser.GameStrings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeroesData.ExtractorData
{
    public class DataUnit : DataExtractorBase<Unit, UnitParser>, IData
    {
        public DataUnit(UnitParser parser)
            : base(parser)
        {
        }

        public override string Name => "units";

        protected override void Validation(Unit unit)
        {
            if (unit.Id.EndsWith("dummy", StringComparison.OrdinalIgnoreCase))
                return;

            if (unit.ContainsAttribute("Heroic"))
            {
                if (!unit.ContainsAttribute("ImmuneToAOE"))
                {
                    if (unit.Life.LifeMax <= 0)
                        AddWarning($"{nameof(unit.Life)} is 0");

                    if (unit.Sight <= 0)
                        AddWarning($"{nameof(unit.Sight)} is 0");

                    if (unit.Speed <= 0 && unit.DamageType != "Structure" && !unit.Attributes.Contains("Structure"))
                        AddWarning($"{nameof(unit.Speed)} is 0");

                    if (!unit.Attributes.Any())
                        AddWarning($"{nameof(unit.Attributes)} is 0");
                }

                VerifyAbilities(unit);
            }
            else
            {
                VerifyTooltipDescription(unit);
            }

            VerifyAbilitiesCount(unit.PrimaryAbilities().ToList());
            VerifyAbilitiesCount(unit.SubAbilities().ToList());

            VerifyWeapons(unit);
        }

        private void VerifyWeapons(Unit unit)
        {
            foreach (UnitWeapon weapon in unit.Weapons)
            {
                if (weapon.Damage > 0)
                {
                    if (weapon.Period <= 0)
                        AddWarning($"{weapon.WeaponNameId} {nameof(weapon.Period)} is 0");

                    if (weapon.Range <= 0)
                        AddWarning($"{weapon.WeaponNameId} {nameof(weapon.Range)} is 0");
                }
            }
        }

        private void VerifyAbilities(Unit unit)
        {
            foreach (Ability ability in unit.Abilities)
            {
                if (ability.Tooltip.ShortTooltip?.RawDescription == GameStringParser.FailedParsed)
                    AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.ShortTooltip)} failed to parse correctly");

                if (ability.Tooltip.FullTooltip?.RawDescription == GameStringParser.FailedParsed)
                    AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.FullTooltip)} failed to parse correctly");

                if (!string.IsNullOrEmpty(ability.Tooltip.Cooldown?.CooldownTooltip?.RawDescription))
                {
                    if (ability.Tooltip.Cooldown.CooldownTooltip?.RawDescription == GameStringParser.FailedParsed)
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Cooldown.CooldownTooltip)} failed to parse correctly");
                    else if (char.IsDigit(ability.Tooltip.Cooldown.CooldownTooltip.PlainText[0]))
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Cooldown.CooldownTooltip)} does not have a prefix");
                }

                if (!string.IsNullOrEmpty(ability.Tooltip.Energy?.EnergyTooltip?.RawDescription))
                {
                    if (ability.Tooltip.Energy.EnergyTooltip?.RawDescription == GameStringParser.FailedParsed)
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Energy.EnergyTooltip)} failed to parse correctly");
                    else if (char.IsDigit(ability.Tooltip.Energy.EnergyTooltip.PlainText[0]))
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Energy.EnergyTooltip)} does not have a prefix");
                }

                if (!string.IsNullOrEmpty(ability.Tooltip.Life?.LifeCostTooltip?.RawDescription))
                {
                    if (ability.Tooltip.Life.LifeCostTooltip?.RawDescription == GameStringParser.FailedParsed)
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Life.LifeCostTooltip)} failed to parse correctly");
                    else if (char.IsDigit(ability.Tooltip.Life.LifeCostTooltip.PlainText[0]))
                        AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.Life.LifeCostTooltip)} does not have a prefix");
                }
            }
        }

        private void VerifyAbilitiesCount(List<Ability> abilitiesList)
        {
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Q).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Q} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.W).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.W} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.E).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.E} abilities");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Heroic).Count() > 2)
                AddWarning($"has more than 2 {AbilityType.Heroic} abilities");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Z).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Z} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.B).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.B} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Trait).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Trait} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Taunt).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Taunt} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Spray).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Spray} ability");
            if (abilitiesList.Where(x => x.AbilityType == AbilityType.Dance).Count() > 1)
                AddWarning($"has more than 1 {AbilityType.Dance} ability");
        }

        private void VerifyTooltipDescription(Unit unit)
        {
            foreach (Ability ability in unit.Abilities)
            {
                if (ability.Tooltip.ShortTooltip?.RawDescription == GameStringParser.FailedParsed)
                    AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.ShortTooltip)} failed to parse correctly");

                if (ability.Tooltip.FullTooltip?.RawDescription == GameStringParser.FailedParsed)
                    AddWarning($"[{ability.AbilityTalentId.Id}] {nameof(ability.Tooltip.FullTooltip)} failed to parse correctly");
            }
        }
    }
}
