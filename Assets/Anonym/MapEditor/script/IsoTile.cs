using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{	
	using Util;

	public enum SelectionType
	{
		LastTile,
		NewTile,
		AllTile,
	}

	[SelectionBase]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(GridCoordinates))]
	[RequireComponent(typeof(IsometircSortingOrder))]
	[RequireComponent(typeof(RegularCollider))]
	[ExecuteInEditMode]
    public class IsoTile : MonoBehaviour
    {		
#if UNITY_EDITOR				
		[SerializeField]
		IsoTileBulk _bulk;
		[HideInInspector]
		public IsoTileBulk Bulk{get{
			if (_bulk != null)
				return _bulk;
			if (transform.parent != null)
				return _bulk = transform.parent.GetComponent<IsoTileBulk>();
			return null;
		}}

		[SerializeField]
		GridCoordinates _coordinates = null;
		[HideInInspector]
		public GridCoordinates coordinates{get{
			return _coordinates == null ?
				_coordinates = GetComponent<GridCoordinates>() : _coordinates;
		}}

		[SerializeField]
        AutoNaming _autoName = null;
        [HideInInspector]
        public AutoNaming autoName
        {
            get
            {
                return _autoName == null ?
                    _autoName = GetComponent<AutoNaming>() : _autoName;
            }
        }

		public void Rename()
		{
			autoName.AutoName(); 
		}

		[SerializeField]
        IsometircSortingOrder _so = null;
		[HideInInspector]
        public IsometircSortingOrder sortingOrder{get{
            return _so != null ? _so : _so = GetComponent<IsometircSortingOrder>();
        }}

        public void Update_SortingOrder()
        {
            if (sortingOrder != null)
			{
                sortingOrder.Update_SortingOrder(true);
			}
        }
		
		[HideInInspector, SerializeField]
        public AttachmentHierarchy _attachedList;
		public void Update_AttachmentList()
        { 
			_attachedList.Init(gameObject);   
        }

		[SerializeField]
		public bool bAutoFit_ColliderScale = true;
		[SerializeField]
		public bool bAutoFit_SpriteSize = true;

		public bool IsUnionCube()
		{
			return _attachedList.childList.Exists(r => r.Iso2DObj._Type == Iso2DObject.Type.Side_Union);
		}
		public Iso2DObject GetSideObject(Iso2DObject.Type _type)
		{
			if (_attachedList.childList.Exists(r => r.Iso2DObj._Type == _type))
				return _attachedList.childList.Find(r => r.Iso2DObj._Type == _type).Iso2DObj;
			return null;
		}
		public Iso2DObject[] GetSideObjects(params Iso2DObject.Type[] _types)
		{
			if (_types == null || _types.Length == 0)
				_types = new Iso2DObject.Type[]{	
					Iso2DObject.Type.Obstacle, Iso2DObject.Type.Overlay,
					Iso2DObject.Type.Side_Union, Iso2DObject.Type.Side_X,
					Iso2DObject.Type.Side_Y, Iso2DObject.Type.Side_Z,
				};
			Iso2DObject[] results = new Iso2DObject[0];
			_attachedList.childList.ForEach(r =>{
				if (r.Iso2DObj != null && ArrayUtility.Contains<Iso2DObject.Type>(_types, r.Iso2DObj._Type))
					ArrayUtility.Add<Iso2DObject>(ref results, r.Iso2DObj);
			});
			return results;
		}

		void Update()
		{			
			if (!Application.isEditor || Application.isPlaying  || !enabled)
				return;

		}
		void OnEnable()
		{
			Update_AttachmentList();
		}
		void OnTransformParentChanged()
		{
			_bulk = null;
		}
		void OnTransformChildrenChanged()
		{
			if (autoName.bPostfix_Sprite)
				Rename();
			Update_AttachmentList();
		}
		public IsoTile Duplicate()
		{
			IsoTile result = GameObject.Instantiate(this);
			result.transform.SetParent(transform.parent, false);
			result.Rename();
			Undo.RegisterCreatedObjectUndo(result.gameObject, "IsoTile:Dulicate");			
			return result;
		}		

		public void Copycat(IsoTile from, bool bCopyChild = true, bool bUndoable = true)
		{
			if (from == this)
				return;
				
			coordinates.Apply_SnapToGrid();

			if (bCopyChild)
			{
				for (int i = transform.childCount - 1; i >= 0 ; --i)
				{
					if (bUndoable)
						Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
					else
						DestroyImmediate(transform.GetChild(i).gameObject);
				}

				foreach (Transform child in from.transform) 
				{
					GameObject _newObj = GameObject.Instantiate(child.gameObject, transform, false);
					if (bUndoable)
						Undo.RegisterCreatedObjectUndo(_newObj, "IsoTile:Copycat");
				}

				
			}
			// Update_AttachmentList();
		}	

		public IsoTile Extrude(Vector3 _direction, bool _bContinuously, bool _withAttachment)
		{
			IsoTile _new = Duplicate();
			if (!_withAttachment)
				_new.Clear_Attachment(false);
			Undo.RegisterCreatedObjectUndo(_new.gameObject, "IsoTile:Extrude");
			_new.coordinates.Translate(_direction, "IsoTile:Extrude");	
			Undo.RecordObject(gameObject, "IsoTile:Extrude");
			return _new;			
		}		

		public void Clear_Attachment(bool bCanUndo)
		{
			Iso2DObject[] _iso2Ds = transform.GetComponentsInChildren<Iso2DObject>();
			for (int i = 0; i < _iso2Ds.Length; ++i)
			{
				Iso2DObject _iso2D = _iso2Ds[i];
				if (_iso2D != null && _iso2D.IsAttachment)
					_iso2D.DestoryGameObject(bCanUndo, false);
			}
		}

		void Clear_SideObject(bool bCanUndo)
        {
			Iso2DObject[] _attachedList = GetSideObjects(
				Iso2DObject.Type.Side_X, Iso2DObject.Type.Side_Y, 
				Iso2DObject.Type.Side_Z, Iso2DObject.Type.Side_Union);
			for (int i = 0; i < _attachedList.Length; ++i)
			{
				if (_attachedList[i] != null)
				{
					_attachedList[i].DestoryGameObject(bCanUndo, true);
				}
			}
        }

		void Add_SideObject(GameObject _prefab, string _UndoMSG)
		{
			GameObject _obj = GameObject.Instantiate(_prefab, transform, false);
			_obj.transform.SetAsFirstSibling();
			RegularCollider _rc = _obj.GetComponent<RegularCollider>();
			_rc.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
			Undo.RegisterCreatedObjectUndo(_obj, _UndoMSG);
			Update_AttachmentList();
		}

		public void Reset_SideObject(bool _bTrueUnion)
		{
			Clear_SideObject(true);
			Add_SideObject(_bTrueUnion ? 
				IsoMap.instance.Side_Union_Prefab 
				: IsoMap.instance.Side_Y_Prefab, 
				"Change Tile Style");
		}

		public void Toggle_Side(bool _bToggle, Iso2DObject.Type _toggleType)
		{
			Iso2DObject _obj = GetSideObject(_toggleType);
			if (_bToggle)
            {
                if (_obj == null)
                {
					Add_SideObject(IsoMap.instance.GetSidePrefab(_toggleType),
						"Created : " + _toggleType + " Object");
                }
            }
            else
            {
                if (_obj != null)
                {
					_obj.DestoryGameObject(true, true);
                    Update_AttachmentList();
                }
            }
		}

        public bool IsLastTile(Vector3 _direction)
        {
            return Bulk.GetTileList_At(coordinates._xyz, _direction, false, true).Count == 0;
        }

        public IsoTile NextTile(Vector3 _direction)
        {
            List<IsoTile> _tiles = Bulk.GetTileList_At(coordinates._xyz, _direction, false, false);
            return (_tiles.Count > 0) ? _tiles[0] : null;
        }

        public bool IsAccumulatedTile_Coordinates(Vector3 _direction)
        {
			Vector3 _xyz = coordinates._xyz;
            List<IsoTile> _tiles = Bulk.GetTileList_At(_xyz, _direction, false, true);

            int iCheckValue = coordinates.grid.CoordinatesCountInTile(_direction);
			
            iCheckValue *= iCheckValue;
            for(int i = 0 ; i < _tiles.Count ; ++i)
            {
                Vector3 diff = Vector3.Scale(_xyz - _tiles[i].coordinates._xyz, _direction);
                if (Mathf.RoundToInt(diff.sqrMagnitude) < iCheckValue)
                {
                    return true;
                }
            }
            return false;
        }

		public bool IsAccumulatedTile_Collider(Vector3 _direction)
        {
			Vector3 _xyz = coordinates._xyz;
            List<IsoTile> _tiles = Bulk.GetTileList_At(_xyz, _direction, false, true);

			Bounds _bounds = GetBounds();
			// Vector3 _diff = transform.position - _bounds.center;
			// _bounds.SetMinMax(_bounds.min + 2f * _diff, _bounds.max + 2f * _diff);
            for(int i = 0 ; i < _tiles.Count ; ++i)
            {
				if (_tiles[i].GetBounds().Intersects(_bounds))
					return true;
            }
            return false;
        }

		public Bounds GetBounds()
		{
			Collider[] _colliders = transform.GetComponentsInChildren<Collider>();
			if (_colliders == null || _colliders.Length == 0)
				return new Bounds(transform.position, Vector3.zero);

			Bounds _bounds = new Bounds(_colliders[0].bounds.center, _colliders[0].bounds.size);
			for(int i = 1 ; i < _colliders.Length; ++i)
			{
				_bounds.Encapsulate (_colliders[i].bounds);
			}
			_bounds.Expand(Grid.fGridTolerance);
			return _bounds;
		}
        
        public void MoveToZeroground()
        {
            Vector3 _ZeroGround = coordinates._xyz;
            coordinates.Move(_ZeroGround.x, 0, _ZeroGround.z, "IsoTile:MoveToZeroGround");
        }

		public void Init()
		{
			RegularCollider[] _RCs = GetComponentsInChildren<RegularCollider>();
			Vector3 _tilsSize = coordinates.grid.TileSize;
			foreach(var _RC in _RCs)
			{
				_RC.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
				//_RC.AdjustScale();
			}
		}

		public void Update_Grid()
		{
			coordinates.Update_Grid(true);
			RegularCollider[] _RCs = GetComponentsInChildren<RegularCollider>();
			foreach(var _RC in _RCs)
			{
				_RC.Toggle_UseGridTileScale(bAutoFit_ColliderScale);
				// _RC.AdjustScale();
			}
		}

		public void Update_Attached_Iso2DScale()
		{
			foreach(var _attached in _attachedList.childList)
			{
				Iso2DObject _Iso2D = _attached.Iso2DObj;
				if (_Iso2D != null)
				{
					_Iso2D.AdjustScale();
				}
			}
		}
#endif
    }
}