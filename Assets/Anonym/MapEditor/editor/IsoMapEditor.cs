using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Anonym.Isometric
{
	using Util;
	[CustomEditor(typeof(IsoMap))]
    public class IsoMapEditor : Editor
    {
		[SerializeField]
		Vector2 vRotate;
		bool bPrefab = true;
		SerializedProperty spTileAngle;
		
		SerializedProperty spReferencePPU;
		bool bEditPrefab;
		SerializedProperty spBulkPrefab;
		SerializedProperty spTilePrefab;
		SerializedProperty spObstacle;
		SerializedProperty spOverlay;
		SerializedProperty spSideUnion;
		SerializedProperty spSideX;
		SerializedProperty spSideY;
		SerializedProperty spSideZ;
		SerializedProperty spRCU;
		SerializedProperty spRCX;
		SerializedProperty spRCY;
		SerializedProperty spRCZ;
		SerializedProperty spGameCamera;
		SerializedProperty spBISSO;
		SerializedProperty spCustomResolution;
		SerializedProperty spUseCustomResolution;
		
		void OnEnable()
        {
			if (bPrefab = PrefabUtility.GetPrefabType(target).Equals(PrefabType.Prefab))
                return;

			IsoMap.instance.UpdateIsometricSortingResolution();
			// IsoMap.instance.Update_TileAngle();
			spBISSO = serializedObject.FindProperty("bUseIsometricSorting");
			spTileAngle = serializedObject.FindProperty("TileAngle");
			spReferencePPU = serializedObject.FindProperty("ReferencePPU");
			bEditPrefab = false;
			spBulkPrefab = serializedObject.FindProperty("BulkPrefab");
			spTilePrefab = serializedObject.FindProperty("TilePrefab");
			spObstacle = serializedObject.FindProperty("ObstaclePrefab");
			spOverlay = serializedObject.FindProperty("OverlayPrefab");
			spSideUnion = serializedObject.FindProperty("Side_Union_Prefab");
			spSideX = serializedObject.FindProperty("Side_X_Prefab");
			spSideY = serializedObject.FindProperty("Side_Y_Prefab");
			spSideZ = serializedObject.FindProperty("Side_Z_Prefab");
			spRCU = serializedObject.FindProperty("Collider_Cube_Prefab");
			spRCX = serializedObject.FindProperty("Collider_X_Prefab");
			spRCY = serializedObject.FindProperty("Collider_Y_Prefab");
			spRCZ = serializedObject.FindProperty("Collider_Z_Prefab");
			spGameCamera = serializedObject.FindProperty("GameCamera");
			spUseCustomResolution = serializedObject.FindProperty("bCustomResolution");
			spCustomResolution = serializedObject.FindProperty("vCustomResolution");
		}

		public override void OnInspectorGUI()
        {
			if (IsoMap.IsNull || bPrefab)
            {
                base.DrawDefaultInspector();
                return;
            } 

			bool bAngleChanged = false;

            serializedObject.Update();
				
			using (new EditorGUILayout.VerticalScope())
			{
				CustomEditorGUI.NewParagraph("[Game Camera]");
				spGameCamera.objectReferenceValue = EditorGUILayout.ObjectField(
					spGameCamera.objectReferenceValue, typeof(Camera), allowSceneObjects:true);
				EditorGUILayout.Separator();

				CustomEditorGUI.NewParagraph("[Isometric Angle]");
				
				EditorGUI.BeginChangeCheck();
				spTileAngle.vector2Value = new Vector2(
					Util.CustomEditorGUI.FloatSlider("Up/Down", spTileAngle.vector2Value.x, -90f, 90f, EditorGUIUtility.currentViewWidth, true),
					Util.CustomEditorGUI.FloatSlider("Left/Right", spTileAngle.vector2Value.y, -90f, 90f, EditorGUIUtility.currentViewWidth, true));
				if (EditorGUI.EndChangeCheck())
				{
					bAngleChanged = true;
				}

				EditorGUILayout.Separator();
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Reset", GUILayout.Width(75f));
					using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightBlue))
					{
						if (GUILayout.Button("30°"))
						{
							spTileAngle.vector2Value = new Vector2(30f, -45f);
							bAngleChanged = true;
						}
						if (GUILayout.Button("35.264°"))
						{
							spTileAngle.vector2Value = new Vector2(35.264f, -45f);
							bAngleChanged = true;
						}
					}
				}

				EditorGUILayout.Separator();
				CustomEditorGUI.NewParagraph("[Ref Tile Sprite]");
				using (new EditorGUILayout.HorizontalScope())
				{
					float fWidth = 120f;
					Rect _rt = EditorGUI.IndentedRect(EditorGUI.IndentedRect(GUILayoutUtility.GetRect(fWidth, fWidth * 0.5f)));
					CustomEditorGUI.DrawSprite(_rt, IsoMap.instance.RefTileSprite, Color.clear, true, false);

					using (new EditorGUILayout.VerticalScope())
					{
						EditorGUILayout.Separator();

						spReferencePPU.floatValue = EditorGUILayout.FloatField(
							string.Format("Pixel Per Unit : Ref({0})", IsoMap.instance.RefTileSprite.pixelsPerUnit),
							spReferencePPU.floatValue);
							
						EditorGUILayout.Separator();

						EditorGUI.BeginChangeCheck();
						Sprite _newSprite = (Sprite) EditorGUILayout.ObjectField(
							IsoMap.instance.RefTileSprite, typeof(Sprite), allowSceneObjects:false);
						if (EditorGUI.EndChangeCheck())
						{
							if (_newSprite != null)
							{
								IsoMap.instance.RefTileSprite = _newSprite;
								//spReferencePPU.floatValue = IsoMap.instance.RefTileSprite.pixelsPerUnit;
							}
						}		
					}
				}
				
				if (spBISSO.boolValue)
				{
					EditorGUILayout.Separator();
					using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.HelpBox("'IsometricSortingOrder' features will no longer be available. Please turn it off.", MessageType.Error);
						spBISSO.boolValue = EditorGUILayout.ToggleLeft("Use IsometricSortingOrder", spBISSO.boolValue);
						
						if (IsoMap.instance.bUseIsometricSorting)
						{
							EditorGUI.indentLevel++;
							spUseCustomResolution.boolValue = !EditorGUILayout.ToggleLeft("Use Auto Resolution", !spUseCustomResolution.boolValue);
							EditorGUI.indentLevel--;
							if (spUseCustomResolution.boolValue)
							{
								spCustomResolution.vector3Value = Util.CustomEditorGUI.Vector3Slider(spCustomResolution.vector3Value, 
									IsoMap.vMAXResolution, "Custom Resolution of Axis", Vector3.zero, IsoMap.vMAXResolution ,EditorGUIUtility.currentViewWidth);
							}
							else
							{
								EditorGUILayout.LabelField("Resolution: " + IsoMap.instance.fResolutionOfIsometric);
							}
						}	

						if (EditorGUI.EndChangeCheck())
						{
							bAngleChanged = true;
						}		
					}
				}
			}

			EditorGUILayout.Separator();
            Util.CustomEditorGUI.NewParagraph("[Util]");
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightYellow))
				{
					if (GUILayout.Button("New Bulk"))
					{
                        IsoMap.instance.NewBulk();
					}
				}

				using (new GUIBackgroundColorScope(Util.CustomEditorGUI.Color_LightGreen))
				{
					if (GUILayout.Button("Reset Scene Camera"))
					{
                        IsoMap.instance.Update_TileAngle();
					}
				}
			}

			EditorGUILayout.Separator();
            Util.CustomEditorGUI.NewParagraph("[Prefab]");
			if (bEditPrefab = EditorGUILayout.ToggleLeft("Edit Prefab", bEditPrefab))
			{
				EditorGUILayout.LabelField("Core Object");
				EditorGUI.indentLevel++;
				spBulkPrefab.objectReferenceValue = 
					EditorGUILayout.ObjectField("Bulk", spBulkPrefab.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spTilePrefab.objectReferenceValue = 
					EditorGUILayout.ObjectField("Tile", spTilePrefab.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spObstacle.objectReferenceValue = 
					EditorGUILayout.ObjectField("Obstacle", spObstacle.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spOverlay.objectReferenceValue = 
					EditorGUILayout.ObjectField("Overlay", spOverlay.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				EditorGUILayout.Separator();
				EditorGUI.indentLevel--;

				EditorGUILayout.LabelField("Side Object");
				EditorGUI.indentLevel++;
				spSideUnion.objectReferenceValue = 
					EditorGUILayout.ObjectField("Union", spSideUnion.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spSideX.objectReferenceValue = 
					EditorGUILayout.ObjectField("Axis-X", spSideX.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spSideY.objectReferenceValue = 
					EditorGUILayout.ObjectField("Axis-Y", spSideY.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spSideZ.objectReferenceValue = 
					EditorGUILayout.ObjectField("Axis-Z", spSideZ.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				EditorGUILayout.Separator();
				EditorGUI.indentLevel--;

				EditorGUILayout.LabelField("Regular Collider Object");
				EditorGUI.indentLevel++;
				spRCU.objectReferenceValue = 
					EditorGUILayout.ObjectField("Cube", spRCU.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spRCX.objectReferenceValue = 
					EditorGUILayout.ObjectField("Plane-YZ", spRCX.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spRCY.objectReferenceValue = 
					EditorGUILayout.ObjectField("Plane-XZ", spRCY.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				spRCZ.objectReferenceValue = 
					EditorGUILayout.ObjectField("Plane-XY", spRCZ.objectReferenceValue, 
					typeof(GameObject), allowSceneObjects:false);
				EditorGUILayout.Separator();
				
			}

			serializedObject.ApplyModifiedProperties();
			if (bAngleChanged)
			{
				IsoMap.instance.Update_TileAngle();
				IsoMap.instance.Update_All_ISO();
			}
			// DrawPropertiesExcluding(serializedObject, "m_Script");
        }

    }
}