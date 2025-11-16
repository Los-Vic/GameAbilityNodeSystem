using Gameplay.Common;

namespace Gameplay.Ability
{
    public enum EEquationOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }
    
    public enum EBasicAttributeBelong
    {
        Unit,
        UnitsOfSameType,
        AllUnits,
    }
    
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
    public class PrimaryAttributeTable : Table<uint, float>
    {
    }

    //Key = UnitID, Record = Val
    public class SecondaryAttributeTable : Table<uint, float>
    {
    }

    //Key = UnitID， Val = UnitID
    public class UnitIDTable : Table<uint, uint>
    {
        
    }
    
    /*---------------Table-----------*/
    /* BasicAttributesForAllUnits */
    /* |BasicAttributeType| AttributeVal |*/
    
    /* BasicAttributesForSameTypeUnits */
    /* |EUnitType| UnitBasicAttributeTable| */
    
    /* BasicAttributeForUnit */
    /* |UnitID | UnitBasicAttributeTable| */
    
    /* UnitBasicAttributeTable */
    /* |BasicAttributeType|AttributeVal|*/
    
    /* CompositeAttributeForUnit */
    /* |UnitID|UnitCompositeAttributeTable| */
    
    /* UnitCompositeAttributeTable */
    /* |CompositeAttributeType| */
    
    /* CompositeAttributeRelationTable */
    /* |CompositeAttributeType|CompositeAttributeDefineTable| */
    
    /* CompositeAttributeDefineTable */
    /* |EEquationOperator|EBasicAttributeBelong|BasicAttributeType|*/
    
    /* AttributeIsIntegerTable*/
    /* |AttributeType| */
}