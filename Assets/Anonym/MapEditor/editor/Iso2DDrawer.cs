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
    [CustomPropertyDrawer(typeof(Iso2DObject))]
    public class Iso2DDrawer : PropertyDrawer
    {
        const int cellSize = 44;
        const int fudgeWidth = 175;
        const int border = 2;

        public static int RectHeight{   get {   return cellSize + border * 2;   }  }
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

            GameObject _target = ((Component) sp.objectReferenceValue).gameObject;
            IsoTile _thisTile = _target.GetComponent<IsoTile>();
            Iso2DObject _iso2D = _target.GetComponent<Iso2DObject>();
            SpriteRenderer sprr = _target.GetComponent<SpriteRenderer>();
            Color borderColor = Util.CustomEditorGUI.Color_Tile;

            if (_thisTile == null && _iso2D != null)
            {
                switch(_iso2D._Type)
                {
                    case Iso2DObject.Type.Obstacle:
                        borderColor = Util.CustomEditorGUI.Color_Obstacle;
                        break;
                    case Iso2DObject.Type.Overlay:
                        borderColor = Util.CustomEditorGUI.Color_Overlay;
                        break;
                    case Iso2DObject.Type.Side_Union:
                    case Iso2DObject.Type.Side_X:
                    case Iso2DObject.Type.Side_Y:
                    case Iso2DObject.Type.Side_Z:
                        borderColor = Util.CustomEditorGUI.Color_Tile;
                        break;
                    default:
                        borderColor = Util.CustomEditorGUI.Color_Side;
                        break;
                }
            }

            if (sprr == null)
            {
                GUILayout.Label("Empty Bulk", EditorStyles.objectFieldThumb);
                return;
            }

            Rect rect = position;
            Rect rect_inside = new Rect(rect.xMin + border, rect.yMin + border, rect.width - border * 2, rect.height - border * 2);

            Rect rect_preview = new Rect(rect_inside.xMin, rect_inside.yMin, cellSize, rect_inside.height);
            Rect rect_info_name =
                new Rect(rect_preview.xMax, rect_inside.yMin,
                    rect_inside.width - cellSize - fudgeWidth, rect_inside.height * 0.5f);
            Rect rect_Fudge = 
                new Rect(rect_info_name.xMax, rect_inside.yMin,
                    fudgeWidth, rect_inside.height * 0.5f - border);
            Rect rect_info_Sub =
                new Rect(rect_info_name.xMin, rect_info_name.yMin + cellSize * 0.5f,
                    rect_info_name.width, rect_inside.height - rect_info_name.height);
            Rect rect_delete = 
                new Rect(rect_inside.xMax - cellSize * 3.3f, rect_info_Sub.yMin, cellSize, rect_info_Sub.height);
            Rect rect_select_ctlr = 
                new Rect(rect_inside.xMax - cellSize * 2.2f, rect_info_Sub.yMin, cellSize, rect_info_Sub.height);
            Rect rect_select_go = 
                new Rect(rect_inside.xMax - cellSize * 1.1f, rect_info_Sub.yMin, cellSize, rect_info_Sub.height);
                
            bool bControllerable = (_thisTile == null || _thisTile.gameObject != _target.gameObject)
                ||  (Selection.activeGameObject != null 
                    && Selection.activeGameObject.GetComponent<IsoTileBulk>());

            EditorGUI.DrawRect(rect, borderColor);
            EditorGUI.DrawRect(rect_inside, new Color(0.8f, 0.8f, 0.8f));
            
            CustomEditorGUI.DrawSideSprite(rect_preview, _iso2D, false, false);

            int iLv = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect_info_name, _target.name, EditorStyles.boldLabel);

            float _fTmp = CustomEditorGUI.FloatSlider(rect_Fudge, "Depth", _iso2D.fDepthFudge, -1f, 1f);
            if (_fTmp != _iso2D.fDepthFudge)
            {
                _iso2D.Undo_DepthFudge(_fTmp);
            }
            // 서브 인포 출력
            //using (new EditorGUILayout.HorizontalScope())
            {
                float _fMinSize = Mathf.Min(rect_info_Sub.width, rect_info_Sub.height);
                SpriteRenderer[] _sprrList = _target.GetComponentsInChildren<SpriteRenderer>();

                for(int i = 0 ; i < _sprrList.Length; ++i)
                {                   
                    if (_sprrList[i].sprite != null && _sprrList[i] != sprr)
                    {
                        Rect _rt = EditorGUI.IndentedRect(rect_info_Sub);
                        _rt.width = _rt.height = _fMinSize;
                        rect_info_Sub.xMin += _fMinSize;
                        // CustomEditorGUI.DrawSideSprite(_rt, _sprrList[i].sprite, ._Type);
                        Util.CustomEditorGUI.DrawSprite(_rt, _sprrList[i].sprite, _sprrList[i].color, true, true);
                    }
                }
            }
            if (bControllerable)
            {
                RegularCollider ctlr = _target.transform.parent.GetComponentInParent<RegularCollider>();
                if (Selection.activeGameObject == ctlr.gameObject)
                    ctlr = null;
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                {
                    if (GUI.Button((ctlr != null ? rect_delete : rect_select_ctlr).ReSize(2f, 2f), "Del!"))
                    {                
                        _iso2D.DestoryGameObject(true, true);
                    }
                }
                if (ctlr != null)
                {
                    using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightMagenta))
                    {
                        if (GUI.Button(rect_select_ctlr, "Ctlr"))
                        {
                            Selection.activeGameObject = ctlr.gameObject;
                        }
                    }
                }
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightMagenta))
                {
                    if (GUI.Button(rect_select_go, "Iso2D"))
                    {
                        Selection.activeGameObject = _target.gameObject;
                    }
                }
            }
            EditorGUI.indentLevel = iLv;
        }
    }
}

