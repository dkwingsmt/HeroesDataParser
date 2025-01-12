﻿using Heroes.Models.AbilityTalents;
using HeroesData.Parser.Overrides.DataOverrides;
using HeroesData.Parser.Overrides.PropertyOverrides;
using System.Xml.Linq;

namespace HeroesData.Parser.Overrides
{
    public class UnitOverrideLoader : OverrideLoaderBase<UnitDataOverride>, IOverrideLoader
    {
        public UnitOverrideLoader(int? hotsBuild)
            : base(hotsBuild)
        {
        }

        protected override string OverrideFileName => $"unit-{base.OverrideFileName}";

        protected override string OverrideElementName => "CUnit";

        protected override void SetOverride(XElement element)
        {
            UnitDataOverride unitDataOverride = new UnitDataOverride();

            AbilityPropertyOverride abilityOverride = new AbilityPropertyOverride();
            WeaponPropertyOverride weaponOverride = new WeaponPropertyOverride();

            string unitId = element.Attribute("id").Value;

            foreach (XElement dataElement in element.Elements())
            {
                string elementName = dataElement.Name.LocalName;
                string valueAttribute = dataElement.Attribute("value")?.Value;

                XElement overrideElement = null;

                switch (elementName)
                {
                    case "Name":
                        if (!string.IsNullOrEmpty(valueAttribute))
                            unitDataOverride.NameOverride = (true, valueAttribute);
                        break;
                    case "HyperlinkId":
                        if (!string.IsNullOrEmpty(valueAttribute))
                            unitDataOverride.HyperlinkIdOverride = (true, valueAttribute);
                        break;
                    case "CUnit":
                        if (!string.IsNullOrEmpty(valueAttribute))
                            unitDataOverride.CUnitOverride = (true, valueAttribute);
                        break;
                    case "EnergyType":
                        unitDataOverride.EnergyTypeOverride = (true, valueAttribute);
                        break;
                    case "Energy":
                        string energyValue = valueAttribute;
                        if (int.TryParse(energyValue, out int value))
                        {
                            if (value < 0)
                                value = 0;

                            unitDataOverride.EnergyOverride = (true, value);
                        }
                        else
                        {
                            unitDataOverride.EnergyOverride = (true, 0);
                        }

                        break;
                    case "ParentLink":
                        unitDataOverride.ParentLinkOverride = (true, valueAttribute);
                        break;
                    case "Ability":
                        string abilityId = dataElement.Attribute("id")?.Value ?? string.Empty;
                        string buttonAbilityId = dataElement.Attribute("button")?.Value ?? abilityId;
                        string passiveAbility = dataElement.Attribute("passive")?.Value;
                        string addedAbility = dataElement.Attribute("add")?.Value;

                        if (bool.TryParse(passiveAbility, out bool abilityPassiveResult))
                        {
                            buttonAbilityId = $"{buttonAbilityId}~Passive~";
                        }

                        if (bool.TryParse(addedAbility, out bool abilityAddedResult))
                        {
                            unitDataOverride.AddAddedAbility(new AbilityTalentId(abilityId, buttonAbilityId), abilityAddedResult);

                            if (!abilityAddedResult)
                                continue;
                        }

                        if (!string.IsNullOrEmpty(abilityId))
                        {
                            overrideElement = dataElement.Element("Override");

                            if (overrideElement != null)
                                abilityOverride.SetOverride(new AbilityTalentId(abilityId, buttonAbilityId), overrideElement, unitDataOverride.PropertyAbilityOverrideMethodByAbilityId);
                        }

                        break;
                    case "Weapon":
                        string weaponId = dataElement.Attribute("id")?.Value;
                        string addedWeapon = dataElement.Attribute("add")?.Value;

                        if (string.IsNullOrEmpty(weaponId))
                            continue;

                        if (bool.TryParse(addedWeapon, out bool weaponValidResult))
                        {
                            unitDataOverride.AddAddedWeapon(weaponId, weaponValidResult);

                            if (!weaponValidResult)
                                continue;
                        }

                        overrideElement = dataElement.Element("Override");
                        if (overrideElement != null)
                            weaponOverride.SetOverride(weaponId, overrideElement, unitDataOverride.PropertyWeaponOverrideMethodByWeaponId);
                        break;
                }
            }

            DataOverridesById[unitId] = unitDataOverride;
        }
    }
}
