namespace GameAbilitySystem.Logic
{
    public class GameAbilitySystemCfg
    {
        #region Define Attribute Enum

        public enum ESimpleAttributeType
        {
            //这里添加新的属性
        }

        public enum ECompositeAttributeType
        {
            //这里添加新的属性
        }

        #endregion


        #region Object Pool

        public static class PoolSizeDefine
        {
            public const int DefaultCapacity = 512;
            public const int DefaultMaxSize = 1024;
        }

        #endregion

        #region Tag

        public enum EGameTag
        {
            //这里添加标签
        }

        #endregion
    }
}