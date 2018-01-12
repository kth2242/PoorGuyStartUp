using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;

namespace Anonym.Isometric
{
    using Util;
    [CustomPropertyDrawer(typeof(IsoTile))]
    public class IsoTileDrawer : PropertyDrawer
    {
        static int cellSize = 40;
        static int border = 2;
        public static int RectHeight { get { return cellSize + border * 2; } }
        public static Rect GetRect()
        {
            Rect rt = EditorGUILayout.GetControlRect(
                        new GUILayoutOption[] { GUILayout.Height(RectHeight), GUILayout.ExpandWidth(true) });
            return EditorGUI.IndentedRect(rt);
        }

        Editor gameObjectEditor;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.ScrollWheel)
                return;
                
            SerializedProperty sp = property.serializedObject.FindProperty(property.propertyPath);
            if (sp != property)
                sp = property;
            if (sp.objectReferenceValue == null)
                return;
            GameObject _target = ((Component)sp.objectReferenceValue).gameObject;
            Color borderColor = Util.CustomEditorGUI.Color_Tile;

            Rect rect = position;
            Rect rect_inside = new Rect(rect.xMin + border, rect.yMin + border, rect.width - border * 2, rect.height - border * 2);

            Rect rect_preview = new Rect(rect_inside.xMin, rect_inside.yMin, cellSize, rect_inside.height);
            Rect rect_info_name =
                new Rect(rect_inside.xMin + cellSize, rect_inside.yMin,
                    rect_inside.width - cellSize * 3, rect_inside.height / 2);
            Rect rect_info_Sub =
                new Rect(rect_info_name.xMin, rect_info_name.yMin + cellSize / 2,
                    rect_info_name.width, rect_info_name.height);
            Rect rect_delete = new Rect(rect_inside.xMax - cellSize * 2, rect_inside.yMin, cellSize, rect_inside.height);
            Rect rect_select = new Rect(rect_inside.xMax - cellSize, rect_inside.yMin, cellSize, rect_inside.height);

            float fExWidth = rect_delete.width + rect_select.width;
            rect_info_name.width += fExWidth;
            rect_info_Sub.width += fExWidth;

            EditorGUI.DrawRect(rect, borderColor);
            EditorGUI.DrawRect(rect_inside, new Color(0.8f, 0.8f, 0.8f));
            
            Iso2DObject[] _iso2Ds = _target.GetComponentsInChildren<Iso2DObject>();
            for (int i = 0 ; i < _iso2Ds.Length; ++i)
            {
                if (_iso2Ds[i] != null && (_iso2Ds[i].IsXYZSide || _iso2Ds[i].IsUnionSide))
                {
                    CustomEditorGUI.DrawSideSprite(rect_preview, _iso2Ds[i], false, false);
                }
            }

            int iLv = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect_info_name, _target.name, EditorStyles.boldLabel);
            // 서브 인포 출력
            //using (new EditorGUILayout.HorizontalScope())
            {
                float _fMinSize = Mathf.Min(rect_info_Sub.width, rect_info_Sub.height);

                for (int i = 0; i < _iso2Ds.Length; ++i)
                {
                    if (_iso2Ds[i] != null && _iso2Ds[i].IsAttachment)
                    {
                        Rect _rt = EditorGUI.IndentedRect(rect_info_Sub);
                        _rt.width = _rt.height = _fMinSize;
                        rect_info_Sub.xMin += _fMinSize;
                        CustomEditorGUI.DrawSideSprite(_rt, _iso2Ds[i], false, true);
                    }
                }
            }
            EditorGUI.indentLevel = iLv;

            using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
            {
                if (GUI.Button(rect_delete.ReSize(8f, 8f), "Del!"))
                {
                    Undo.DestroyObjectImmediate(_target.gameObject);
                }
            }
            using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightMagenta))
            {
                if (GUI.Button(rect_select, "Go!"))
                {
                    Selection.activeGameObject = _target.gameObject;
                }
            }
        }
    }
}