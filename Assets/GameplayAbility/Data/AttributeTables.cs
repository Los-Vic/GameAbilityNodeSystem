using System;
using System.Collections.Generic;
using Gameplay.Common;

namespace Gameplay.Ability
{
    public enum EUnitPrimaryAttribute
    {
        Hp = 0,
        AttackBase = 1,
        AttackBuff = 2,
    }

    public enum EUnitSecondaryAttribute
    {
        Attack = 0
    }

    //Key = UnitID , Record = Val
    public class PrimaryAttributeTable : DataTable<uint, float>
    {
    }
    public class PrimaryAttributeTableAccess : DataTableAccess<PrimaryAttributeTable, uint, float>
    {
    }

    //Key = UnitID, Record = Val
    public class SecondaryAttributeTable : DataTable<uint, float>
    {
    }

    public class SecondaryAttributeTableAccess : DataTableAccess<SecondaryAttributeTable, uint, float>
    {
    }
    
    public class AttributeTables
    {
        public Dictionary<EUnitPrimaryAttribute, PrimaryAttributeTableAccess> PrimaryAttributeTableAccesses;
        public Dictionary<EUnitSecondaryAttribute, SecondaryAttributeTableAccess> SecondaryAttributeTableAccesses;

        public void Init()
        {
            PrimaryAttributeTableAccesses = new();
            SecondaryAttributeTableAccesses = new();
            
            foreach (var val in (EUnitPrimaryAttribute[])typeof(EUnitPrimaryAttribute).GetEnumValues())
            {
                var access = new PrimaryAttributeTableAccess();
                access.Init(new PrimaryAttributeTable(), Database.DefaultCollectionCapacity);
                PrimaryAttributeTableAccesses.Add(val, access);
            }

            foreach (var val in (EUnitSecondaryAttribute[])typeof(EUnitSecondaryAttribute).GetEnumValues())
            {
                var access = new SecondaryAttributeTableAccess();
                access.Init(new SecondaryAttributeTable(), Database.DefaultCollectionCapacity);
                SecondaryAttributeTableAccesses.Add(val, access);
            }

            InitAttackEquationTables();
        }

        private void InitAttackEquationTables()
        {
            if (!PrimaryAttributeTableAccesses.TryGetValue(EUnitPrimaryAttribute.AttackBase, out var access1))
            {
                return;
            }
            
            if(!PrimaryAttributeTableAccesses.TryGetValue(EUnitPrimaryAttribute.AttackBuff, out var access2))
            {
                return;
            }
            
            if (!SecondaryAttributeTableAccesses.TryGetValue(EUnitSecondaryAttribute.Attack, out var access))
            {
                return;
            }

            access1.EventDispatcher.OnUpdateRecord += (uintId, _, _) => UpdateAttack(uintId);
            access2.EventDispatcher.OnUpdateRecord += (uintId, _, _) => UpdateAttack(uintId);
            
            return;
            
            void UpdateAttack(uint unitID)
            {
                var val1 = access1.Get(unitID);
                var val2 = access2.Get(unitID);
                var v = val1 + val2;
                access.Update(unitID, ref v);
            }
        }
        
    }
}