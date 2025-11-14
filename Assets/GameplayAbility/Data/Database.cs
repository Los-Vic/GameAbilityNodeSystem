using Gameplay.Common;

namespace Gameplay.Ability
{
    public class Database
    {
        public const int DefaultCollectionCapacity = 8;
        public DataTableAccess<UnitIDTable, uint, uint> UnitIDTableAccess { get; private set; }
        public DataTableSetAccess<UnitFieldsTable, uint, UnitFieldsRecord> UnitTableSetAccess { get; private set; }


        public void Init()
        {
            UnitIDTableAccess.Init(new UnitIDTable(), DefaultCollectionCapacity);
            UnitTableSetAccess.Init(DefaultCollectionCapacity,DefaultCollectionCapacity);

            UnitIDTableAccess.EventDispatcher.OnAddRecord += OnUnitIDTableAdd;
            UnitIDTableAccess.EventDispatcher.OnRemoveRecord += OnUnitIDTableRemove;
        }

        private void OnUnitIDTableRemove(uint uintID, uint _)
        {
            
        }

        private void OnUnitIDTableAdd(uint uintID, uint _)
        {
            
        }
    }
}