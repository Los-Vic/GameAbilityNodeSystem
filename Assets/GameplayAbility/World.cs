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
        
        //Event 
        public EventDispatcher EventDispatcher { get; private set; }
        
        //Database
        public Database Database { get; private set; }
        
        #region Life Cycle

        public void OnCreate(ref InterfaceImplementors interfaceImplementors)
        {
            _assetManager = interfaceImplementors.AssetManager;
            
            _systems = new List<ISystem>();
            Database = new Database();
            EventDispatcher = new EventDispatcher();
        }
        
        public void Init()
        {
            EventDispatcher.Init(this);
            Database.Init(this);
        }

        public void UnInit()
        {
            Database.UnInit();
            EventDispatcher.UnInit();
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
