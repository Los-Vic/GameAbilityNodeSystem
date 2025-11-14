using System;
using Gameplay.Common;

namespace Gameplay.Ability
{
   

    public class UnitTableRelations
    {
        
    }
    
    
    public struct UnitFieldsRecord
    {
        public Handler<UnitPrimaryAttributeTable> HPrimaryAttributeTable;
        public Handler<UnitSecondaryAttributeTable> HSecondaryAttributeTable;
        
        public override bool Equals(object obj)
        {
            if(obj is not UnitFieldsRecord r)
                return false;

            if (r.HPrimaryAttributeTable != HPrimaryAttributeTable)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(HPrimaryAttributeTable);
            return hashCode.ToHashCode();
        }
    }

    public class UnitIDTable : DataTable<uint, uint>
    {
        
    }
    

    public class UnitFieldsTable : DataTable<uint, UnitFieldsRecord>
    {
        
    }
    
    public class UnitPrimaryAttributeTable : DataTable<EUnitPrimaryAttribute, float>
    {
        
    }
    
    public class UnitSecondaryAttributeTable: DataTable<EUnitSecondaryAttribute, float>
    {
        
    }
    
}