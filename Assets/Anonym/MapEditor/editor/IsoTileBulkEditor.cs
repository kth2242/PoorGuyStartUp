using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Anonym.Isometric
{
    using Util;

    [CustomEditor(typeof(IsoTileBulk))]
    [CanEditMultipleObjects]
    public class IsoTileBulkEditor : Editor
    {
        IsoTileBulk _bulk;
        bool bFoldout = true;
        bool bPrefab = false;
        Grid _bulkGrid;
        Vector2 tileListScroll;
        string filterMsg = "Filter string(Seperator{',', ' '})";
        string filterString;
        string[] filters;
        SerializedProperty _SizeXZ;
        SerializedProperty _RefTile;
        List<int> filteredChildIndexList = new List<int>();
        List<IsoTileBulk> connectedBulkList = new List<IsoTileBulk>();
        bool undoredo()
        {
            if (Event.current.commandName == "UndoRedoPerformed")
            {
                refresh_Child(true);
                return true;
            }
            return false;
        }
        void refresh_ConnectedBulk()
        {
            connectedBulkList.Clear();
            connectedBulkList.AddRange(_bulk.GetComponentsInParent<IsoTileBulk>());
            connectedBulkList.AddRange(_bulk.GetComponentsInChildren<IsoTileBulk>());
            connectedBulkList.RemoveAll(r => r == _bulk);
        }
        void refresh_Child(bool bForceSPUpdate = false)
        {
            tileListScroll = Vector2.zero;
            filterString = filterMsg;
            if (bForceSPUpdate)
                _bulk.Update_ChildList();
            refresh_FilteredChild(bForceSPUpdate);
        }

        void refresh_FilteredChild(bool bForceSPUpdate = false)
        {
            if (bForceSPUpdate)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            filteredChildIndexList.Clear();
            bool bAll = filterString.Equals(filterMsg);
            if (!bAll)
                filters = filterString.Split(',', ' ');

            for (int i = 0; i < _bulk._attachedList.Count; ++i)
            {
                if (bAll || SPNameFilterCheck(_bulk._attachedList[i].name, ref filters))
                {   
                    filteredChildIndexList.Add(i);
                }
            }
        }

        void OnEnable()
        {
            if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;
            _bulk = (IsoTileBulk)target;
            _SizeXZ = serializedObject.FindProperty("SizeXZ");
            _RefTile = serializedObject.FindProperty("_referenceTile");
            _bulkGrid = _bulk.gameObject.GetComponent<Grid>();
            refresh_Child(true);
            refresh_ConnectedBulk();
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

            ShowSelector();

            EditorGUILayout.LabelField("[Bulk Control]", EditorStyles.boldLabel);
            ShowResize();
            ShowBulkControl();
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("[Tile control]", EditorStyles.boldLabel);
            ShowTileControl();
            ShowTileFilter();
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
			if (PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;

            if (_bulkGrid.IsInheritGrid)
                return;
                
            int iOddCount = 9;
            Vector3 _centor = _bulkGrid.transform.position;
            for (int i = 0 ; i < iOddCount; ++i)
            {
                Handles.color = Color.red;
                Handles.CircleHandleCap(iOddCount + i * 3, _centor
                    + new Vector3((i - (iOddCount - 1)/2) * _bulkGrid.GridInterval.x, 0, 0), 
                    SceneView.currentDrawingSceneView.camera.transform.rotation, 0.05f, EventType.Repaint);
                Handles.color = Color.green;
                Handles.CircleHandleCap(iOddCount + i * 3 + 1, _centor
                    + new Vector3(0, (i - (iOddCount - 1)/2) * _bulkGrid.GridInterval.y, 0), 
                    SceneView.currentDrawingSceneView.camera.transform.rotation, 0.05f, EventType.Repaint);
                Handles.color = Color.blue;
                Handles.CircleHandleCap(iOddCount + i * 3 + 2, _centor
                    + new Vector3(0, 0, (i - (iOddCount - 1)/2) * _bulkGrid.GridInterval.z), 
                    SceneView.currentDrawingSceneView.camera.transform.rotation, 0.05f, EventType.Repaint);
            }
        }

        bool SPNameFilterCheck(string targetName, ref string[] filters)
        {
            for (int i = 0; i < filters.Length; ++i)
            {
                if (!string.IsNullOrEmpty(filters[i]) && !targetName.Contains(filters[i]))
                    return false;
            }
            return true;
        }

        private void ShowAdjustButtons(Rect _rt,
            string sL, string s, string sR,
            System.Action _action_L,
            System.Action _action_B,
            System.Action _action_R)
        {

            Rect[] _rectList = _rt.Division(new float[] { 0.35f, 0.3f, 0.35f }, null);
            bool bChanged = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUI.Button(_rectList[0], sL, EditorStyles.miniButtonLeft))
                {
                    bChanged = true;
                    _action_L();
                }
                if (GUI.Button(_rectList[1], s, EditorStyles.miniButtonMid))
                {
                    bChanged = true;
                    _action_B();
                }
                if (GUI.Button(_rectList[2], sR, EditorStyles.miniButtonRight))
                {
                    bChanged = true;
                    _action_R();
                }
            }

            if (bChanged)
            {
                refresh_Child(true);
                EditorUtility.SetDirty(this);
            }
        }

        private void ShowResizeAxis(Rect _rt, string _name, bool _Axis_XTrue_YFalse)
        {
            Rect _newSize = _SizeXZ.rectValue;
            System.Action<bool, bool, bool, int> buttonAction = (bool bAxis, bool bMax, bool bMin, int i) =>
            {
                if (bAxis)
                {
                    if (bMax)
                        _newSize.xMax += i;
                    if (bMin)
                        _newSize.xMin += -i;
                }
                else
                {
                    if (bMax)
                        _newSize.yMax += i;
                    if (bMin)
                        _newSize.yMin += -i;
                }
                _newSize.xMin = Mathf.Min(0, _newSize.xMin);
                _newSize.xMax = Mathf.Max(0, _newSize.xMax);
                _newSize.yMin = Mathf.Min(0, _newSize.yMin);
                _newSize.yMax = Mathf.Max(0, _newSize.yMax);

                _bulk.Resize(_newSize, false);
            };

            using (new EditorGUILayout.VerticalScope())
            {
                Rect[] _rectList = _rt.Division(null, new float[] { 0.3f, 0.35f, 0.35f });
                EditorGUI.LabelField(_rectList[0], _name, EditorStyles.largeLabel);
                ShowAdjustButtons(_rectList[1], "<", "+", ">",
                    () => { buttonAction(_Axis_XTrue_YFalse, false, true, 1); },
                    () => { buttonAction(_Axis_XTrue_YFalse, true, true, 1); },
                    () => { buttonAction(_Axis_XTrue_YFalse, true, false, 1); });
                ShowAdjustButtons(_rectList[2], ">", "-", "<",
                    () => { buttonAction(_Axis_XTrue_YFalse, false, true, -1); },
                    () => { buttonAction(_Axis_XTrue_YFalse, true, true, -1); },
                    () => { buttonAction(_Axis_XTrue_YFalse, true, false, -1); });
            }
        }
        private void ShowBulkControl()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                {
                    if (GUILayout.Button("Duplicate"))
                    {
                        List<GameObject> _selection = new List<GameObject>();
                        for (int i = 0; i < Selection.gameObjects.Length; ++i)
                        {
                            _selection.Add((
                                Selection.gameObjects[i].GetComponent<IsoTileBulk>()).Duplicate().gameObject);
                        }
                        Undo.RecordObjects(Selection.objects, "IsoTileBulk:Dulicate");
                        Selection.objects = _selection.ToArray();
                    }
                    // if (duplicatedObject != null)
                    //     CustomEditorGUI.Iso2DSelector(duplicatedObject);
                }
            }
        }

        private void SelectTiles()
        {
            List<GameObject> _selectList = new List<GameObject>();
            for (int i = 0; i < filteredChildIndexList.Count; ++i)
                _selectList.Add((_bulk._attachedList[filteredChildIndexList[i]].gameObject));

            Selection.objects = _selectList.ToArray();
            EditorUtility.SetDirty(this);
        }

        private void ShowSelector()
        {
            EditorGUILayout.LabelField("[Object Selector]", EditorStyles.boldLabel);
            Util.CustomEditorGUI.ComSelector<IsoMap>(IsoMap.instance, "GO IsoMap");
            for(int i = 0 ; i < connectedBulkList.Count; ++i)
                Util.CustomEditorGUI.ComSelector<IsoTileBulk>(connectedBulkList[i], "GO Bulk");
            EditorGUILayout.Separator();
        }

        private void ShowResize()
        {
            using (var view = new EditorGUILayout.HorizontalScope(GUILayout.Height(75)))
            {
                Rect[] _rtList = view.rect.Division(new float[] { 0.3f, 0.05f, 0.3f, 0.05f, 0.3f }, null);

                float fSize;

                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightRed))
                {
                    fSize = _SizeXZ.rectValue.width + 1;
                    ShowResizeAxis(_rtList[0], string.Format("Size X({0})", fSize), true);
                }
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightGreen))
                {
                    fSize = _SizeXZ.rectValue.height + 1;
                    ShowResizeAxis(_rtList[2], string.Format("Size Z({0})", fSize), false);
                }
                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightBlue))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _rtList = _rtList[4].Division(null, new float[] { 0.15f, 0.25f, 0.05f, 0.25f, 0.05f, 0.25f });
                        if (GUI.Button(_rtList[1], "No Redundancy"))
                        {
                            _bulk.NoRedundancy();
                            refresh_Child(true);
                        }

                        if (GUI.Button(_rtList[3], "Flat"))
                        {
                            _bulk.Flat();
                            refresh_Child(true);
                        }     

                        if (GUI.Button(_rtList[5], "Clear"))
                        {
                            _bulk.Clear();
                            refresh_Child(true);
                        }                    
                    }
                }
            }
        }

        private void ShowTileControl()
        {
            using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightBlue))
            {
                EditorGUILayout.ObjectField(_RefTile, new GUIContent("Style Reference Tile", "벌크에서 생성되는 타일의 기본 스타일"));
                using (new EditorGUILayout.HorizontalScope())
                {
                    string _BtnMsg = _bulk.bYFirstSort ? "Sort Xyz -> Yxz" : "Sort Yxz -> Xyz";
                    if (GUILayout.Button(_BtnMsg))
                    {
                        _bulk.Sort(bToggle: true);
                        refresh_FilteredChild();
                        EditorUtility.SetDirty(this);
                    }

                    if (GUILayout.Button("Rename All!"))
                    {
                        _bulk.Rename();
                        EditorUtility.SetDirty(this);
                    }

                    using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
                    {
                        if (GUILayout.Button("New Tile!"))
                        {
                            _bulk.NewTile(Vector3.zero);
                            refresh_FilteredChild(true);
                            EditorUtility.SetDirty(this);
                        }
                    }
                }
            }
        }
        private void ShowTileFilter()
        {
            // if (Event.current.type != EventType.Layout
            //     && Event.current.type != EventType.Repaint
            //     && Event.current.type != EventType.ScrollWheel)
            //     return;

            using (new EditorGUILayout.HorizontalScope())
            {
                bFoldout = EditorGUILayout.Foldout(bFoldout, 
                    string.Format("Fold({0}/{1})", filteredChildIndexList.Count, _bulk._attachedList.Count), true);
                EditorGUI.BeginChangeCheck();
                filterString = EditorGUILayout.TextField(filterString);
                if (EditorGUI.EndChangeCheck())
                    refresh_FilteredChild();

                using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightMagenta))
                {
                    if (GUILayout.Button("(S)elect!", GUILayout.Width(75)))
                    {
                        SelectTiles();
                    }
                }
            }
            if (bFoldout && _bulk._attachedList.Count > 0)
            {
                float cellHeight = Iso2DDrawer.RectHeight;
                int iMaxRowCount = 18;

                using (var scrolled = new EditorGUILayout.ScrollViewScope(tileListScroll, GUILayout.ExpandHeight(true)))
                {
                    tileListScroll = scrolled.scrollPosition;
                    int iStart = Mathf.Max(Mathf.FloorToInt(tileListScroll.y / cellHeight) -1 , 0);
                    int iEnd = Mathf.Min(filteredChildIndexList.Count, iStart + iMaxRowCount);
                    // Debug.LogFormat("Tile List : start({0}), end({1})", iStart, iEnd);

                    try
                    {
                        if (iStart > 0)                 
                            EditorGUILayout.GetControlRect(GUILayout.Height(cellHeight * (iStart)));

                        for (int i = iStart; i < iEnd; ++i)
                            EditorGUI.PropertyField(Iso2DDrawer.GetRect(), 
                                serializedObject.FindProperty(
                                    string.Format(@"_attachedList.Array.data[{0}]", 
                                    filteredChildIndexList[i])));
                                    
                        if ((iEnd = filteredChildIndexList.Count - iEnd) > 0)
                            EditorGUILayout.GetControlRect(GUILayout.Height(cellHeight * iEnd));
                    }
                    catch
                    {
                        Debug.Log(Event.current.type);
                        // There is a bug here. But there is no harm to use. It will be updated next time.
                    }

                }
            }
        }
    }
}