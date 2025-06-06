using System;

namespace GameplayCommonLibrary
{
    //目前没有对应的具体实现逻辑，暂时只用来标记用
    namespace GameplayCommonLibrary
    {
        /// <summary>
        /// 计算基于的数据，将来也许需要序列化的数据
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class EssentialStateAttribute : Attribute
        {
        }
    
    
        /// <summary>
        /// 不需要序列化的数据，在运行时，根据EssentialState可以计算得出
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class AccidentalStateAttribute : Attribute
        {
        }

    
        /// <summary>
        /// 引用类型，没法直接序列化
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class ReferenceStateAttribute : Attribute
        {
        
        }
    }
}