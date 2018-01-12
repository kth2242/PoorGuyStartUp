using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{
    [System.Serializable]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Grid))]
    [ExecuteInEditMode]
    public class IsoTileBulk : MonoBehaviour {
#if UNITY_EDITOR
        [SerializeField]
        public Rect SizeXZ;
        [SerializeField]
		GridCoordinates _coordinates;
		[HideInInspector]
		public GridCoordinates coordinates{get{
			return _coordinates == null ?
				_coordinates = GetComponent<GridCoordinates>() : _coordinates;
		}}

        [HideInInspector, SerializeField]        
        public List<IsoTile> _attachedList;

        [SerializeField]
        GameObject _referenceTile;
        [SerializeField]
        bool _bSensorOff_ChildChange = false;
        public void Do_With_SensorOff(System.Action _underAction)
        {
            _bSensorOff_ChildChange = true;
            try{
                _underAction();
            }catch{ }
            _bSensorOff_ChildChange = false;
        }
        Vector3 lastPosition = new Vector3();
        void Update()
		{
			if (!Application.isEditor || Application.isPlaying || !enabled)
				return;

            if (!lastPosition.Equals(transform.position))
            {
                lastPosition = transform.position;
                for(int i = 0 ; i < _attachedList.Count; ++i)
                {
                    if (_attachedList[i] != null)
                    {
                        _attachedList[i].Update_SortingOrder();
                        // 수정
                    }
                }
            }
        }
        bool preventZeroTileBulk()
        {
            if (transform.childCount == 0)
            {
                NewTile(Vector3.zero);
                Update_ChildList();
                EditorUtility.SetDirty(this);
                return true;
            }
            return false;
        }
        void OnValidate()
        {
            if (!IsoMap.IsNull)
                IsoMap.instance.Regist_Bulk(this);
        }
        void OnTransformParentChanged()
        {
            if (!_bSensorOff_ChildChange)
                preventZeroTileBulk();
        }
        void OnTransformChildrenChanged()
        {
            if (!_bSensorOff_ChildChange)
            {
                if (!preventZeroTileBulk())
                    Update_ChildList();
            }
        }

        public void Update_ChildList()
        {
            _attachedList.Clear();
            SizeXZ_Clear();
            for(int i = 0 ; i < transform.childCount; ++i)
            {
                IsoTile _obj = transform.GetChild(i).GetComponent<IsoTile>();
                if (_obj != null)
                {
                    _attachedList.Add(_obj);
                    SizeXZ_Update(_obj.coordinates);
                }
            }
            Sort();
        }

        public void Update_Grid()
        {
            for(int i = 0 ; i < _attachedList.Count; ++i)
            {
                if (_attachedList[i] != null)
                {
                    _attachedList[i].Update_Grid();
                }
            }
        }

        public bool bYFirstSort;
        public void Sort(bool bToggle = false)
        {            
            _attachedList.Sort((r1, r2) => 
            {   
                Vector3 r1Coordinates = r1.coordinates._xyz;
                Vector3 r2Coordinates = r2.coordinates._xyz;
                int iResult;                
                if (bYFirstSort && ((iResult = r2Coordinates.y.CompareTo(r1Coordinates.y)) != 0))
                    return iResult;
                if ((iResult = r2Coordinates.x.CompareTo(r1Coordinates.x)) != 0)
                    return iResult;
                if (!bYFirstSort && ((iResult = r2Coordinates.y.CompareTo(r1Coordinates.y)) != 0))
                    return iResult;
                return r2Coordinates.z.CompareTo(r1Coordinates.z);
            });
            if (bToggle)
                bYFirstSort = !bYFirstSort;
        }

        public void Flat()
        {
            Undo.RecordObject(this, "Bulk : Flat");
            List<IsoTile> _DelList = new List<IsoTile>();
            for (int i = _attachedList.Count - 1 ; i >= 0; --i)
            {
                if (!_attachedList[i].IsLastTile(Vector3.up))
                {
                    _DelList.Add(_attachedList[i]);
                    _attachedList.RemoveAt(i);
                }
            }
            _attachedList.ForEach(r => r.MoveToZeroground());
            Do_With_SensorOff(() => _DelList.ForEach(r => Undo.DestroyObjectImmediate(r.gameObject)));
        }

        public void Clear()
        {
            Do_With_SensorOff(() =>
            {
                Undo.RecordObject(this, "Bulk : Clear");
                for (int i = _attachedList.Count - 1 ; i >= 0; --i)
                {
                    Undo.DestroyObjectImmediate(_attachedList[i].gameObject);
                }
                _attachedList.Clear();
            });
            preventZeroTileBulk();
        }

        public void Earthquake(float yMinDef, float yMaxDef)
        {
            int yMin = Mathf.RoundToInt(SizeXZ.yMin);
            int yMax = Mathf.RoundToInt(SizeXZ.yMax);
            int xMin = Mathf.RoundToInt(SizeXZ.xMin);
            int xMax = Mathf.RoundToInt(SizeXZ.xMax);

            for(int z = yMin ; z <= yMax ; ++z)
            {
                for(int x = xMin ; x <= xMax ; ++x) 
                {
                    int _iHeighDef = Mathf.RoundToInt(Random.Range(yMinDef, yMaxDef));
                    _attachedList.ForEach(r =>{
                        if (r.coordinates.IsSame(new Vector3(x,0,z), true, false, true))
                        {
                            r.coordinates.Translate(0, _iHeighDef, 0);
                        }
                    });
                }
            }
        }

        public void NoRedundancy()
        {
            Undo.RecordObject(this, "Bulk : No Redundancy");
            List<IsoTile> _UniqueList = new List<IsoTile>();
            List<IsoTile> _DelList = new List<IsoTile>();
            for (int i = _attachedList.Count - 1 ; i >= 0; --i)
            {
                IsoTile _lookupTile = _attachedList[i];
                IsoTile _RedundantTile = 
                    _UniqueList.Find(r => r.coordinates.IsSame(_lookupTile.coordinates._xyz));
                if (_RedundantTile != null)
                {
                    if (_lookupTile.GetComponentsInChildren<Iso2DObject>().Length > 
                        _RedundantTile.GetComponentsInChildren<Iso2DObject>().Length)
                    {
                        _UniqueList.Remove(_RedundantTile);
                        _RedundantTile = null;
                    }
                }
                _UniqueList.Add(_lookupTile);
                if (_RedundantTile != null)
                    _DelList.Add(_RedundantTile);
            }
            Do_With_SensorOff(() => _DelList.ForEach(r => Undo.DestroyObjectImmediate(r.gameObject)));
        }

        void SizeXZ_Clear()
        {
            SizeXZ.x = SizeXZ.y = SizeXZ.width = SizeXZ.height = 0;
        }
        void SizeXZ_Update(GridCoordinates _GC)
        {
            Vector3 coordV3 = _GC._xyz;
            Vector2 coord = new Vector2(coordV3.x , coordV3.z);
            if (!SizeXZ.Contains(coord))
            {
                SizeXZ = Rect.MinMaxRect(
                    Mathf.Min(coord.x, SizeXZ.xMin),
                    Mathf.Min(coord.y, SizeXZ.yMin),
                    Mathf.Max(coord.x, SizeXZ.xMax),
                    Mathf.Max(coord.y, SizeXZ.yMax));               
            }
        }
        public List<IsoTileBulk> GetChildBulkList(bool bLocalBulkOnly)
        {
            List<IsoTileBulk> _bulks = new List<IsoTileBulk>();

            if (!bLocalBulkOnly || GetComponent<Grid>().IsInheritGrid)
                _bulks.Add(this);

            for(int i = 0 ; i < transform.childCount; ++i)
            {
                IsoTileBulk _bTmp = transform.GetChild(i).GetComponent<IsoTileBulk>();
                if (_bTmp != null)
                    _bulks.AddRange(_bTmp.GetChildBulkList(bLocalBulkOnly));
            }
            return _bulks;
        }
        
        public List<IsoTile> GetTileList()
        {
            List<IsoTile> _tiles = new List<IsoTile>();
            for(int i = 0 ; i < transform.childCount; ++i)
            {
                IsoTile _obj = transform.GetChild(i).GetComponent<IsoTile>();
                if (_obj != null)
                    _tiles.Add(_obj);
            }
            return _tiles;
        }

        public List<IsoTile> GetTileList_At(Vector3 atCoordinates)
        {
            List<IsoTile> _tiles = new List<IsoTile>();
            for(int i = 0 ; i < transform.childCount; ++i)
            {
                IsoTile _obj = transform.GetChild(i).GetComponent<IsoTile>();
                if (_obj != null)
                {
                    if ((atCoordinates - _obj.coordinates._xyz).magnitude < Grid.fGridTolerance)
                    {
                        _tiles.Add(_obj);
                    }
                }
            }
            return _tiles;
        }

        public List<IsoTile> GetTileList_At(Vector3 startingCoordinates, 
            Vector3 _direction, bool bContinuously, bool bApplyRefTileSize)
        {
            List<IsoTile> _tiles = new List<IsoTile>();
            Vector3 v3SubGrid = new Vector3();
            v3SubGrid.Set(bApplyRefTileSize ? coordinates.grid.TileSize.x : 1 / coordinates.grid.GridInterval.x,
                bApplyRefTileSize ? coordinates.grid.TileSize.y : 1 / coordinates.grid.GridInterval.y,
                bApplyRefTileSize ? coordinates.grid.TileSize.z : 1 / coordinates.grid.GridInterval.z);
            int iSubGridCount = !bApplyRefTileSize ? 1 : coordinates.grid.CoordinatesCountInTile(_direction);
            int iLastCount = -1;
            do
            {
                iLastCount = _tiles.Count;
                for(int j = 0 ; j < iSubGridCount; j++)
                {
                    startingCoordinates += _direction;
                    _tiles.AddRange(GetTileList_At(startingCoordinates));
                }
            }while(bContinuously && (iLastCount != _tiles.Count));
            return _tiles;
        }

        public void Resize(Rect _reSize, bool _bFillIn)
        {
            Undo.RecordObject(this, "Bulk:Resize");
            
            Vector3 _xyz = new Vector3();
            int yMin = Mathf.RoundToInt(_reSize.yMin);  int yMax = Mathf.RoundToInt(_reSize.yMax);
            int xMin = Mathf.RoundToInt(_reSize.xMin);  int xMax = Mathf.RoundToInt(_reSize.xMax);
            bool bExtention = _reSize.xMin < SizeXZ.xMin || _reSize.yMin < SizeXZ.yMin
                || _reSize.xMax > SizeXZ.xMax || _reSize.yMax > SizeXZ.yMax;

            Do_With_SensorOff(() => {
                for(int z = yMin ; z <= yMax ; ++z){
                    bool bZEdige = z == yMin || z == yMax;
                    for(int x = xMin ; x <= xMax ; ++x){
                        bool bXEdige = x == xMin || x == xMax;
                        if (bExtention && (bZEdige || bXEdige)){
                            _xyz.Set(x, 0, z);                        
                            if (!_attachedList.Exists(r => r.coordinates.IsSame(_xyz, true, false, true)))
                                NewTile(_xyz);
                        }
                    }
                }
                SizeXZ = _reSize;
                for(int i = 0 ; i < _attachedList.Count; ++i){
                    if (_attachedList[i] == null || _attachedList[i].gameObject == null)
                        continue;
                    Vector3 _coord = _attachedList[i].coordinates._xyz;                
                    if (_coord.x < SizeXZ.xMin || _coord.x > SizeXZ.xMax ||
                        _coord.z < SizeXZ.yMin || _coord.z > SizeXZ.yMax ){
                        Undo.DestroyObjectImmediate(_attachedList[i].gameObject);                
                    }
                }
            });
        }

        public void NewTile(Vector3 _xyz_coordinates)
        {
            IsoTile _newTile;
            if (_referenceTile != null)
            {
                _newTile = GameObject.Instantiate(_referenceTile).GetComponent<IsoTile>();
                Undo.RegisterCreatedObjectUndo(_newTile.gameObject, "IsoTile:Create");			
            }
			else
				_newTile = IsoMap.instance.NewTile_Raw();
			
            _newTile.transform.SetParent(transform, false);
            _newTile.coordinates.Move(_xyz_coordinates);
            _newTile.Init();
            _newTile.Rename();
            SizeXZ_Update(_newTile.coordinates);
        }

        public void Rename()
        {
            _attachedList.ForEach(r => {
                IsoTile _tile = r.GetComponent<IsoTile>();
                if (_tile != null)
                    _tile.Rename();
            });
        }

        public IsoTileBulk Duplicate()
		{
			IsoTileBulk result = GameObject.Instantiate(this);
			result.transform.SetParent(transform.parent, false);
			result.Rename();
			Undo.RegisterCreatedObjectUndo(result.gameObject, "IsoTileBulk:Dulicate");			
			return result;
		}

#endif
    }
}