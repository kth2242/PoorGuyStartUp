using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
	using Util;
    [DisallowMultipleComponent]
    [System.Serializable]
    [ExecuteInEditMode]
    public class Grid : MonoBehaviour
    {
        public static float fGridTolerance = 0.01f;
#if UNITY_EDITOR
        [Header("Grid"), SerializeField, HideInInspector]
        bool bUseLocalGrid = true;

        [HideInInspector]
        Grid _parentGrid;
		[ConditionalHide("!bUseLocalGrid", true)]
        [SerializeField]
        public Grid parentGrid{
            get{
                if (_parentGrid == null && transform.parent != null)
                    _parentGrid = transform.parent.GetComponent<Grid>();
                if (_parentGrid == null)// && gameObject != IsoMap.instance.gameObject)
                    _parentGrid = IsoMap.instance.gGrid;
                
                return _parentGrid;
            }
        }

		[ConditionalHide("bUseLocalGrid", true)]
        public bool IsInheritGrid { get { return !bUseLocalGrid;}}// && parentGrid != null; } }

		[ConditionalHide("bUseLocalGrid", true)]
        [SerializeField]
        Vector3 _TileSize = Vector3.one;

		[ConditionalHide("bUseLocalGrid", true)]
        [SerializeField]
        Vector3 _GridInterval = new Vector3(1f, 1f/3f, 1f);

        [HideInInspector]
		GridCoordinates _coordinates;
		[HideInInspector]
		public GridCoordinates coordinates{get{
			return _coordinates == null ?
				_coordinates = GetComponent<GridCoordinates>() : _coordinates;
		}}

        public Vector3 TileSize{   
            //get {   return IsInheritGrid ? Vector3.Scale(_TileScale, parentGrid.Scale) : _TileScale;    }
            get {   return IsInheritGrid ? parentGrid.TileSize : _TileSize;    }
        }
        public Vector3 GridInterval{    
            // get {   return IsInheritGrid ? Vector3.Scale(_Size, parentGrid.Size) : _Size;   }
            get {   return Vector3.Scale(TileSize, IsInheritGrid ? parentGrid.GridInterval : _GridInterval);   }
        }
        public int CoordinatesCountInTile(Vector3 _direction)
        {            
            Vector3 result = Vector3.Scale(_direction, TileSize);
            Vector3 size = GridInterval;
            return Mathf.Abs(Mathf.RoundToInt(result.x / size.x + result.y / size.y + result.z / size.z));
        }
        public Vector3 Centor
        {
            get{
                if (IsInheritGrid)
                {
                    Vector3 v3Result = new Vector3();
                    v3Result.x = transform.localPosition.x / parentGrid.GridInterval.x;
                    v3Result.y = transform.localPosition.y / parentGrid.GridInterval.y;
                    v3Result.z = transform.localPosition.z / parentGrid.GridInterval.z;
                    //v3Result -= parentGrid.Centor;
                    return v3Result;
                }
                //Debug.Log("Grid(" + gameObject.name + ") Centor : " + v3Result);
                return transform.position;
            }
        }

        public bool bChildUpdatedFlagForEditor = false;
        void OnTransformChildrenChanged()
		{
			bChildUpdatedFlagForEditor = true;
		} 
#endif
    }
}