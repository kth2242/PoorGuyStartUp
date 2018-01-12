using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
    [ExecuteInEditMode]
	[DisallowMultipleComponent]
    public class SubColliderHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        public static Vector3 V3positiveInfinity
        {
            get
            {
                return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            }
        }
        [SerializeField, HideInInspector]
		bool bApplyGridScale = true;
        BoxCollider _box;
		public BoxCollider BC{get{return _box != null ? _box : (_box = GetComponent<BoxCollider>());}}

		[SerializeField, HideInInspector]
        Vector3 _localScale = V3positiveInfinity;
		Vector3 LocalScale{get{
				if (_localScale.Equals(V3positiveInfinity))
				{
					_localScale = BC.size;
				}
				return _localScale;
			}
		}
		[SerializeField, HideInInspector]
		Vector3 _localPosition = V3positiveInfinity;
		Vector3 LocalPosition{get{
			if (_localPosition.Equals(V3positiveInfinity))
				_localPosition = transform.localPosition;
			return _localPosition;
		}}
		[SerializeField, HideInInspector]
		Vector3 _localCenter = V3positiveInfinity;
		Vector3 LocalCenter{get{
			if (_localCenter.Equals(V3positiveInfinity))
				_localCenter = BC.center;
			return _localCenter;
		}}
		[SerializeField, HideInInspector]
		Vector3 _LastTileScale = Vector3.one;

		public virtual void Toggle_UseGridTileScale(bool _bApplyGridScale)
		{
			if (BC == null)
			{
				bApplyGridScale = _bApplyGridScale;
				return;
			}

			UnityEditor.Undo.RecordObject(this, "Update SubCollider");
			if (!_bApplyGridScale)
			{
				_localPosition = transform.localPosition;
				_localCenter = BC.center;
				_localScale = BC.size;					
			}
			else
			{
				_localPosition = new Vector3(transform.localPosition.x / _LastTileScale.x , 
					transform.localPosition.y / _LastTileScale.y, transform.localPosition.z / _LastTileScale.z);
				_localCenter = new Vector3(BC.center.x / _LastTileScale.x , 
					BC.center.y / _LastTileScale.y, BC.center.z / _LastTileScale.z);
				_localScale = new Vector3(BC.size.x / _LastTileScale.x , 
					BC.size.y / _LastTileScale.y, BC.size.z / _LastTileScale.z);
			}
			bApplyGridScale = _bApplyGridScale;

			ScaleMultiplier(_LastTileScale);
		}
		public void ScaleMultiplier(Vector3 _tileSize)
		{
			if (BC != null)
			{				
				UnityEditor.Undo.RecordObject(BC, "Update SubCollider");
				_LastTileScale = _tileSize;
				transform.localPosition = bApplyGridScale ? Vector3.Scale(LocalPosition, _LastTileScale) : LocalPosition;
				BC.size = bApplyGridScale ? Vector3.Scale(LocalScale, _LastTileScale) : LocalScale;
				BC.center = bApplyGridScale ? Vector3.Scale(LocalCenter, _LastTileScale) : LocalCenter;
			}
		}
#endif
    }
}