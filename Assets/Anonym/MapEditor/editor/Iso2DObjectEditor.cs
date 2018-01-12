using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;

namespace Anonym.Isometric
{
    using Util;

	[CustomEditor(typeof(Iso2DObject))]
     public class Iso2DObjectEditor : Editor
    {
		bool bPrefab = false;
        Sprite tileSprite;
        
        public static float Max_Slider = 2f;

		Iso2DObject _t;

		void OnEnable()
        {
			if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;

			_t = (Iso2DObject)target;
        }
        
        

        bool undoredo()
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                Repaint();
                return true;
            }
            return false;
        }

		public override void OnInspectorGUI()
        {
			if (bPrefab)
            {
                base.DrawDefaultInspector();
                return;
            }

            if (undoredo())
                return;
            
			EditorGUILayout.Separator();
			CustomEditorGUI.Iso2DObjectField(serializedObject);
            EditorGUILayout.Separator();			
		}

		public void OnSceneGUI()
        {
			if (bPrefab)
                return;
		}
        
    }
}