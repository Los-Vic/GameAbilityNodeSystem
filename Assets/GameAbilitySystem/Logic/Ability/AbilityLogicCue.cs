namespace GAS.Logic
{
    public class AbilityLogicCue
    {
        public bool IsValid { get; private set; }
        private IAbilityCue _abilityCue;

        public void Reset()
        {
            IsValid = false;
            _abilityCue = null;
        }
        
        public void BindCue(IAbilityCue abilityCue)
        {
            IsValid = true;
            _abilityCue = abilityCue;
        }
    }
}