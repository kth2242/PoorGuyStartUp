using UnityEngine;
using UnityEditor;

//Original version of the ConditionalEnumHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: -
namespace Anonym.Util
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //get the attribute data
			ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
			//check if the propery we want to draw should be enabled
			bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
		
			//Enable/disable the property
			bool wasEnabled = GUI.enabled;
			GUI.enabled = enabled;
		
			//Check if we should draw the property
			if (!condHAtt.HidenInspector || enabled)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		
			//Ensure that the next property that is being drawn uses the correct settings
			GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
 
			if (!condHAtt.HidenInspector || enabled)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				//The property is not being drawn
				//We want to undo the spacing added before and after the property
				return -EditorGUIUtility.standardVerticalSpacing;
			}
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
        {
            bool enabled = true;
			//Look for the sourcefield within the object that the property belongs to
			string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
			string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
		
			if (sourcePropertyValue != null)
			{
				enabled = condHAtt.Inverse ? !sourcePropertyValue.boolValue : sourcePropertyValue.boolValue;
			}
			else
			{
				Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
			}
		
			return enabled;
        }

    }
}