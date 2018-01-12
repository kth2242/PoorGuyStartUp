using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Anonym.Util
{
	using Isometric;	
    public partial class CustomEditorGUI
	{
#if UNITY_EDITOR
        public static void Iso2DDrawField(SerializedProperty _Iso2D)
        {
            EditorGUI.PropertyField(Iso2DDrawer.GetRect(), _Iso2D);
        }
        public static void Iso2DObjectField(SerializedObject _Iso2DSerializedObject)
        {
            _Iso2DSerializedObject.Update();

            SerializedProperty vScaler = _Iso2DSerializedObject.FindProperty("localScale");
            SerializedProperty vRotator = _Iso2DSerializedObject.FindProperty("localRotation");
            
            Iso2DObject _Iso2D = (Iso2DObject)_Iso2DSerializedObject.targetObject;
            IsoTile _parentTile = _Iso2D.GetComponentInParent<IsoTile>();
            SpriteRenderer sprr = _Iso2D.GetComponent<SpriteRenderer>();

            //_Iso2D._Type = (Iso2DObject.Type) EditorGUILayout.EnumPopup("Type", _Iso2D._Type);

            EditorGUI.indentLevel = 0;
            Undo_Iso2DSpriteField(_Iso2D, Color.cyan);
            EditorGUILayout.LabelField("Type : " + _Iso2D._Type);
            
            float iWidth = EditorGUIUtility.currentViewWidth / 2 - 4;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUILayout.VerticalScope(
                    GUILayout.MaxWidth(iWidth)))
            {                 
                GUILayout.Space(5);

                Iso2DObjectEditor.Max_Slider = Mathf.Max(new float[]{1f, vScaler.vector3Value.x, vScaler.vector3Value.y, 
                    EditorGUILayout.FloatField("Cap of Scale Slider", Iso2DObjectEditor.Max_Slider)});

                vScaler.vector3Value = Vector2Slider(vScaler.vector3Value, Vector2.one, "[Scale]", 
                    -Iso2DObjectEditor.Max_Slider *  Vector2.one, Iso2DObjectEditor.Max_Slider * Vector2.one, iWidth);
                // vScaler.vector3Value = EditorGUILayout.Vector3Field("",vScaler.vector3Value, GUILayout.MaxWidth(iWidth));
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Flip", GUILayout.MaxWidth(iWidth * 0.3f));
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.MaxWidth(iWidth * 0.3f)))
                        vScaler.vector3Value = Vector3.Scale(vScaler.vector3Value, new Vector3(-1, 1, 1));
                    if (GUILayout.Button("Y", EditorStyles.miniButton, GUILayout.MaxWidth(iWidth * 0.3f)))
                        vScaler.vector3Value = Vector3.Scale(vScaler.vector3Value, new Vector3(1, -1, 1));
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ToggleLeft(
                    string.Format("Use Global PPU Scale(x{0:0.00})", _Iso2D.PPURefScale),
                    _Iso2D.bApplyPPUScale, GUILayout.MaxWidth(iWidth));
                if (EditorGUI.EndChangeCheck())
                {                     
                    _Iso2D.Toggle_ApplyPPUScale();
                }
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Global PPU Scale = Source PPU / Ref PPU", MessageType.None);
                EditorGUILayout.LabelField(
                    "Image Source PPU " + sprr.sprite.pixelsPerUnit, 
                    GUILayout.MaxWidth(iWidth));
                EditorGUILayout.LabelField(
                    "IsoMap Reference PPU " + IsoMap.instance.ReferencePPU, 
                    GUILayout.MaxWidth(iWidth));                
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();

                Util.CustomEditorGUI.NewParagraph("[Rotation]");
                EditorGUILayout.LabelField("Tile local rotation adjustment", GUILayout.MaxWidth(iWidth));
                vRotator.vector2Value = EditorGUILayout.Vector2Field("",vRotator.vector2Value, GUILayout.MaxWidth(iWidth));
                EditorGUILayout.LabelField(
                    string.Format("+ global tile rotation(X {0}, Y {1})", 
                        IsoMap.instance.TileAngle.x,
                        IsoMap.instance.TileAngle.y), GUILayout.MaxWidth(iWidth));
                EditorGUILayout.Separator();
                
                //EditorGUILayout.EndVertical();
                //GUILayout.EndArea();         
            }
            drawPackedTexture(_Iso2D, Mathf.Min(125f, iWidth * 0.75f));
            EditorGUILayout.EndHorizontal();
            
            if (_parentTile != null && _Iso2D.gameObject != _parentTile.gameObject)
            {
                EditorGUILayout.Separator();
                Util.CustomEditorGUI.NewParagraph("[Object Selector]");
                if (_Iso2D.RC != null)
                    Util.CustomEditorGUI.ComSelector<RegularCollider>(_Iso2D.RC, "GO Controller");
                Util.CustomEditorGUI.ComSelector<IsoTile>(_parentTile, "GO IsoTile");
            }

            _Iso2DSerializedObject.ApplyModifiedProperties();            
        }   
        static void drawPackedTexture(Iso2DObject _Iso2D, float _fMaxWidth)
        {
            if (_Iso2D == null)
                return;

            // Vector2 GUIPoint = EditorGUIUtility.GUIToScreenPoint(new Vector2(_rt.x, _rt.y));
            // Rect __rt = new Rect(_rt.xMin + GUIPoint.x, _rt.yMin + GUIPoint.y, _rt.xMax + GUIPoint.x, _rt.yMax + GUIPoint.y);

            // EditorGUI.DrawRect(__rt, Color.gray);
            EditorGUILayout.BeginVertical();
            //GUILayout.BeginArea(_rt);
            EditorGUI.indentLevel = 0;
            
            EditorGUILayout.LabelField("[Sprite]", EditorStyles.boldLabel, GUILayout.MaxWidth(_fMaxWidth));
            GUILayoutOption[] _options = new GUILayoutOption[]{
                    GUILayout.MinWidth(20), GUILayout.Width(_fMaxWidth),
                    GUILayout.MinHeight(20), GUILayout.Height(_fMaxWidth)};
            DrawSideSprite(EditorGUILayout.GetControlRect(_options), _Iso2D, false, false);
            EditorGUILayout.Separator();

            Util.CustomEditorGUI.ShowPackInfo(_Iso2D.sprr.sprite, GUILayout.MaxWidth(_fMaxWidth));
            EditorGUILayout.Separator();
            //GUILayout.EndArea();
            EditorGUILayout.EndVertical();
        }
        static void AttachmentDraw(SerializedProperty attachment)
        {
            if (attachment == null)
                return;

            SerializedProperty Iso2DObj = attachment.FindPropertyRelative("Iso2DObj");
            SerializedProperty indentLevel = attachment.FindPropertyRelative("indentLevel");
   
            int indentLVBackup = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1; //indentLevel.intValue;            
            if (Iso2DObj != null && Iso2DObj.objectReferenceValue != null)
                Iso2DDrawField(Iso2DObj);
            
            EditorGUI.indentLevel = indentLVBackup;            
        }
        static void AttachmentListDraw(SerializedProperty attachedHierarchy)
        {
            if (attachedHierarchy == null)
                return;

            SerializedProperty Iso2DObj = attachedHierarchy.FindPropertyRelative("Iso2DObj");
            SerializedProperty bFoldout = attachedHierarchy.FindPropertyRelative("bFoldout");
            SerializedProperty childList = attachedHierarchy.FindPropertyRelative("childList");

            if (Iso2DObj != null && Iso2DObj.objectReferenceValue != null)
                Iso2DDrawField(Iso2DObj);
            
            EditorGUI.indentLevel++;
            if (childList.arraySize > 0)
            {
                if (bFoldout.boolValue = EditorGUILayout.Foldout(bFoldout.boolValue, "Attchment List", true))
                {
                    for (int i = 0 ; i < childList.arraySize; ++i)  
                    {
                        using (new EditorGUIIndentLevelScope())
                        {
                            AttachmentDraw(childList.FindPropertyRelative(string.Format("Array.data[{0}]", i)));
                        }
                    }
                }
            }
        }
        public static void AttachmentHierarchyField(SerializedObject _root, string _dataPath = "_attachedList")
        {
            SerializedProperty attachedHierarchy = _root.FindProperty(_dataPath);
            AttachmentListDraw(attachedHierarchy);
        }
        public static void GridCoordinatesField(SerializedObject _boject)
        {
            GridCoordinates _gc = (GridCoordinates)(_boject.targetObject);
            Vector3 _gridXYZ = _gc._xyz;

            float _fWidth = EditorGUIUtility.currentViewWidth * 0.475f;
            float _mfWidth = _fWidth * 0.95f;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    Util.CustomEditorGUI.NewParagraph("[Grid Coordinates]", GUILayout.MaxWidth(_mfWidth * 0.75f));
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.ToggleLeft(
                            new GUIContent("Snap To Grid", "Snap to Grid Coordinates"), 
                            !_gc.bSnapFree, GUILayout.MaxWidth(_mfWidth * 0.75f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_gc, "Snap Free");
                        _gc.bSnapFree = !_gc.bSnapFree;
                        _gc.Apply_SnapToGrid();
                        EditorUtility.SetDirty(_gc);
                    }

                    if (!_gc.bSnapFree)
                    {
                        EditorGUILayout.LabelField(string.Format("F{1:0}_({0:0}, {2:0})",
                            _gridXYZ.x, _gridXYZ.y, _gridXYZ.z), GUILayout.MaxWidth(_mfWidth * 0.75f));
                        EditorGUILayout.Separator();
                    }
                }

                using (new EditorGUILayout.VerticalScope(GUILayout.Width(_fWidth * 0.4f)))
                {
                    Util.CustomEditorGUI.NewParagraph("[Transform Position]", GUILayout.MaxWidth(_mfWidth * 0.9f));
                    EditorGUILayout.LabelField("Global" + _gc.transform.position, GUILayout.MaxWidth(_mfWidth * 0.9f));
                    EditorGUILayout.LabelField("Local" + _gc.transform.localPosition, GUILayout.MaxWidth(_mfWidth * 0.9f));
                }
            }
        }     

        public static void Iso2DSelector(GameObject _target)
        {
            EditorGUILayout.ObjectField(_target, typeof(Iso2DObject), allowSceneObjects:true);
        }

        static void Undo_Iso2DSpriteField(Iso2DObject _obj, Color _color)
        {
            using (new GUIBackgroundColorScope(_color))
            {
                using (var result = new EditorGUI.ChangeCheckScope())
                {             
                    Sprite _newSprite = (Sprite)EditorGUILayout.ObjectField(_obj.sprr.sprite, 
                        typeof(Sprite), allowSceneObjects: false);
                    if (result.changed)
                    {
                        _obj.ChangeSprite(_newSprite);
                        EditorUtility.SetDirty(Selection.activeObject);
                    }
                }
            }
        }

        static void Undo_Iso2DSpriteField(Sprite _origin, List<Iso2DObject> _objs, Color _color, params GUILayoutOption[] _options)
        {
            using (new GUIBackgroundColorScope(_color))
            {
                using (var result = new EditorGUI.ChangeCheckScope())
                {             
                    Sprite _newSprite = (Sprite)EditorGUILayout.ObjectField(_origin, 
                        typeof(Sprite), allowSceneObjects: false, options:_options);
                    if (result.changed)
                    {
                        Undo_Iso2DSprite(_objs, _newSprite);
                        EditorUtility.SetDirty(Selection.activeObject);
                    }
                }
            }
        }
        public static void Undo_Iso2DSpriteField(Rect _rect, Sprite _origin, List<Iso2DObject> _objs, Color _color)
        {
            _rect.x += 5;
            using (new GUIBackgroundColorScope(_color))
            {
                using (var result = new EditorGUI.ChangeCheckScope())
                {
                    Sprite _newSprite = (Sprite) EditorGUI.ObjectField(_rect, _origin,
                        typeof(Sprite), allowSceneObjects: false);
                    if (result.changed)
                    {
                        Undo_Iso2DSprite(_objs, _newSprite);
                        EditorUtility.SetDirty(Selection.activeObject);
                    }
                }
            }
        }
        public static void Undo_Iso2DSprite(List<Iso2DObject> _objs, Sprite _newSprite)
        {
            foreach(Iso2DObject _obj in _objs)
            {
                if (_obj != null)
                    _obj.ChangeSprite(_newSprite);
            }
        }

        public static void DrawSideSprite(Rect _FullRect, Iso2DObject _Iso2D, 
            bool _bSquare, bool _bSimpleDraw)
        {
            if (_Iso2D == null || _Iso2D.sprr.sprite == null)
                return;

            Rect _rt = _FullRect.Divid_TileSide(_Iso2D._Type);
            if (_Iso2D.transform.localScale.x < 0)
            {
                _rt.x += _rt.width;
                _rt.width *= -1f;
            }
            if (_Iso2D.transform.localScale.y < 0)
            {
                _rt.y += _rt.height;
                _rt.height *= -1f;
            }
            Util.CustomEditorGUI.DrawSprite(_rt, _Iso2D.sprr.sprite, _Iso2D.sprr.color , _bSquare, _bSimpleDraw);
        }
        public static void DrawSideSprite(Rect _FullRect, Sprite _sprite, Color _color, Iso2DObject.Type _side, 
            bool _bSquare, bool _bSimpleDraw)
        {
            // Rect _rt = _sprite.textureRectOffset.Equals(Vector2.zero) 
            //     ? _FullRect.Divid_TileSide(_side) : _FullRect;
            Rect _rt = _FullRect.Divid_TileSide(_side);
            Util.CustomEditorGUI.DrawSprite(_rt, _sprite, _color, _bSquare, _bSimpleDraw);
        }
        
        public static bool Undo_TileDeco_Instantiate_DoAll(GameObject _prefab, string _actionName, bool _buttonAction)
        {
            if (!_buttonAction || GUILayout.Button(_actionName))
            {
                IsoTile _tile;
                GameObject _go;
                RegularCollider _rc;
                for (int i = 0; i < Selection.gameObjects.Length; ++i)
                {
                    _tile = Selection.gameObjects[i].GetComponent<IsoTile>();
                    if (_tile != null)
                    {
                        _go = Undo_Instantiate(_prefab, Selection.gameObjects[i].transform, _actionName);
                        if (_go != null && (_rc = _go.GetComponent<RegularCollider>()) != null)
                        {
                            _rc.Toggle_UseGridTileScale(_tile.bAutoFit_ColliderScale);
                        }
                    }
                }
                return true;
            }
            return false;
        }

#endif
	}
}
