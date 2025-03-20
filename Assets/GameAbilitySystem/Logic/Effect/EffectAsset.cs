using System;
using System.Collections.Generic;
using GAS.Logic.Cue;
using GAS.Logic.EffectProcess;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GAS.Logic
{
    public enum EEffectTag
    {
        
    }

    [Serializable]
    public class EffectGameCueCfg : GameCueCfg
    {
        
    }
    
    
    [CreateAssetMenu(menuName = "GameAbilitySystem/EffectAsset", fileName = "NewEffect")]
    public class EffectAsset : ScriptableObject
    {
        [Title("Effect")]
        public string effectName;
        
        [BoxGroup("EffectTag")]
        public List<EEffectTag> effectTags = new();
        
        [BoxGroup("EffectProcess")]
        [SerializeReference]
        public List<EffectProcessCfgBase> effectProcesses = new();
        
        [BoxGroup("EffectGameCues")]
        [SerializeReference]
        public List<EffectGameCueCfg> effectGameCues = new();
    }
}