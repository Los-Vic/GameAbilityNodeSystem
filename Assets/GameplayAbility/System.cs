using System;

namespace Gameplay.Ability
{
    public class SystemAttribute : Attribute
    {
        
    }
    
    public interface ISystem
    {
        public void Execute(World world , float time);
    }
}