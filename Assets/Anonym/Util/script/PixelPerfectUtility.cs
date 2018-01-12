using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Util
{
	public static class PixelPerfectUtility {
#if UNITY_EDITOR
		public static float ReferencePPUScale(this Sprite _sprite, float _ReferencePPU)
		{
			if (_ReferencePPU > 0)
				return _sprite.pixelsPerUnit / _ReferencePPU;
			else 
				return 1;
		}
		
		public static float PPU(this Sprite _sprite)
		{
			TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_sprite));
			if (ti == null)
				return 1;
			return ti.spritePixelsPerUnit;
		}
#endif

	}
}