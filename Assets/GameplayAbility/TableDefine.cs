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