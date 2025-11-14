using System.Collections.Generic;

namespace Gameplay.Ability
{
    public class World
    {
        public struct InterfaceImplementors
        {
            public IAssetManager<uint> AssetManager;
        }
        
        public float Time;
        
        private List<ISystem> _systems;
        
        //Interface implementors
        private IAssetManager<uint> _assetManager;
        
        //Handler managers
        
        
        #region Life Cycle

        public void OnCreate(ref InterfaceImplementors interfaceImplementors)
        {
            _assetManager = interfaceImplementors.AssetManager;
            
            _systems = new List<ISystem>();
            
        }
        
        public void Init()
        {
            
        }

        public void UnInit()
        {
            
        }

        #endregion
        
        //todo:使用Source Generator添加System的生成和注册
        #region System

        #endregion

        #region Time Management

        public void UpdateWorld(float deltaTime)
        {
            Time += deltaTime;
        }
        public void SetTime(float time)
        {
            Time = time;
        }

        #endregion

        #region Data

        
        

        #endregion
        
    }
}
