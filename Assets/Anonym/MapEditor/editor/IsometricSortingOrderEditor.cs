using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Anonym.Isometric
{
    using Util;

    [CanEditMultipleObjects]
	[CustomEditor(typeof(IsometircSortingOrder))]
    public class IsometricSortingOrderEditor : Editor
    {
        bool bPrefab;
		SerializedProperty _iParticleSortingAdd;
        SerializedProperty _iExternSortingAdd;
        SerializedProperty _iLastSortingOrder;
        
		IsometircSortingOrder _target;

		void OnEnable()
        {
            if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;

			if ((_target = (IsometircSortingOrder)target) == null)
				return;

			_iParticleSortingAdd = serializedObject.FindProperty("iParticleSortingAdd");
            _iLastSortingOrder = serializedObject.FindProperty("iLastSortingOrder");
            _iExternSortingAdd = serializedObject.FindProperty("_iExternAdd");
        }

		public override void OnInspectorGUI()
        {
            if (bPrefab){
                base.DrawDefaultInspector();
                return;
            }

			serializedObject.Update();

            EditorGUILayout.Separator();

            if(_iExternSortingAdd.intValue != 0)
                EditorGUILayout.LabelField("Extern Sorting Order : ", _iExternSortingAdd.intValue.ToString());

            bool bCorruptedSortingOrder = _target.Corrupted_LastSortingOrder();
            using (new EditorGUI.DisabledGroupScope(bCorruptedSortingOrder))
            {
                if (!bCorruptedSortingOrder)
                    EditorGUILayout.LabelField("Last SortingOrder : ", _iLastSortingOrder.intValue.ToString());
                else
                    EditorGUILayout.LabelField("Sorting Order is 0");
            }
            
            EditorGUI.BeginChangeCheck();
            
            using (new EditorGUI.DisabledGroupScope(!IsoMap.instance.bUseIsometricSorting))
            {
                EditorGUILayout.PropertyField(_iParticleSortingAdd, new GUIContent("iAdd for ParticleSorting : "));
            }

            bool bUpdated = EditorGUI.EndChangeCheck();
            
			serializedObject.ApplyModifiedProperties();
            
            if (bUpdated)
            {
                _target.Update_SortingOrder(true);
            }
		}
    }
}