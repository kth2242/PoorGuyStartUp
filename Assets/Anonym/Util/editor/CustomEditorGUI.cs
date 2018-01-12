using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Sprites;

namespace Anonym.Util
{
    public partial class CustomEditorGUI
    {
        public static Color Color_Tile = new Color(0.72f, 1, 0.69f);
        public static Color Color_Overlay = new Color(0.69f, 1, 0.99f);
        public static Color Color_Obstacle = new Color(1, 0.69f, 0.87f);
        public static Color Color_Side = new Color(0.8f, 0.79f, 0.82f);

        public static Color Color_LightRed = new Color(0.89f, 0.80f, 0.80f);
        public static Color Color_LightGreen = new Color(0.80f, 0.89f, 0.81f);
        public static Color Color_LightBlue = new Color(0.80f, 0.88f, 0.89f);

        public static Color Color_LightMagenta = new Color(0.84f, 0.407f, 0.45f);
        public static Color Color_LightYellow = new Color(0.945f, 0.835f, 0.305f);



        public static void ShowPackInfo(Sprite _sprite, GUILayoutOption _option)
        {
            if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
                packMSG();
            else if (EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOn
                    || EditorSettings.spritePackerMode == SpritePackerMode.BuildTimeOnly)
                showPackInfo_Legacy(_sprite, _option);
#if UNITY_2017_1_OR_NEWER
            else if (EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas
                    || EditorSettings.spritePackerMode == SpritePackerMode.BuildTimeOnlyAtlas)
                showPackInfo_Atlas(_sprite, _option);
#endif
        }
        static void packMSG()
        {
            EditorGUILayout.HelpBox("Sprite packs such as\n[Atlas] or [Legacy Pack]\n" +
                    "can help reduce rendering time.\n" +
                    "File Menu: Edit -> Project Settings -> Editor -> Sprite Packer Mode\n" + 
                    "If it is already set, it will be visible after the first build.", MessageType.None);
        }
        static void showPackInfo_Atlas(Sprite _sprite, GUILayoutOption _option)
        {
            Texture2D _spriteTexture = null;
            if (_sprite.packed)
                _spriteTexture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(_sprite, true);
            if (_spriteTexture != null)
            {
                EditorGUILayout.LabelField("[SpriteAtlas]", EditorStyles.boldLabel, _option);
                EditorGUI.DrawTextureTransparent(
                    EditorGUILayout.GetControlRect(
                        new GUILayoutOption[] { _option, GUILayout.MaxHeight(150) }),
                        _spriteTexture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.LabelField("[No Atlas]", EditorStyles.boldLabel, _option);
                packMSG();
            }

        }
        static void showPackInfo_Legacy(Sprite _sprite, GUILayoutOption _option)
        {
            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_sprite));
            ti.spritePackingTag = EditorGUILayout.TextField(ti.spritePackingTag, _option);

            EditorGUILayout.LabelField("[Packing Tag]", EditorStyles.boldLabel, _option);
            Texture2D _texture = SpriteUtility.GetSpriteTexture(_sprite, _sprite.packed);
            if (_texture != null)
            {
                if (_sprite.packed)
                {
                    EditorGUI.DrawTextureTransparent(
                        EditorGUILayout.GetControlRect(
                            new GUILayoutOption[] { _option, GUILayout.MaxHeight(150) }),
                        _texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    string _msg = !string.IsNullOrEmpty(ti.spritePackingTag)
                        ? "You have to build to see a Packed Texture."
                        : "If you set the same [Packing Tag] to sprites used in the map, render speed will be faster.";
                    EditorGUI.HelpBox(
                        EditorGUILayout.GetControlRect(
                            new GUILayoutOption[] { _option, GUILayout.MaxHeight(75) }), _msg, MessageType.Info);
                }
            }
        }

        public static void NewParagraph(string _msg, params GUILayoutOption[] options)
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField(_msg, EditorStyles.boldLabel, options);
            EditorGUI.indentLevel++;
        }

        public static void NewHelpBox(string _msg, MessageType _type, int _indextLevel)
        {
            EditorGUI.indentLevel += _indextLevel;
            EditorGUILayout.HelpBox(_msg, _type);
            EditorGUI.indentLevel -= _indextLevel;
        }

        public static bool Undo_Change_Sprite(SpriteRenderer _sprr, Sprite _newSprite)
        {
            if (_newSprite != _sprr.sprite)
            {
                Undo.RecordObject(_sprr, "Sprite Changed");
                _sprr.sprite = _newSprite;
                return true;
            }
            return false;
        }

        public static GameObject Undo_Instantiate(GameObject _prefab, Transform _transform, string _actionName, bool _buttonAction)
        {
            if (!_buttonAction || GUILayout.Button(_actionName))
            {
                return Undo_Instantiate(_prefab, _transform, _actionName);
            }
            return null;
        }

        static GameObject Undo_Instantiate(GameObject _prefab, Transform _transform, string _actionName)
        {
            GameObject _obj = GameObject.Instantiate(_prefab, _transform, false);
            Undo.RegisterCreatedObjectUndo(_obj, _actionName);
            return _obj;
        }
        // public static void DrawTexture(Rect _DrawRect, SpriteRenderer sprr, bool _bSimpleDraw = false)
        // {
        //     if (sprr == null)
        //     {
        //         EditorGUI.LabelField(_DrawRect, "No Texture!");
        //         return;
        //     }

        //     DrawSprite(_DrawRect, sprr.sprite, _bSimpleDraw);
        // }

        public static void DrawSprite(Rect _DrawRect, Sprite _sprite, Color _color, bool _bSquare, bool _bSimpleDraw)
        {
            if (_sprite == null || _sprite.texture == null)
            {
                EditorGUI.LabelField(_DrawRect, "No Texture!");
                return;
            }

            if (_bSquare)
                _DrawRect.width = _DrawRect.height = Mathf.Min(_DrawRect.width, _DrawRect.height);
            Texture texture = _sprite.texture;

            bool bRectPacked = _sprite.packed && _sprite.packingMode == SpritePackingMode.Rectangle;
            Rect tr = bRectPacked ? _sprite.textureRect : _sprite.rect;
            Rect _SourceRect = new Rect(tr.x / texture.width, tr.y / texture.height,
                tr.width / texture.width, tr.height / texture.height);

            if (_bSimpleDraw)
            {
                float fRatio = tr.width / tr.height;
                if (fRatio > 1)
                    _DrawRect.height /= fRatio;
                else if (fRatio < 1)
                    _DrawRect.width *= fRatio;
            }

            bool bDrawColorRect = !_SourceRect.Equals(new Rect(0,0,1,1));

#if !UNITY_2017_1_OR_NEWER
            bDrawColorRect = true;
#endif

            if (!_color.Equals(Color.clear))
            {
                if (bDrawColorRect)
                {
                    GUI.DrawTextureWithTexCoords(_DrawRect, texture, _SourceRect);
                    Rect _rt = new Rect();
                    float _size = Mathf.Min(_DrawRect.width, _DrawRect.height) * 0.25f;
                    _rt.x = (_DrawRect.xMin + _DrawRect.xMax - _size) * 0.5f;
                    _rt.y = (_DrawRect.yMin + _DrawRect.yMax - _size) * 0.5f;
                    _rt.width = _rt.height = _size;
                    EditorGUI.DrawRect(_rt, _color);
                }
#if UNITY_2017_1_OR_NEWER
                else
                    GUI.DrawTexture(_DrawRect, texture, ScaleMode.StretchToFill, true, 1, _color, 0, 0);
#endif
            }
            else
            {
                GUI.DrawTextureWithTexCoords(_DrawRect, texture, _SourceRect);
            }

            if(EditorApplication.isPlaying && _sprite.packed)
                GUI.Label(_DrawRect, "Playing Mode\nPacked Texture");
        }

        public static Vector2 Vector2Slider(Vector2 _v2Value, Vector2 _v2ResetValue,
            string _label, Vector2 _vMin, Vector2 _vMax, float _Editor_MAXWidth)
        {
            EditorGUILayout.LabelField(_label, EditorStyles.boldLabel, GUILayout.MaxWidth(_Editor_MAXWidth));

            using (new EditorGUILayout.VerticalScope())
            {
                float _fSpcaeHeight = EditorGUIUtility.singleLineHeight / 3f;
                GUILayout.Space(_fSpcaeHeight);                
                _v2Value.x = FloatSlider("X", _v2Value.x, _vMin.x, _vMax.x, _Editor_MAXWidth);
                GUILayout.Space(_fSpcaeHeight);                
                _v2Value.y = FloatSlider("Y", _v2Value.y, _vMin.y, _vMax.y, _Editor_MAXWidth);
                GUILayout.Space(_fSpcaeHeight);
            }
            _v2ResetValue = Vector3.Max(Vector3.Min(_v2ResetValue, _vMax), _vMin);

            EditorGUILayout.Separator();
            using (new GUIBackgroundColorScope(Color_LightBlue))
            if (GUILayout.Button("Reset " + _v2ResetValue, GUILayout.MaxWidth(_Editor_MAXWidth)))
            {
                _v2Value = _v2ResetValue;
            }
            return _v2Value;
        }
        public static Vector3 Vector3Slider(Vector3 _v3Value, Vector3 _v3ResetValue,
            string _label, Vector3 _vMin, Vector3 _vMax, float _Editor_MAXWidth)
        {
            EditorGUILayout.LabelField(_label, EditorStyles.boldLabel, GUILayout.MaxWidth(_Editor_MAXWidth));

            using (new EditorGUILayout.VerticalScope())
            {
                _v3Value.x = FloatSlider("X", _v3Value.x, _vMin.x, _vMax.x, _Editor_MAXWidth);
                _v3Value.y = FloatSlider("Y", _v3Value.y, _vMin.y, _vMax.y, _Editor_MAXWidth);
                _v3Value.z = FloatSlider("Z", _v3Value.z, _vMin.z, _vMax.z, _Editor_MAXWidth);
            }
            _v3ResetValue = Vector3.Max(Vector3.Min(_v3ResetValue, _vMax), _vMin);

            EditorGUILayout.Separator();
            using (new GUIBackgroundColorScope(Color_LightBlue))
            if (GUILayout.Button("Reset " + _v3ResetValue, GUILayout.MaxWidth(_Editor_MAXWidth)))
            {
                _v3Value = _v3ResetValue;
            }
            return _v3Value;
        }
        public static float FloatSlider(string sLabel, float fValue, float fMin, float fMax, float _fMaxWidth, bool _bIndent = false)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect _rt = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(_fMaxWidth));
                if (_bIndent)
                    _rt = EditorGUI.IndentedRect(_rt);
                fValue = FloatSlider(_rt, sLabel, fValue, fMin, fMax);
            }
            return fValue;
        }

        public static float FloatSlider(Rect _rt, string sLabel, float fValue, float fMin, float fMax)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent _label = new GUIContent(sLabel);
                Vector2 textSize = GUI.skin.label.CalcSize(_label);
                Rect _rtLabel = _rt;
                _rtLabel.width = textSize.x;
                _rt.xMin = _rtLabel.xMax;
                Rect[] _rts = _rt.Division(new float[] { 0.025f, 0.7f, 0.025f, 0.25f }, null);

                GUI.Label(_rtLabel, _label);
                fValue = GUI.HorizontalSlider(_rts[1], fValue, fMin, fMax);
                fValue = Mathf.Clamp(EditorGUI.FloatField(_rts[3], fValue), fMin, fMax);
            }
            return fValue;
        }

        public static void SceneViewAligne()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (SceneView.lastActiveSceneView == null)
                return;

            Camera cam = sceneView.camera;

            if (cam.orthographic == false)
            {
                cam.orthographic = true;
                cam.orthographicSize = Screen.height / 2;
            }

            Vector3 position = sceneView.pivot;
            position.x = 0;
            position.y = 0;
            position.z = -Screen.height / 2f / Mathf.Tan(cam.fieldOfView / 2 * Mathf.PI / 180);

            sceneView.rotation = new Quaternion(0, 0, 0, 1);

            sceneView.Repaint();
        }

        public static float FrameCollider(Camera cam, GameObject target)
        {
            MeshCollider c = target.GetComponent<MeshCollider>();
            return calc_FrameDistance(cam, (c.bounds.max - c.bounds.center).magnitude);
        }
        public static float FrameRenderer(Camera cam, GameObject target)
        {
            Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
            foreach (var renderer in target.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return FrameBounds(cam, bounds);
        }
        public static float FrameBounds(Camera cam, Bounds bounds)
        {
            return calc_FrameDistance(cam, bounds.size.magnitude);
        }
        static float calc_FrameDistance(Camera cam, float radius)
        {
            // D = R / sin( FOV/2 );
            var fov = cam.fieldOfView;
            var d = radius / Mathf.Sin(Mathf.Deg2Rad * (fov * 0.5f));
            return d + cam.nearClipPlane;
        }
        public static void AddToSelection(GameObject _obj)
        {
            GameObject[] _gameObjects = new GameObject[Selection.gameObjects.Length + 1];
            Selection.gameObjects.CopyTo(_gameObjects, 0);
            _gameObjects[_gameObjects.Length - 1] = _obj;
            Selection.objects = _gameObjects;
        }
        public static void ComSelector<T>(T _target, string _msg) where T : Component
        {
            if (_target == null || _target.gameObject == null)
                return;

            float iWidth = EditorGUIUtility.currentViewWidth / 2 - 4;
            using (new EditorGUILayout.HorizontalScope())
            {
                //using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(_target, typeof(GameObject), allowSceneObjects: true,
                        options: GUILayout.MaxWidth(iWidth * 1.25f));
                }
                using (new GUIBackgroundColorScope(CustomEditorGUI.Color_LightMagenta))
                {
                    if (GUILayout.Button(_msg, GUILayout.MinWidth(iWidth * 0.5f)))
                        Selection.activeGameObject = _target.gameObject;
                }
            }
        }
    }
}