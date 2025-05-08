using System;
using System.Collections.Generic;
using GAS.Logic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GAS.Editor
{
    [Serializable]
    public class GameUnitEditorAttribute
    {
        [BoxGroup("Attribute")]
        [ReadOnly]
        public string name;
        [BoxGroup("Attribute")]
        [ReadOnly]
        public float val;
    }
    
    [Serializable]
    public class GameUnitEditorEffect
    {
        [ReadOnly] public string name;
    }
    
    [Serializable]
    public class GameUnitEditorAbility
    {
        [ReadOnly] public string name;
    }
    
    [Serializable]
    public class GameUnitEditorTag
    {
        [ReadOnly] public string name;
    }
    
    [Serializable]
    public class GameUnitEditorObj
    {
        [BoxGroup("Unit")]
        [ReadOnly]
        public string name;
        
        [BoxGroup("Unit")]
        [LabelText("Attributes")]
        [Searchable]
        [ListDrawerSettings(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
        public List<GameUnitEditorAttribute> attributes;
        
        [BoxGroup("Unit")]
        [LabelText("Effects")]
        [Searchable]
        [ListDrawerSettings(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
        public List<GameUnitEditorEffect> effects;
        
        [BoxGroup("Unit")]
        [LabelText("Abilities")]
        [Searchable]
        [ListDrawerSettings(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
        public List<GameUnitEditorAbility> abilities;
        
        [BoxGroup("Unit")]
        [LabelText("Tags")]
        [Searchable]
        [ListDrawerSettings(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
        public List<GameUnitEditorTag> tags;
    }
    
    public class GameUnitEditorWindow:OdinEditorWindow
    {
        [ReadOnly]
        public string introduction = "Help to view unit states in game ability system";
        
        [Searchable]
        [ListDrawerSettings(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 5)]
        [InfoBox("No unit !", VisibleIf = "@unitsTable.Count == 0")]
        public List<GameUnitEditorObj> unitsTable = new();
        
        [MenuItem("GameAbilitySystem/GameUnitEditorWindow")]
        private static void OpenWindow()
        {
            var window = GetWindow<GameUnitEditorWindow>();
            window.InitWindow();
        }

        [Button("RefreshList", ButtonSizes.Large)]
        [GUIColor(0, 0.8f ,0)]
        [InfoBox("Please refresh manually when unit states change !")]
        public void RefreshUnitList()
        {
            BuildUnitList();
        }

        private void InitWindow()
        {
            BuildUnitList();
        }

        private void BuildUnitList()
        {
            unitsTable.Clear();
            var systemAuthoring = FindAnyObjectByType<GameAbilitySystemAuthoring>();
            if (systemAuthoring?.System == null)
            {
                Debug.LogWarning("Can not find a game ability system instance !");
                return;
            }

            var unitList = new List<GameUnit>();
            systemAuthoring.System.GetGameUnits(ref unitList);
            
            var simpleAttributes = new List<SimpleAttribute>();
            var compositeAttributes = new List<CompositeAttribute>();
            var tags = new List<EGameTag>();
            
            foreach (var unit in unitList)
            {
                var editorObj = new GameUnitEditorObj
                {
                    name = unit.UnitName,
                    attributes = new List<GameUnitEditorAttribute>(),
                    effects = new List<GameUnitEditorEffect>(),
                    abilities = new List<GameUnitEditorAbility>(),
                    tags = new List<GameUnitEditorTag>()
                };

                unit.GetAllSimpleAttributes(ref simpleAttributes);
                unit.GetAllCompositeAttributes(ref compositeAttributes);
                unit.GetTagContainer().GetAllTag(ref tags);

                foreach (var attribute in simpleAttributes)
                {
                    editorObj.attributes.Add(new GameUnitEditorAttribute()
                    {
                        name = attribute.Type.ToString(),
                        val = (float)attribute.Val,
                    });
                }

                foreach (var attribute in compositeAttributes)
                {
                    editorObj.attributes.Add(new GameUnitEditorAttribute()
                    {
                        name = attribute.Type.ToString(),
                        val = (float)attribute.Val,
                    });
                }

                foreach (var ability in unit.GameAbilities)
                {
                    editorObj.abilities.Add(new GameUnitEditorAbility()
                    {
                        name = ability.AbilityName
                    });
                }

                foreach (var effect in unit.GameEffects)
                {
                    editorObj.effects.Add(new GameUnitEditorEffect()
                    {
                        name = effect.EffectName
                    });
                }

                foreach (var tag in tags)
                {
                    editorObj.tags.Add(new GameUnitEditorTag()
                    {
                        name = tag.ToString()
                    });   
                }
                
                unitsTable.Add(editorObj);
            }
        }
    }
}