using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anonym.Isometric
{
    using Util;

    [CustomEditor(typeof(RegularCollider))]    
	public class RegularColliderEditor : Editor {

        bool bPrefab = false;
		RegularCollider _rc;
		IsoTile _t;
        Iso2DObject _childIso2D_0;
        SerializedProperty _spIso2DScaleMultiplier;

        bool undoredo()
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                Repaint();
                return true;
            }
            return false;
        }

		void OnEnable()
        {
            if (target != null && (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab)))
                return;

			_rc = (RegularCollider) target;
            if (_rc == null)
                return;

			_t = _rc.GetComponentInParent<IsoTile>();

            _spIso2DScaleMultiplier = serializedObject.FindProperty("_vIso2DScaleMultiplier");

            update_childIso2D_0();
            // _r.update_subColliders();
        }

        void update_childIso2D_0()
        {
            if (_rc.Iso2Ds != null && _rc.Iso2Ds.Length > 0)
                _childIso2D_0 = _rc.Iso2Ds[0];
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

            serializedObject.Update();

            EditorGUILayout.Separator();

            if (Event.current.type == EventType.Layout)
                update_childIso2D_0();

            if (_childIso2D_0 != null)
            {
                Util.CustomEditorGUI.NewParagraph("[Iso2DObject Control Helper]");
                EditorGUI.indentLevel = 0;
            
                float fWidth = EditorGUIUtility.currentViewWidth * 0.475f;
                float mfWidth = fWidth * 0.95f;
                Vector3 v3Max = _t.coordinates.grid.TileSize * 2f;

                EditorGUILayout.LabelField("Sorting order : " + _childIso2D_0.sprr.sortingOrder, EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        _spIso2DScaleMultiplier.vector3Value = Util.CustomEditorGUI.Vector2Slider(
                            _spIso2DScaleMultiplier.vector3Value, _t.coordinates.grid.TileSize, 
                            "Scale", Vector3.one * 0.1f, v3Max, mfWidth);
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach(var r in _rc.Iso2Ds)
                                r.Undo_LocalScale(_spIso2DScaleMultiplier.vector3Value);
                        }
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        Vector3 _origin = _rc.Iso2Ds[0].GetLocalPosition_WithoutFudge();
                        EditorGUI.BeginChangeCheck();
                        Vector3 _new = Util.CustomEditorGUI.Vector3Slider(
                            _origin, Vector3.zero, "Iso2D Position Handle", 
                            -v3Max, v3Max, mfWidth);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Vector3 _diff = _new - _origin;
                            for (int i = 0 ; i < _rc.Iso2Ds.Length; ++i)
                                _rc.Iso2Ds[i].Undo_LocalOffset(_rc.Iso2Ds[i].GetLocalPosition_WithoutFudge() + _diff);
                        }
                    }
                }
            }

			EditorGUILayout.Separator();
            Util.CustomEditorGUI.NewParagraph("[Sub Collider Creator]");
            using (new EditorGUILayout.HorizontalScope()){
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                {
                    EditorGUI.BeginChangeCheck();

                    init_subCollider(Util.CustomEditorGUI.Undo_Instantiate(
                        IsoMap.instance.Collider_Cube_Prefab, _rc.transform, "Create Cube", true));
                    init_subCollider(Util.CustomEditorGUI.Undo_Instantiate(
                        IsoMap.instance.Collider_X_Prefab, _rc.transform, "Axis X", true));
                    init_subCollider(Util.CustomEditorGUI.Undo_Instantiate(
                        IsoMap.instance.Collider_Y_Prefab, _rc.transform, "Axis Y", true));
                    init_subCollider(Util.CustomEditorGUI.Undo_Instantiate(
                        IsoMap.instance.Collider_Z_Prefab, _rc.transform, "Axis Z", true));
                        
                    if (EditorGUI.EndChangeCheck())
                    {
                        // _r.update_subColliders();
                    }
                }
            }

            if (_rc.SubColliders != null && _rc.SubColliders.Length > 0)
            {
                EditorGUILayout.Separator();
                Util.CustomEditorGUI.NewParagraph("[Sub Colliders(Can not change)]");

                foreach(var subCollider in _rc.SubColliders)
                {
                    if (subCollider != null)
                    {
                        using (new EditorGUILayout.HorizontalScope()){
                            using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                            {
                                if (GUILayout.Button("Del"))
                                {
                                    Undo.RecordObject(_rc, "Destroy : SubCollider");
                                    Undo.DestroyObjectImmediate(subCollider.gameObject);
                                    EditorUtility.SetDirty(_rc.gameObject);
                                }
                            }
                            EditorGUILayout.ObjectField(subCollider, typeof(BoxCollider), allowSceneObjects:true);
                        }
                    }
                }
            }

            if (_t.gameObject != _rc.gameObject)
            {
                EditorGUILayout.Separator();
                Util.CustomEditorGUI.NewParagraph("[Object Selector]");
                Util.CustomEditorGUI.ComSelector<IsoTile>(_t, "GO IsoTile");
                foreach(var r in _rc.Iso2Ds)
                    Util.CustomEditorGUI.ComSelector<Iso2DObject>(r, "GO Iso2DObject");
            }

            serializedObject.ApplyModifiedProperties();
		}

        void init_subCollider(GameObject _go)
        {
            if (_go == null)
                return;

            SubColliderHelper _sch = _go.GetComponent<SubColliderHelper>();
            if (_t.bAutoFit_ColliderScale)
			{
                _sch.ScaleMultiplier(_t.coordinates.grid.TileSize);
            }
            _sch.Toggle_UseGridTileScale(_t.bAutoFit_ColliderScale);
        }
	}
}
