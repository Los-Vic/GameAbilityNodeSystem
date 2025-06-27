using MissQ;

namespace GAS.Logic.Value
{
    public static class ValuePickerUtility
    {
        public static FP GetValue(ValuePickerBase valuePicker, GameUnit unit, uint lv = 0)
        {
            switch (valuePicker)
            {
                case ValuePickerSimpleAttribute v:
                    return unit.GetSimpleAttributeVal(v.simpleAttributeType);
                case ValuePickerCompositeAttribute v:
                    return unit.GetCompositeAttributeVal(v.compositeAttributeType);
                case ValuePickerConst v:
                    return v.val;
            }

            return unit.Sys.ValueProvider.GetValue(valuePicker, unit, lv);
        }
    }
}