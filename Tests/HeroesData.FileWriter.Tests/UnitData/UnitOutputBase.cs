﻿using Heroes.Models;

namespace HeroesData.FileWriter.Tests.UnitData
{
    public class UnitOutputBase : FileOutputTestBase<Unit>
    {
        public UnitOutputBase()
            : base(nameof(UnitData))
        {
        }

        protected override void SetTestData()
        {
            WeaponAttributeFactor weaponAttributeFactor1 = new WeaponAttributeFactor()
            {
                Type = "Minion",
                Value = 5,
            };
            WeaponAttributeFactor weaponAttributeFactor2 = new WeaponAttributeFactor()
            {
                Type = "Structure",
                Value = 0.75,
            };
            UnitWeapon unitWeapon = new UnitWeapon()
            {
                Damage = 56,
                Name = "Minion Archer Weapon",
                Period = 4,
                Range = 6,
                WeaponNameId = "MinionArcherWeapon",
            };
            unitWeapon.AddAttributeFactor(weaponAttributeFactor1);
            unitWeapon.AddAttributeFactor(weaponAttributeFactor2);

            Unit unit = new Unit()
            {
                Name = "Minion Archer",
                Id = "MinionArcher",
                Description = new TooltipDescription(string.Empty),
                HyperlinkId = "MinionArcher",
                CUnitId = "MinionArcher",
                DamageType = "Minion",
                InnerRadius = 1.125,
                Radius = 1.111,
                Sight = 6,
                Speed = 4,
                ScalingBehaviorLink = "MinionScaling",
                KillXP = 210,
                UnitPortrait = new UnitPortrait()
                {
                     TargetInfoPanelFileName = "targetInfo.png",
                },
            };
            unit.Life.LifeMax = 500;
            unit.AddUnitWeapon(unitWeapon);
            unit.AddAttribute("att1");
            unit.AddAttribute("att2");

            TestData.Add(unit);

            Unit unit2 = new Unit()
            {
                Name = "Minion Footman",
                Id = "MinionFootman",
                Description = new TooltipDescription(string.Empty),
                HyperlinkId = "MinionFootman",
                CUnitId = "MinionFootman",
                DamageType = "Minion",
                InnerRadius = 1.125,
                Radius = 1.111,
                Sight = 6,
                Speed = 4,
            };
            unit2.Life.LifeMax = 900;
            unit2.AddUnitWeapon(new UnitWeapon()
            {
                Damage = 75,
                Name = "Minion Footman Weapon",
                Period = 6,
                Range = 7,
                WeaponNameId = "MinionFootmanWeapon",
            });
            unit2.AddUnitWeapon(new UnitWeapon()
            {
                Damage = 56,
                Name = "Minion Archer Weapon",
                Period = 4,
                Range = 6,
                WeaponNameId = "MinionArcherWeapon",
            });

            TestData.Add(unit2);
        }
    }
}
