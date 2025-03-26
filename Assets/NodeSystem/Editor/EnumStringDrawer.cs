using NS;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NSEditor
{
    [CustomPropertyDrawer(typeof(EnumStringAttribute))]
    public class EnumStringDrawer:PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var view = new VisualElement();
            if (attribute is not EnumStringAttribute attr)
                return view;
            
            var cfg = AssetDatabase.LoadAssetAtPath<ScriptableObject>(attr.ProviderAsset);
            if (cfg is not IEnumStringProvider provider)
            {
                Debug.LogError($"EnumStringAttribute provider not found: {attr.ProviderAsset}");
                return view;
            }

            var stringList = provider.GetEnumStringList();
            
            var dropdownField = new DropdownField(property.displayName);
            
            dropdownField.choices.Add("Null");
            dropdownField.RegisterValueChangedCallback((evt) => 
                OnDropdownFieldValueChanged(evt, property));
            
            foreach (var t in stringList)
            {
                dropdownField.choices.Add(t);
            }

            var val = property.stringValue;
            var bFindMatch = false;
            for (var i = 0; i < dropdownField.choices.Count; i++)
            {
                if (val == dropdownField.choices[i])
                {
                    bFindMatch = true;
                    dropdownField.index = i;
                }
            }

            if (!bFindMatch)
            {
                property.stringValue = "Null";
                dropdownField.index = 0;
            }
            
            view.Add(dropdownField);

            property.serializedObject.ApplyModifiedProperties();
            return view;
        }

        private void OnDropdownFieldValueChanged(ChangeEvent<string> evt, SerializedProperty prop)
        {
            prop.stringValue = evt.newValue;
            prop.serializedObject.ApplyModifiedProperties();
        }
    }
}