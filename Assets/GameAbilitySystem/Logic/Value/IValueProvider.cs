using MissQ;

namespace GAS.Logic.Value
{
    public interface IValueProvider
    {
        FP GetValue(ValuePickerBase valuePicker, GameUnit unit, uint lv = 0);
    }
}