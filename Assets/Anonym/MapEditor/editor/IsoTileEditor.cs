using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Sprites;
using System.Linq;
using System.Reflection;

namespace Anonym.Isometric
{
    using Anonym.Util;

    [CustomEditor(typeof(IsoTile))]
    [CanEditMultipleObjects]
    public class IsoTileEditor : Editor
    {
        enum EditorMode
        {
            Extrude,
            Select,
        }
        EditorMode _E_Mode = EditorMode.Extrude;
        int selectedToolbar;
        //IsoTileBulk _bulk;
        IsoTile _tile_Inspector;
        IsoTile _tile_Scene;        
        IsoTile refTile;

        bool bPrefab = false;        

        bool bRightEdge = false;
        bool bLeftEdge = false;
        bool bUpEdge = false;
        bool bDownEdge = false;
        bool bFowardEdge = false;
        bool bBackEdge = false;
        float fHandleSize = 1;
        float fHandleOffsetSize = 1.5f;
        Vector3 handlePosition_Centor = Vector3.zero;

        bool bSingleCtlr = true;
        static bool bFrameSelected = false;
        static bool bWithAttachment = false;
        SelectionType selectionType = SelectionType.NewTile;
        static GUIStyle screenInfoFontStyle = null;

        PreviewRenderUtility previewRenderUtility;
        Camera PreviewCam { get
            {
#if UNITY_2017_1_OR_NEWER
                return previewRenderUtility.camera;
#else
                return previewRenderUtility.m_Camera;
#endif
            }
        }
        GameObject previewObject;

        void update_InspectorTile()
        {
            if (_tile_Inspector == null || _tile_Inspector.gameObject != Selection.activeGameObject)
            {
                _tile_Inspector = Selection.activeGameObject == null ? null : 
                    Selection.activeGameObject.GetComponent<IsoTile>();
                childInfoUpdate();
            }
        }
        void update_SceneTile()
        {
            if (_tile_Scene == null || _tile_Scene.gameObject != Selection.activeGameObject)
            {
                _tile_Scene = Selection.activeGameObject == null ? null : 
                    Selection.activeGameObject.GetComponent<IsoTile>();
                edgeUpdate();   
            }
        }
        void childInfoUpdate()
        {
            if (_tile_Scene != null)
                _tile_Scene.Update_AttachmentList();
            //EditorUtility.SetDirty(target);
        }
        bool undoredo()
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                childInfoUpdate();
                Repaint();
                return true;
            }
            return false;
        }
        void edgeUpdate()
        {
            if (_tile_Scene != null)
            {
                bRightEdge = _tile_Scene.IsLastTile(Vector3.right);
                bLeftEdge = _tile_Scene.IsLastTile(Vector3.left);
                bUpEdge = _tile_Scene.IsLastTile(Vector3.up);
                bDownEdge = _tile_Scene.IsLastTile(Vector3.down);
                bFowardEdge = _tile_Scene.IsLastTile(Vector3.forward);
                bBackEdge = _tile_Scene.IsLastTile(Vector3.back);
            }
        }

        void OnEnable()
        {
            if (target == null || Selection.activeGameObject == null)
                return;

            if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;
            
            update_InspectorTile();

            if (screenInfoFontStyle == null)
            {
                screenInfoFontStyle = new GUIStyle();
                screenInfoFontStyle.normal.textColor = Color.green;
                screenInfoFontStyle.fontStyle = FontStyle.Bold;
                screenInfoFontStyle.fontSize = 15;
            }

            if (SceneView.lastActiveSceneView != null && bFrameSelected)
            {
                SceneView.lastActiveSceneView.FrameSelected(false);
            }
        }
        void OnDisable ()
        {
            clearPreviewInstance();
        }
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                clearPreviewInstance();
        }
        public override void OnInspectorGUI()
        {
            if (bPrefab)
            {
                base.DrawDefaultInspector();
                return;
            }

            update_InspectorTile();

            if (_tile_Inspector == null)
                return; 
                
            if (undoredo())
                return;

            serializedObject.Update();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("[Tile Control]", EditorStyles.boldLabel);
            Inspector_Tile();

            Inspector_Side();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("[Attachment Control]", EditorStyles.boldLabel);
            Inspector_Attached();

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            if (bPrefab)
                return;

            if (((IsoTile)target).gameObject != Selection.activeGameObject)
                return; 

            if (Event.current.type == EventType.Layout)
            {
                update_SceneTile();

                Handles.BeginGUI();
                Rect _windowRT = new Rect(Screen.width - 250, Screen.height - 120, 240, 95);
                int windowID = EditorGUIUtility.GetControlID(FocusType.Passive, _windowRT);
                GUILayout.Window(windowID, _windowRT, ScreenInfoWindow,
                    _E_Mode == EditorMode.Extrude ?
                    "Extrude Mode" : "Select Mode");
                Handles.EndGUI();

                if (_tile_Scene.coordinates.bChangedforEditor)
                {
                    edgeUpdate();
                    _tile_Scene.coordinates.bChangedforEditor = false;
                }
            }

            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint
                || Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseMove
                || Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
            {
                HandleDraw();

                Vector3 position = _tile_Scene.transform.position + fHandleSize * (new Vector3(1f, -0.5f, 0));
                string posString = _tile_Scene.transform.position.ToString();

                Handles.color = Color.gray;
                Handles.DrawLine(_tile_Scene.transform.position, position);
                Handles.Label(position, _tile_Scene.name + "\n" + posString, screenInfoFontStyle);
            }
            return;
        }
        void HandleDraw()
        {
            handle_Init();

            if (_E_Mode == EditorMode.Extrude)
            {
                handle_Editor(Vector3.right, Handles.xAxisColor, bRightEdge);
                handle_Editor(Vector3.left, Handles.xAxisColor, bLeftEdge);

                handle_Editor(Vector3.up, Handles.yAxisColor, bUpEdge);
                handle_Editor(Vector3.down, Handles.yAxisColor, bDownEdge);

                handle_Editor(Vector3.forward, Color.cyan, bFowardEdge);
                handle_Editor(Vector3.back, Color.cyan, bBackEdge);
            }
        }
        void ScreenInfoWindow(int id)
        {
            if (_E_Mode == EditorMode.Extrude)
            {
                bFrameSelected = GUILayout.Toggle(bFrameSelected, "Auto Frame Selected");
                bWithAttachment = GUILayout.Toggle(bWithAttachment, "Extrude with Attachment");
                GUILayout.Toggle(selectionType == SelectionType.AllTile,
                    selectionType == SelectionType.AllTile ? "Shift On : Select with last Tile(Slow)" : "Shift Off : Select only new Tile(Fast)");
                GUILayout.Toggle(!bSingleCtlr, !bSingleCtlr ? "Ctlr On : Apply to Selection(Slow)" : "Ctlr Off : Apply to a Tile(Fast)");
                // if (GUILayout.Button("Selection Mode"))
                //     _E_Mode = EditorMode.Select;
            }
            else
            {
                if (GUILayout.Button("Extrude Mode"))
                    _E_Mode = EditorMode.Extrude;
            }
        }
        void handle_Init()
        {
            fHandleSize = HandleUtility.GetHandleSize(_tile_Scene.transform.position);
            handlePosition_Centor = _tile_Scene.transform.position;
            bSingleCtlr = !Event.current.control;
            selectionType = !Event.current.shift ? SelectionType.NewTile : SelectionType.AllTile;
        }
        void handle_Editor(Vector3 _direction, Color _color, bool _active)
        {
            Vector3 handlePosition = handlePosition_Centor + fHandleSize * _direction * fHandleOffsetSize;
            if (_tile_Scene != null)// && _tile.coordinates.IsLastTile(_direction))
            {
                if (_active)
                {
                    Handles.color = _color;
                    Handles.DrawLine(handlePosition_Centor, handlePosition_Centor + fHandleSize * _direction);
                    EditorGUI.BeginChangeCheck();
                    if (bSingleCtlr)
                        Handles.FreeMoveHandle(handlePosition,
                            Quaternion.identity, fHandleSize * 0.15f, Vector3.zero, Handles.RectangleHandleCap);
                    else
                        Handles.FreeMoveHandle(handlePosition,
                            Quaternion.identity, fHandleSize * 0.25f, Vector3.zero, Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        float fDragDistance = HandleUtility.CalcLineTranslation(
                            HandleUtility.WorldToGUIPoint(handlePosition),
                            Event.current.mousePosition, handlePosition, _direction);

                        float fHurdleDistance = Vector3.Scale(_direction,
                            _tile_Scene.coordinates.grid.GridInterval).magnitude;

                        _tile_Scene.Bulk.Do_With_SensorOff(() =>
                        {
                            if (fDragDistance >= fHurdleDistance)
                                handle_Extrude(_direction, selectionType, bSingleCtlr, bWithAttachment);
                            else if (fDragDistance <= -fHurdleDistance)
                                handle_Press(-_direction, bSingleCtlr);
                        });
                        _tile_Scene.Bulk.Update_ChildList();
                    }
                }
                else
                {
                    Handles.color = _color / 1.5f;
                    Handles.FreeMoveHandle(handlePosition,
                        Quaternion.LookRotation(_direction), fHandleSize * 0.1f, Vector3.one, Handles.DotHandleCap);
                }
            }
        }

        void handle_Press(Vector3 _direction, bool _singleAction)
        {
            List<GameObject> _removeTileObjects = new List<GameObject>();
            List<IsoTile> _moveTileObjects = new List<IsoTile>();
            
            if (_singleAction)
            {
                tile_Press(_tile_Scene, _direction, ref _moveTileObjects, ref _removeTileObjects);
            }
            else
            {
                for (int i = 0 ; i < Selection.gameObjects.Length; ++i)
                {
                    tile_Press(Selection.gameObjects[i].GetComponent<IsoTile>(),
                        _direction, ref _moveTileObjects, ref _removeTileObjects);
                }
            }

            for( int i = 0 ; i < _moveTileObjects.Count; ++i)
                _moveTileObjects[i].coordinates.Translate(_direction);
                
            if (_removeTileObjects.Count > 0)
            {
                Selection.objects = Selection.gameObjects.Except(_removeTileObjects.ToArray()).ToArray();
                for (int i = 0 ; i < _removeTileObjects.Count; ++i)
                    Undo.DestroyObjectImmediate(_removeTileObjects[i]);
            }
        }
        void tile_Press(IsoTile _tile, Vector3 _direction, ref List<IsoTile> _moveList, ref List<GameObject> _removeList)
        {
            if (_tile.IsLastTile(-_direction) && !_tile.IsLastTile(_direction))
            {
                IsoTile _removeTile = _tile.NextTile(_direction);
                _moveList.Add(_tile);
                if (_removeTile != null)
                    _removeList.Add(_removeTile.gameObject);
            }
        }

        void handle_Extrude(Vector3 _direction, SelectionType _selectionType, bool _singleAction, bool _bWithAttachment)
        {
            Vector3 _lastPos = _tile_Scene.coordinates._xyz;
            List<GameObject> _newTileObjects = new List<GameObject>();
            IsoTile _newTile = null;
            if (_singleAction)
            {
                if ((_newTile = tile_Extrude(_tile_Scene, _direction, _bWithAttachment)) != null)
                    _newTileObjects.Add(_newTile.gameObject);
            }
            else
            {
                foreach (var obj in Selection.gameObjects)
                {
                    if ((_newTile = tile_Extrude(obj.GetComponent<IsoTile>(), _direction, _bWithAttachment)) != null)
                        _newTileObjects.Add(_newTile.gameObject);
                }
            }
            if (_newTileObjects.Count > 0)
            {
                Undo.IncrementCurrentGroup();
                switch (_selectionType)
                {
                    case SelectionType.LastTile:
                        Selection.objects = _newTileObjects.ToArray();
                        break;
                    case SelectionType.NewTile:                        
                        break;
                    case SelectionType.AllTile:
                        Selection.objects = Selection.objects.Concat(_newTileObjects.ToArray()).ToArray();
                        break;
                }
                edgeUpdate();
            }
        }

        IsoTile tile_Extrude(IsoTile _tile, Vector3 _direction, bool _bWithAttachment)
        {
            if (_tile.IsLastTile(_direction))
            {
                _tile.coordinates.Translate(_direction);
                if (!_tile.IsAccumulatedTile_Collider(-_direction))
                {
                    return _tile.Extrude(-_direction, false, _bWithAttachment);
                }
            }
            return null;
        }

        void Inspector_Tile()
        {    
            using (new GUIBackgroundColorScope(Color.cyan))
            {
                refTile = (IsoTile)EditorGUILayout.ObjectField(refTile, typeof(IsoTile), allowSceneObjects: true);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                {
                    using (new GUIBackgroundColorScope(Color.cyan))
                    {
                        //refTile = (IsoTile)EditorGUI.ObjectField(GUILayoutUtility.GetRect(70, 40), refTile, typeof(IsoTile), allowSceneObjects:true);
                        EditorGUI.BeginDisabledGroup(refTile == null);
                        if (GUILayout.Button("Copycat"))
                        {
                            for (int i = 0; i < Selection.gameObjects.Length; ++i)
                            {
                                Selection.gameObjects[i].GetComponent<IsoTile>().Copycat(refTile);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                {
                    if (GUILayout.Button("Duplicate"))
                    {
                        List<GameObject> _selection = new List<GameObject>();
                        for (int i = 0; i < Selection.gameObjects.Length; ++i)
                        {
                            _selection.Add((
                                Selection.gameObjects[i].GetComponent<IsoTile>()).Duplicate().gameObject);
                        }
                        Undo.RecordObjects(Selection.objects, "IsoTile:Dulicate");
                        Selection.objects = _selection.ToArray();
                    }
                }
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightMagenta))
                {
                    if (GUILayout.Button("Select (B)ulk"))
                    {
                        Selection.activeGameObject = _tile_Inspector.Bulk.gameObject;
                    }
                }
            }
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(string.Format(
                "Grid's Tile Scale : Width({0:0.00}), Height({1:0.00})",
                        _tile_Inspector.coordinates.grid.TileSize.x ,
                        _tile_Inspector.coordinates.grid.TileSize.y));

            float fWidth = EditorGUIUtility.currentViewWidth * 0.25f;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Auto Fit", GUILayout.MaxWidth(fWidth));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ToggleLeft("Collider",
                    _tile_Inspector.bAutoFit_ColliderScale,
                    GUILayout.MaxWidth(fWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    bool _bTmp = !_tile_Inspector.bAutoFit_ColliderScale;
                    for (int i = 0; i < Selection.gameObjects.Length; ++i)
                    {
                        if (Selection.gameObjects[i] != null)
                        {
                            IsoTile _tTmp = Selection.gameObjects[i].GetComponent<IsoTile>();
                            if (_tTmp != null)
                            {
                                Undo.RecordObject(_tTmp, "Use AutoFit Collider");
                                _tTmp.bAutoFit_ColliderScale = _bTmp;
                                _tTmp.GetComponent<RegularCollider>().Toggle_UseGridTileScale(_bTmp);
                            }
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ToggleLeft("Sprite", 
                    _tile_Inspector.bAutoFit_SpriteSize,
                    GUILayout.MaxWidth(fWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    bool _bTmp = !_tile_Inspector.bAutoFit_SpriteSize;
                    for (int i = 0; i < Selection.gameObjects.Length; ++i)
                    {
                        if (Selection.gameObjects[i] != null)
                        {
                            IsoTile _tTmp = Selection.gameObjects[i].GetComponent<IsoTile>();
                            if (_tTmp != null)
                            {
                                Undo.RecordObject(_tTmp, "Use AutoFit Iso2DObject");
                                _tTmp.bAutoFit_SpriteSize = _bTmp;
                                _tTmp.Update_Attached_Iso2DScale();
                            }
                        }
                    }
                }
            }
            EditorGUILayout.Separator();
            EditorGUI.indentLevel = 0;
        }

        void Inspector_Side()
        {
            EditorGUILayout.Separator();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[Side Control]", EditorStyles.boldLabel, GUILayout.MaxWidth(120));
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightBlue))
                {
                    EditorGUI.BeginChangeCheck();
                    bool _bUnionMode = 1 == EditorGUILayout.Popup(
                        _tile_Inspector.GetComponent<IsoTile>().IsUnionCube()
                         ? 1 : 0, new string[] { "Side Mode", " Union Mode" });
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach(GameObject _go in Selection.objects)
                        {
                            if (_go == null)
                                continue;
                            IsoTile _t = _go.GetComponent<IsoTile>();
                            if (_t != null && _t.IsUnionCube() != _bUnionMode)
                            {
                                _t.Reset_SideObject(_bUnionMode);
                            }
                        }                        
                    }
                }
            }
            EditorGUILayout.Separator();

            using (new EditorGUILayout.HorizontalScope())
            {
                float fWidth = EditorGUIUtility.currentViewWidth / 3f;
                Rect _Rect = EditorGUILayout.GetControlRect(
                    new GUILayoutOption[] { GUILayout.Height(fWidth), GUILayout.ExpandWidth(true) });

                Rect[] _DescRectDivision;
                Rect[] _SubRects;

                if (_tile_Inspector.IsUnionCube())
                {
                    Iso2DObject _Union = _tile_Inspector.GetSideObject(Iso2DObject.Type.Side_Union);
                    if (_Union != null)
                    {
                        SpriteRenderer sprr = _Union.sprr;                        
                        _DescRectDivision = _Rect.Division(
                            new float[] { 0.25f/8f, 2.75f/8f, 1/8f, 3f/8f, 1f/8f}, null);
                        _SubRects = _DescRectDivision[1].Division(null, new float[] { 0.05f, 0.35f, 0.6f });
                        Union_Field(_SubRects[1], "Union Sprite", _Union, Handles.selectedColor);
                        Util.CustomEditorGUI.DrawSprite(_SubRects[2].Division(new float[] { 0.25f, 0.5f }, null)[1],
                            IsoMap.instance.IsoTile_Union_OutlineImage, Color.clear, true, false);
                        CustomEditorGUI.DrawSideSprite(_DescRectDivision[3], _Union, false, false);
                    }
                }
                else
                {
                    _DescRectDivision = _Rect.Division(
                        new float[] { 0.5f/8f, 2.5f/8f, 1f/8f, 3/8f, 1f/8f }, null);

                    _SubRects = _DescRectDivision[1].Division(null, 
                        new float[] {2/10f, 2/10f, 2/10f, 2/10f, 2/10f});
                    
                    _Rect = _DescRectDivision[3].Division(new float[]{0.1f, 0.8f}, new float[]{0.2f, 0.8f})[3];
                    Util.CustomEditorGUI.DrawSprite(_Rect, IsoMap.instance.IsoTile_Side_OutlineImage, Color.clear, false, false);

                    Iso2DObject _Iso2D_x = Side_Field(_SubRects[0], Iso2DObject.Type.Side_X,
                        Handles.xAxisColor, IsoMap.instance.Side_X_Prefab);
                    Iso2DObject _Iso2D_y = Side_Field(_SubRects[2], Iso2DObject.Type.Side_Y, 
                        Handles.yAxisColor, IsoMap.instance.Side_Y_Prefab);
                    Iso2DObject _Iso2D_z = Side_Field(_SubRects[4], Iso2DObject.Type.Side_Z, 
                        Handles.zAxisColor, IsoMap.instance.Side_Z_Prefab);

                    CustomEditorGUI.DrawSideSprite(_Rect, _Iso2D_y, false, false);
                    CustomEditorGUI.DrawSideSprite(_Rect, _Iso2D_x, false, false);
                    CustomEditorGUI.DrawSideSprite(_Rect, _Iso2D_z, false, false);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        Iso2DObject Side_Field(Rect _Rect, Iso2DObject.Type _sideType, Color _color, GameObject _prefab)
        {
            Iso2DObject _obj = _tile_Inspector.GetSideObject(_sideType);
            bool _bToggle = _obj != null;
            _Rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            _bToggle = EditorGUI.ToggleLeft(_Rect, _sideType.ToString(), _bToggle);
            if (EditorGUI.EndChangeCheck())
            {
                for(int i = 0 ; i < Selection.gameObjects.Length; ++i)
                {
                    if (Selection.gameObjects[i] == null)
                        continue;
                    IsoTile _t = Selection.gameObjects[i].GetComponent<IsoTile>();
                    if (_t != null && !_t.IsUnionCube())
                        _t.Toggle_Side(_bToggle, _sideType);
                }
            }

            List<Iso2DObject> _lookupList = Iso2DObject.GetSideListOfTileSelection(_sideType);
            
            if (_bToggle && _lookupList.Count > 0)
            {
                if (_obj == null)
                    _obj = _tile_Scene.GetSideObject(_sideType);
                _Rect.y += _Rect.height;
                CustomEditorGUI.Undo_Iso2DSpriteField(_Rect, _obj.sprr.sprite, _lookupList, _color);
            }
            return _obj;
        }
        
        void Union_Field(Rect _Rect, string _MSG, Iso2DObject _obj, Color _color)
        {
            _Rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(_Rect, _MSG);
            _Rect.y += _Rect.height;
            CustomEditorGUI.Undo_Iso2DSpriteField(_Rect, _obj.sprr.sprite, 
                Iso2DObject.GetSideListOfTileSelection(Iso2DObject.Type.Side_Union), _color);
        }

        void Inspector_Attached()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool bUpdate = false;
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_Overlay))
                {
                    bUpdate = CustomEditorGUI.Undo_TileDeco_Instantiate_DoAll(IsoMap.instance.OverlayPrefab, "Create Overlay", true);
                }

                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_Obstacle))
                {
                    bUpdate = CustomEditorGUI.Undo_TileDeco_Instantiate_DoAll(IsoMap.instance.ObstaclePrefab, "Create Obstacle", true);
                }

                if (bUpdate)
                    childInfoUpdate();
            }

            CustomEditorGUI.AttachmentHierarchyField(serializedObject);
        }

        // for preview
        public override bool HasPreviewGUI ()
        {
            if (bPrefab)
                return false;
            
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        public override GUIContent GetPreviewTitle ()
        {
            return new GUIContent ("Iso2DObject");
        }

        public override void OnPreviewSettings ()
        {
            if (bPrefab)
                return;

            GUIStyle preLabel = new GUIStyle ("preLabel");            

            GUILayout.Label (target.name, preLabel);
            previewInit();         
        }

        public override void OnPreviewGUI (Rect r, GUIStyle background)
        {
            if (bPrefab || previewRenderUtility == null)
                return;

            if (!(r.width > 0 && r.height > 0))
                return;

            previewRenderUtility.BeginPreview (r, background);
            var drag = Vector2.zero;
            //드래그 할 때의 마우스 이동량을 얻어옵니다
            if (Event.current.type == EventType.MouseDrag 
                && r.Contains(Event.current.mousePosition)) {
                drag = Event.current.delta;
            }
            var previewCamera = PreviewCam;
            
            if (drag != Vector2.zero)
            {
                drag *= 0.02f;
                previewCamera.transform.Translate(-drag.x, drag.y, 0f);
            }        
            
            if (IsEditorMode && previewObject != null)
            {               
                previewObject.gameObject.SetActive (true);
                previewCamera.Render ();
                previewObject.gameObject.SetActive (false);                 
            }
            else
            {
                previewCamera.Render ();
            }

            previewRenderUtility.EndAndDrawPreview (r);

            if (drag != Vector2.zero)
                Repaint ();
        }
        bool IsEditorMode
        {
            get{
                return !EditorApplication.isPlayingOrWillChangePlaymode 
                    && !EditorApplication.isCompiling
                    && !EditorApplication.isPlaying
                    && !EditorApplication.isUpdating;
            }
        }
        void previewInit()
        {
            if (bPrefab)
                return;

            if (previewRenderUtility != null)
                return;
            
            previewRenderUtility = new PreviewRenderUtility (true);
            Camera previewCam = PreviewCam;
            previewCam.backgroundColor = Color.black;
            // previewCam.projectionMatrix = Camera.main.projectionMatrix;
            previewCam.orthographic = true;
            previewCam.orthographicSize = Camera.main.orthographicSize * 0.5f;
            previewCam.farClipPlane = 100;
            previewCam.nearClipPlane = 0.01f;
            GameObject targetObj = _tile_Inspector.gameObject;
            Bounds bounds;       
            
            if (IsEditorMode && _tile_Inspector.gameObject.activeSelf)
            {   
                IsoTile _isoTile = Editor.Instantiate (_tile_Inspector);
                previewObject = _isoTile.gameObject;
                previewObject.transform.position = Vector3.zero;
                _isoTile.Copycat(_tile_Inspector.GetComponent<IsoTile>(), true, false);

                previewObject.hideFlags = HideFlags.HideAndDontSave;
                previewObject.SetActive (false);

                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var propInfo = typeof(Camera).GetProperty ("PreviewCullingLayer", flags);
                int previewLayer = (int)propInfo.GetValue (null, new object[0]);
                
                previewObject.layer = previewLayer;
                foreach (Transform _transform in previewObject.transform.GetComponentsInChildren<Transform>()) {
                    _transform.gameObject.layer = previewLayer;
                }
                previewCam.cullingMask = 1 << previewLayer;

                targetObj = previewObject;
                
                // callbacks.Add(clearPreviewInstance);
                // EditorApplication.playmodeStateChanged += clearPreviewInstance;
            }

            bounds = new Bounds (targetObj.transform.position, Vector3.zero);
            foreach (var renderer in targetObj.GetComponentsInChildren<Renderer>()) {
                bounds.Encapsulate (renderer.bounds);
            }

            float distance = Util.CustomEditorGUI.FrameBounds(previewCam, bounds);
            previewCam.transform.eulerAngles = IsoMap.instance.TileAngle;
            previewCam.transform.position = bounds.center - previewCam.transform.forward * distance;
        }
        void clearPreviewInstance()
        {
            if (previewObject != null)
            {
                DestroyImmediate(previewObject.gameObject, true);
                previewObject = null;
                // callbacks.ForEach(r => EditorApplication.playmodeStateChanged -= r);
                // callbacks.Clear();
            }               
            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup ();
                previewRenderUtility = null;
            }
        }

    }
}