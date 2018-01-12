using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Anonym.Isometric
{
    using Util;
	[CustomEditor(typeof(GridCoordinates))]
    //[CanEditMultipleObjects]
    public class GridCoordinatesEditor : Editor
    {
		SerializedProperty _snapFree;
		GridCoordinates _target;

		void OnEnable()
        {
			_snapFree = serializedObject.FindProperty("bSnapFree");
			_target = (GridCoordinates) target;
        }

		public override void OnInspectorGUI()
        {
			serializedObject.Update();
            EditorGUILayout.Separator();
			CustomEditorGUI.GridCoordinatesField(serializedObject);  
			serializedObject.ApplyModifiedProperties();
		}

		public void OnSceneGUI()
        {
			if (PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;

			if (!_snapFree.boolValue)
            {
                int iOddCount = 9;
                for (int i = 0 ; i < iOddCount; ++i)
                {
                    Handles.color = Color.red;
                    Handles.DotHandleCap(i * 3, _target.transform.position 
                        + new Vector3((i - (iOddCount - 1)/2) * _target.grid.GridInterval.x, 0, 0), 
                        Quaternion.identity, 0.025f, EventType.Repaint);
                    Handles.color = Color.green;
                    Handles.DotHandleCap(i * 3 + 1, _target.transform.position 
                        + new Vector3(0, (i - (iOddCount - 1)/2) * _target.grid.GridInterval.y, 0), 
                        Quaternion.identity, 0.025f, EventType.Repaint);
                    Handles.color = Color.blue;
                    Handles.DotHandleCap(i * 3 + 2, _target.transform.position 
                        + new Vector3(0, 0, (i - (iOddCount - 1)/2) * _target.grid.GridInterval.z), 
                        Quaternion.identity, 0.025f, EventType.Repaint);
                }
            }
		}
    }
}