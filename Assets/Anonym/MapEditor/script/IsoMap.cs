using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Isometric
{
	using Util;

	[DisallowMultipleComponent]
	[RequireComponent(typeof(Grid))]
	public class IsoMap : Singleton<IsoMap> {

		[SerializeField]
		public static float fResolution = 100f;		
		public static Vector3 vMAXResolution = Vector3.one * fResolution;
		[SerializeField]
        public Vector3 fResolutionOfIsometric = vMAXResolution;

		[SerializeField]
		public bool bUseIsometricSorting = true;		
		
#if UNITY_EDITOR	

		List<IsoTileBulk> _childBulkList = new List<IsoTileBulk>();
		public void Regist_Bulk(IsoTileBulk _add)
		{
			if (_add == null || PrefabUtility.GetPrefabType(_add).Equals(PrefabType.Prefab))
				return;

			if (!_childBulkList.Exists(r => r == _add))
			{
				_childBulkList.Add(_add);
			}
		}
		public void Update_Grid()
		{
			for(int i = _childBulkList.Count - 1; i >= 0 ; --i)
			{
				if(_childBulkList[i] == null)
				{
					_childBulkList.RemoveAt(i);
					continue;
				}
				_childBulkList[i].coordinates.Update_Grid(true);
				if (_childBulkList[i].coordinates.grid.IsInheritGrid)
				{
					_childBulkList[i].Update_Grid();
				}
			}
		}
		[SerializeField]
		public Vector2 TileAngle = new Vector2(30f, -45f);
		float _last_TileAngle_Y = 0;
		float _last_Scale_TA_Y = 1f;
		public float fScale_TA_Y(Vector3 _v3Size)
		{
			bool bCosRange = (TileAngle.y >= -45f && TileAngle.y < 45f)
					|| (TileAngle.y >= 135f && TileAngle.y < 225f);
			if (_last_TileAngle_Y != TileAngle.y)
			{
				_last_TileAngle_Y = TileAngle.y;
				if (bCosRange)
					_last_Scale_TA_Y = Mathf.Cos(Mathf.Deg2Rad * _last_TileAngle_Y);
				else
					_last_Scale_TA_Y = Mathf.Sin(Mathf.Deg2Rad * _last_TileAngle_Y);
			}
			return Mathf.Abs((bCosRange ? _v3Size.x : _v3Size.z) / _last_Scale_TA_Y);
		}
		private Vector2 _lastTileAngle = Vector2.zero;
		private float _lastMagicValue = 2f;
		public float fMagicValue{
			get{
				if (TileAngle.Equals(_lastTileAngle))
					return _lastMagicValue;
				return _lastMagicValue = Mathf.Abs(2f * ((3 * Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * TileAngle.x), 2) - 1) 
					/ (3 * Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad * TileAngle.y), 2) - 1) + 1) / 3f);
			}
		}

		[SerializeField]
		public float ReferencePPU = 128;
		[SerializeField]
		Grid _grid;
		public Grid gGrid{
			get
			{
				if (_grid == null)
					_grid = GetComponent<Grid>();
				return _grid;
			}
		}
		
		[SerializeField]
		public Camera GameCamera;
		
		[SerializeField]
		bool bCustomResolution = true;
		[SerializeField]
		Vector3 vCustomResolution = vMAXResolution;

		// new Vector3(85.1f, 10.3f, 52.5f);
		public void UpdateIsometricSortingResolution()
		{
			if (bUseIsometricSorting)
			{
				if (!bCustomResolution)
				{
					fResolutionOfIsometric.Set(
						Mathf.Max(Grid.fGridTolerance, Mathf.Sin(Mathf.Deg2Rad * -TileAngle.y) * fResolution),
						Mathf.Max(Grid.fGridTolerance, Mathf.Sin(Mathf.Deg2Rad * TileAngle.x) * fResolution),
						Mathf.Max(Grid.fGridTolerance, Mathf.Cos(Mathf.Deg2Rad * -TileAngle.y) * fResolution)
					);
				}
				else
				{
					fResolutionOfIsometric = vCustomResolution;
				}
			}
			else
				fResolutionOfIsometric.Set(0f, 0f, 0f);
		}
		public void Update_All_ISO()
		{
			IsometircSortingOrder[] _tmpArray = FindObjectsOfType<IsometircSortingOrder>();
			if (_tmpArray != null)
			{
				for (int i = 0 ; i < _tmpArray.Length; ++i)
				{
					if (_tmpArray[i] != null)
					{
						_tmpArray[i].Update_SortingOrder(true);
					}
				}
			}
		}
		public void Update_TileAngle()
		{
			UpdateIsometricSortingResolution();

			if (SceneView.lastActiveSceneView != null)
			{
				if (SceneView.lastActiveSceneView.in2DMode)
					SceneView.lastActiveSceneView.in2DMode = false;

				if (SceneView.lastActiveSceneView.orthographic == false)
					SceneView.lastActiveSceneView.orthographic = true;

				SceneView.lastActiveSceneView.LookAtDirect(
					IsoMap.instance.transform.position, 
					Quaternion.Euler(TileAngle));
			}

			if (GameCamera != null)
			{
				GameCamera.transform.rotation = Quaternion.Euler(TileAngle);
				if (GameCamera.orthographic == false)
				{
					GameCamera.orthographic = true;
					GameCamera.orthographicSize = ((GameCamera.pixelHeight)/(1f * ReferencePPU)) * 0.5f;
				}

			}
		}
		
		public GameObject BulkPrefab;
		public GameObject TilePrefab;
		public GameObject OverlayPrefab;
		public GameObject ObstaclePrefab;
		public GameObject Side_Union_Prefab;
		public GameObject Side_X_Prefab;
		public GameObject Side_Y_Prefab;
		public GameObject Side_Z_Prefab;
		public GameObject Collider_X_Prefab;
		public GameObject Collider_Y_Prefab;
		public GameObject Collider_Z_Prefab;
		public GameObject Collider_Cube_Prefab;
		public Sprite IsoTile_Union_OutlineImage;
		public Sprite IsoTile_Side_OutlineImage;
		public Sprite RefTileSprite;

		public GameObject GetSidePrefab(Iso2DObject.Type _type)
		{
			switch(_type)
			{
				case Iso2DObject.Type.Side_Union:
					return Side_Union_Prefab;
				case Iso2DObject.Type.Side_X:
					return Side_X_Prefab;
				case Iso2DObject.Type.Side_Y:
					return Side_Y_Prefab;
				case Iso2DObject.Type.Side_Z:
					return Side_Z_Prefab;
			}
			return null;
		}

		public IsoTileBulk NewBulk()
		{			
			if (BulkPrefab == null)
			{
				Debug.LogError("IsoMap : No BulkPrefab!");
				return null;
			}
			IsoTileBulk _newBulk = GameObject.Instantiate(BulkPrefab).GetComponent<IsoTileBulk>();
			Undo.RegisterCreatedObjectUndo(_newBulk.gameObject, "IsoTile:Create");
			_newBulk.transform.SetParent(transform, false);
            _newBulk.coordinates.Move(gGrid.Centor);
			return _newBulk;
		}
		
		public IsoTile NewTile_Raw()
		{
			if (TilePrefab == null)
			{
				Debug.LogError("IsoMap : No TilePrefab!");
				return null;
			}
			IsoTile _newTile = GameObject.Instantiate(TilePrefab).GetComponent<IsoTile>();
			Undo.RegisterCreatedObjectUndo(_newTile.gameObject, "IsoTile:Create");			
			return _newTile;
		}
		IsoTileBulk[] GetAllBulk()
		{
			return gameObject.GetComponentsInChildren<IsoTileBulk>();
		}

		public void BakeNavMesh()
		{
		}

		void OnValidate()
		{
			if (!PrefabUtility.GetPrefabType(this).Equals(PrefabType.Prefab)
				&& Application.isEditor && !Application.isPlaying 
				&& !EditorApplication.isPlayingOrWillChangePlaymode
				&& !EditorApplication.isUpdating
				&& !EditorApplication.isTemporaryProject
				&& !IsNull)
				Update_TileAngle();
		}
#endif
	}
}