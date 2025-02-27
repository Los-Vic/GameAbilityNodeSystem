using MissQ;

namespace GAS.Logic.Value
{
    public static class ValuePickerUtility
    {
        public static FP GetValue(ValuePickerBase valuePicker, GameUnit unit, uint lv = 0)
        {
            switch (valuePicker)
            {
                case ValuePickerParam v :
                    return unit.Sys.AssetConfigProvider.GetAbilityEffectParamVal(v.paramName, lv);
                case ValuePickerSimpleAttribute v:
                    return unit.GetSimpleAttributeVal(v.simpleAttributeType);
                case ValuePickerCompositeAttribute v:
                    return unit.GetCompositeAttributeVal(v.compositeAttributeType);
                case ValuePickerConst v:
                    return v.val;
            }

            return 0;
        }
    }
}