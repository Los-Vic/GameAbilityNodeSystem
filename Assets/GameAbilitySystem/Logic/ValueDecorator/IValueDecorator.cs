namespace GameAbilitySystem.Logic.ValueDecorator
{
    public interface IValueDecorator<T>
    {
        public bool Process(in T inVal, out T outVal);
    }
}