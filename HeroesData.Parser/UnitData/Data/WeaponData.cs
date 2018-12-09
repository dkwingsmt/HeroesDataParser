﻿using Heroes.Models;
using HeroesData.Loader.XmlGameData;
using HeroesData.Parser.UnitData.Overrides;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HeroesData.Parser.UnitData.Data
{
    public class WeaponData
    {
        private readonly GameData GameData;
        private readonly DefaultData DefaultData;
        private readonly HeroOverride HeroOverride;

        public WeaponData(GameData gameData, DefaultData defaultData, HeroOverride heroOverride)
        {
            GameData = gameData;
            DefaultData = defaultData;
            HeroOverride = heroOverride;
        }

        /// <summary>
        /// Sets the hero weapon data.
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="weaponElements"></param>
        public void SetHeroWeapons(Hero hero, IEnumerable<XElement> weaponElements)
        {
            List<string> weaponsIds = new List<string>();
            foreach (XElement weaponElement in weaponElements)
            {
                string weaponNameId = weaponElement.Attribute("Link")?.Value;
                if (!string.IsNullOrEmpty(weaponNameId))
                    weaponsIds.Add(weaponNameId);
            }

            foreach (string weaponNameId in weaponsIds)
            {
                if (HeroOverride.IsValidWeaponByWeaponId.TryGetValue(weaponNameId, out bool validWeapon))
                {
                    if (!validWeapon)
                        continue;
                }
                else
                {
                    validWeapon = false;
                }

                if (validWeapon || weaponsIds.Count == 1 || weaponNameId.Contains("HeroWeapon") || weaponNameId == hero.CUnitId || weaponNameId == $"{hero.CHeroId}Weapon" || weaponNameId == $"{hero.CHeroId}WeaponMelee")
                {
                    // defaults
                    UnitWeapon weapon = new UnitWeapon
                    {
                        WeaponNameId = weaponNameId,
                        Name = GameData.GetGameString(DefaultData.WeaponName.Replace(DefaultData.IdReplacer, weaponNameId)),
                        Period = DefaultData.WeaponPeriod,
                        Range = DefaultData.WeaponRange,
                    };

                    WeaponAddDamage(weapon, DefaultData.WeaponDisplayEffect.Replace(DefaultData.IdReplacer, weapon.WeaponNameId));

                    XElement weaponLegacy = GameData.XmlGameData.Root.Elements("CWeaponLegacy").FirstOrDefault(x => x.Attribute("id")?.Value == weaponNameId);
                    if (weaponLegacy != null)
                        weapon = SetWeaponData(weaponLegacy, weapon);

                    hero.Weapons.Add(weapon);
                }
            }
        }

        private UnitWeapon SetWeaponData(XElement weaponElement, UnitWeapon weapon)
        {
            // parent lookup
            string parentValue = weaponElement.Attribute("parent")?.Value;
            if (!string.IsNullOrEmpty(parentValue))
            {
                XElement parentElement = GameData.XmlGameData.Root.Elements("CWeaponLegacy").FirstOrDefault(x => x.Attribute("id")?.Value == parentValue);
                if (parentElement != null)
                    SetWeaponData(parentElement, weapon);
            }

            // loop through all elements and set found elements
            foreach (XElement element in weaponElement.Elements())
            {
                string elementName = element.Name.LocalName.ToUpper();

                if (elementName == "RANGE")
                {
                    weapon.Range = double.Parse(element.Attribute("value")?.Value);
                }
                else if (elementName == "PERIOD")
                {
                    weapon.Period = double.Parse(element.Attribute("value")?.Value);
                }
                else if (elementName == "DISPLAYEFFECT")
                {
                    WeaponAddDamage(weapon, element.Attribute("value")?.Value);
                }
            }

            return weapon;
        }

        private void WeaponAddDamage(UnitWeapon weapon, string displayEffectValue)
        {
            if (!string.IsNullOrEmpty(displayEffectValue))
            {
                XElement effectDamageElement = GameData.XmlGameData.Root.Elements("CEffectDamage").FirstOrDefault(x => x.Attribute("id")?.Value == displayEffectValue);
                if (effectDamageElement != null)
                {
                    XElement amountElement = effectDamageElement.Element("Amount");
                    if (amountElement != null)
                    {
                        weapon.Damage = double.Parse(amountElement.Attribute("value").Value);
                    }
                }

                double? scaleValue = GameData.GetScaleValue(("Effect", displayEffectValue, "Amount"));
                if (scaleValue.HasValue)
                    weapon.DamageScaling = scaleValue.Value;
            }
        }
    }
}
