using System.Collections.Generic;

namespace Gameplay.Ability
{
    public class Database
    {
        public const int DefaultCollectionCapacity = 8;
        public UnitIDTable UnitIDTable { get; private set; }

        public Dictionary<EUnitPrimaryAttribute, PrimaryAttributeTable> PrimaryAttributeTables;
        public Dictionary<EUnitSecondaryAttribute, SecondaryAttributeTable> SecondaryAttributeTables;
        public World World { get; private set; }

        internal void Init(World world)
        {
            UnitIDTable.Init(DefaultCollectionCapacity);
            UnitIDTable.OnAddRecord += OnUnitIDTableAdd;
            UnitIDTable.OnRemoveRecord += OnUnitIDTableRemove;
            
            InitAttributeTables();
        }

        internal void UnInit()
        {
            UnInitAttributeTables();
        }

        private void InitAttributeTables()
        {
            PrimaryAttributeTables = new();
            SecondaryAttributeTables = new();

            foreach (var e in (EUnitPrimaryAttribute[])typeof(EUnitPrimaryAttribute).GetEnumValues())
            {
                var table = new PrimaryAttributeTable();
                table.Init(Database.DefaultCollectionCapacity);
                PrimaryAttributeTables.Add(e, table);
                table.OnUpdateRecord += (unitId, oldVal, newVal) =>
                    World.EventDispatcher.InternalOnPrimaryAttributeUpdate(e, unitId, oldVal, newVal);
            }

            foreach (var e in (EUnitSecondaryAttribute[])typeof(EUnitSecondaryAttribute).GetEnumValues())
            {
                var table = new SecondaryAttributeTable();
                table.Init(Database.DefaultCollectionCapacity);
                SecondaryAttributeTables.Add(e, table);
                table.OnUpdateRecord += (unitId, oldVal, newVal) =>
                    World.EventDispatcher.InternalOnSecondaryAttributeUpdate(e, unitId, oldVal, newVal);
            }

            //ApplyAttackAttributeRelations();
        }

        private void UnInitAttributeTables()
        {
            PrimaryAttributeTables.Clear();
            SecondaryAttributeTables.Clear();
        }

        // private void ApplyAttackAttributeRelations()
        // {
        //     if (!PrimaryAttributeTables.TryGetValue(EUnitPrimaryAttribute.AttackBase, out var table1))
        //     {
        //         return;
        //     }
        //
        //     if (!PrimaryAttributeTables.TryGetValue(EUnitPrimaryAttribute.AttackBuff, out var table2))
        //     {
        //         return;
        //     }
        //
        //     if (!SecondaryAttributeTables.TryGetValue(EUnitSecondaryAttribute.Attack, out var table))
        //     {
        //         return;
        //     }
        //
        //     table1.OnUpdateRecord += (uintId, _, _) => UpdateAttack(uintId);
        //     table2.OnUpdateRecord += (uintId, _, _) => UpdateAttack(uintId);
        //
        //     return;
        //
        //     void UpdateAttack(uint unitID)
        //     {
        //         var val1 = table1.Get(unitID);
        //         var val2 = table2.Get(unitID);
        //         var v = val1 + val2;
        //         table.Update(unitID, ref v);
        //     }
        // }

        private void OnUnitIDTableRemove(uint uintID, uint _)
        {
        }

        private void OnUnitIDTableAdd(uint uintID, uint _)
        {
        }
    }
}