namespace GAS.Logic
{
    public interface IAbilityCue
    {
        void PlayPreCast(int index, GameUnit[] targets);
        void PlayCast(int index, GameUnit[] targets);
        void PlayPostCast(int index, GameUnit[] targets);
    }
}