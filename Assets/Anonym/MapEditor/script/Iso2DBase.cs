using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
	using Util;
	
	[System.Serializable]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SpriteRenderer))]
	[ExecuteInEditMode]
    public class Iso2DBase : MonoBehaviour
    {
#if UNITY_EDITOR
		public Vector2 localRotation;
		public Vector3 localScale;

		protected float IsometricRotationScale = 1f;

		[HideInInspector]
		float _lastRefPPu;
		[HideInInspector]
		float _lastSpritePPU;
		[HideInInspector]
		float _lastPPURefScale;
		public float PPURefScale{
			get{
				if (sprr == null || sprr.sprite == null)
					return 1f;
				if (_lastSpritePPU != sprr.sprite.pixelsPerUnit ||
					_lastRefPPu != IsoMap.instance.ReferencePPU)
				{
					_lastSpritePPU = sprr.sprite.pixelsPerUnit;
					_lastRefPPu = IsoMap.instance.ReferencePPU;
					return _lastPPURefScale = sprr.sprite.ReferencePPUScale(_lastRefPPu);
				}
				return _lastPPURefScale;
			}
		}
		[SerializeField]
		protected bool _bApplyPPUScale = true;
		public bool bApplyPPUScale{get{return _bApplyPPUScale;}}

		protected SpriteRenderer _sprr;
		public SpriteRenderer sprr
		{
			get{return  _sprr != null ? _sprr : _sprr = GetComponent<SpriteRenderer>();}
		}

		Iso2DBase[] _childIso2Ds = null;
		protected Iso2DBase[] ChildIso2Ds {get{
			return _childIso2Ds != null ? _childIso2Ds :
				(_childIso2Ds = findChildIso2D());
		}}
		Iso2DBase[] findChildIso2D()
		{
			List<Iso2DBase> _list = new List<Iso2DBase>();
			Iso2DBase _tmp;
			for (int i = 0 ; i < transform.childCount; ++i)
			{
				if ((_tmp = transform.GetChild(i).GetComponent<Iso2DBase>()))
					_list.Add(_tmp);
			}
			return _list.ToArray();
		}

		void OnTransformChildrenChanged()
		{
			_childIso2Ds = null;
		}

		protected void Update()
		{
			if (!Application.isEditor || Application.isPlaying || !enabled)
				return;

			adjustRotation();
			AdjustScale();

		}

		protected void adjustRotation()
		{			
			transform.eulerAngles = IsoMap.instance.TileAngle + localRotation;
		}

		public virtual void AdjustScale()
		{
			adjustScale(Vector3.one);
		}
		protected void adjustScale(Vector3 vMultiplier)
		{
			Vector3 v3Tmp = vMultiplier;
			if (bApplyPPUScale)
				v3Tmp *= PPURefScale;
			if (localScale.Equals(Vector3.zero))
			{
				localScale = v3Tmp;
			}
			else
			{
				if (IsometricRotationScale != 0f)
				{
					transform.localScale = Vector3.Scale(v3Tmp, localScale) * IsometricRotationScale;
				}
				else
				{
					transform.localScale = localScale;
				}
			}
		}

		public void Toggle_ApplyPPUScale()
		{
			UnityEditor.Undo.RecordObject(this, "ApplyPPU");
			_bApplyPPUScale = !_bApplyPPUScale;
			ApplyPPUScale();
		}

		public void ApplyPPUScale()
		{
			float fMultiplier = bApplyPPUScale ? 1f / PPURefScale : PPURefScale;
			for (int i = 0 ; i < transform.childCount; ++i)
			{
				Transform _t = transform.GetChild(i);
				UnityEditor.Undo.RecordObject(_t, "ApplyPPUScale : Adjust Child Scale");
				_t.localScale *= fMultiplier;
			}
		}

		public int Update_SortingOrder(int _newSortingOrder)
		{
			sprr.sortingOrder = _newSortingOrder;
            if (IsoMap.instance.bUseIsometricSorting)
                _newSortingOrder++;
                
			foreach(var r in ChildIso2Ds)
				_newSortingOrder = r.Update_SortingOrder(_newSortingOrder);
			return _newSortingOrder;
		}
#endif
    }
}