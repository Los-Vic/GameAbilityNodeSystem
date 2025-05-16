namespace GAS.Logic
{
    public interface ICommandDelegator
    {
        public GameUnit SpawnUnit(string unitName, int playerIndex);
    }
}