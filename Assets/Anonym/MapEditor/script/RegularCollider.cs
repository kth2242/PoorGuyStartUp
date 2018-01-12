using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Anonym.Isometric
{
	[DisallowMultipleComponent]
    [ExecuteInEditMode][DefaultExecutionOrder(3)]
    public class RegularCollider : SubColliderHelper
    {
#if UNITY_EDITOR
		Iso2DObject[] _iso2DsCash = null;
		
		public Iso2DObject[] Iso2Ds{get{
			return _iso2DsCash != null ?
				_iso2DsCash : (_iso2DsCash = findIso2DObject());
		}}
		IsoTile _tile = null;
		public IsoTile Tile{get{return _tile != null ? _tile : (_tile = GetComponentInParent<IsoTile>());}}

		[SerializeField]
		Vector3 _vIso2DScaleMultiplier = Vector3.one;
		public Vector3 Iso2DScaleMultiplier {get{return _vIso2DScaleMultiplier;}}
		Iso2DObject[] findIso2DObject()
		{
			List<Iso2DObject> _iso2ds = new List<Iso2DObject>();
			Iso2DObject _iso2d;
			for (int i = 0 ; i < transform.childCount; ++i)
			{
				if ((_iso2d = transform.GetChild(i).GetComponent<Iso2DObject>()) != null)
					_iso2ds.Add(_iso2d);
			}
			return _iso2ds.ToArray();
		}

		SubColliderHelper[] _subColliders;
		public SubColliderHelper[] SubColliders{get{
			if (_subColliders == null)
				update_subColliders();
			return _subColliders; 
		}}
        public void update_subColliders()
        {
            List<SubColliderHelper> _tmpList = new List<SubColliderHelper>();
            for (int i = 0 ; i < transform.childCount; ++i)
            {
                SubColliderHelper _sub = transform.GetChild(i).GetComponent<SubColliderHelper>();
                if (_sub != null)
                    _tmpList.Add(_sub);
            }
			if (_tmpList.Count > 0)
            	_subColliders = _tmpList.Where(r => r.gameObject.GetComponent<RegularCollider>() == null).ToArray();
        }
		public void Update_SortingOrder()
		{
			if (Iso2Ds != null)
			{
				int _so = IsometricSortingOrderUtility.IsometricSortingOrder(transform);
				foreach(var _iso2D in Iso2Ds)
					_so = _iso2D.Update_SortingOrder(_so);
			}
		}
		void Update()
		{
			if (!Application.isEditor || Application.isPlaying || !enabled)
				return;

			if (transform.hasChanged)
			{
				Update_SortingOrder();
				transform.hasChanged = false;
			}
		}
		void OnTransformParentChanged()
		{
			_tile = null;
			Update_SortingOrder();
		}
		void OnTransformChildrenChanged()
		{
			_iso2DsCash = null;	
			_subColliders = null;
            Update_SortingOrder();
        }

		public override void Toggle_UseGridTileScale(bool bTBackup_FRestore)
		{
			base.Toggle_UseGridTileScale(bTBackup_FRestore);

			RegularCollider[] _rcs = GetComponentsInChildren<RegularCollider>();
			if (_rcs != null && _rcs.Length > 0)
			{
				foreach(var _obj in _rcs)
				{
					if (_obj != null && _obj != this)
					{
						_obj.Toggle_UseGridTileScale(bTBackup_FRestore);
					}
				}
			}

			if (SubColliders != null && SubColliders.Length > 0)
			{
				foreach(var _obj in SubColliders)
				{
					if (_obj != null)
					{
						_obj.Toggle_UseGridTileScale(bTBackup_FRestore);
					}
				}
			}

			AdjustScale();
		}		
		public void AdjustScale()
		{
			if (Tile.bAutoFit_ColliderScale)
			{
				Vector3 _tileSize = Tile.coordinates.grid.TileSize;
				ScaleMultiplier(_tileSize);
				if (SubColliders != null && SubColliders.Length > 0)
				{
					foreach(var _obj in SubColliders)
					{
						if (_obj != null)
						{
							_obj.ScaleMultiplier(_tileSize);
						}
					}
				}
			}
		}
#endif
    }
}