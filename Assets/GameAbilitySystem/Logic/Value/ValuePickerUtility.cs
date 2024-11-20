using MissQ;

namespace GAS.Logic.Value
{
    public static class ValuePickerUtility
    {
        public static FP GetValue(ValuePickerBase valuePicker, GameUnit unit, uint lv)
        {
            switch (valuePicker)
            {
                case ValuePickerParam v :
                    return unit.Sys.AssetConfigProvider.GetAbilityEffectParamVal(v.paramName, lv);
                case ValuePickerSimpleAttribute v:
                    return unit.GetSimpleAttributeVal(v.simpleAttributeType);
                case ValuePickerCompositeAttribute v:
                    return unit.GetCompositeAttributeVal(v.compositeAttributeType);
            }

            return 0;
        }
    }
}