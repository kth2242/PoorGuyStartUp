using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIController : MonoBehaviour {

	/* sprite GUI class that would hold sprite to draw on the screen */
	[System.Serializable]
	public class spriteGUI
	{
		public string name;
		public Texture2D texture;
		public Rect textureRect;
	}
	public spriteGUI[] sprites; // variable to hold many sprites in one GUIController class
	public Vector2 desireResolution; // variable to indicate the desire resolution

	private Rect[] changedValue; // variable to store calculated GUI rectangular with desireResolution ratio

	// Use this for initialization
	void Start () 
	{
		/* if desireResolution is not set by user */
		if (desireResolution.x == 0 && desireResolution.y == 0)
		{
			/* get the current screen resolution information */
			desireResolution.x = Screen.width;
			desireResolution.y = Screen.height;
		}

		/* allocate the variable */
		changedValue = new Rect[sprites.Length];
	}

	void OnGUI()
	{
		Draw ();
	}

	void Draw()
	{
		/* for all sprites */
		for (int i = 0; i < sprites.Length; ++i)
		{
			/* calculate the ratio */
			float ratioX = (float)Screen.width / desireResolution.x;
			float ratioY = (float)Screen.height / desireResolution.y;

			/* adjust the position and size of the GUI with calculated ratio */
			changedValue [i].x      = sprites [i].textureRect.x * ratioX;
			changedValue [i].y      = sprites [i].textureRect.y * ratioY;
			changedValue [i].width  = sprites [i].textureRect.width * ratioX;
			changedValue [i].height = sprites [i].textureRect.height * ratioY;

			/* draw sprite */
			GUI.DrawTexture (changedValue[i], sprites[i].texture); 
		}
	}

	public spriteGUI GetSprite(string name)
	{
		/* for all sprites */
		foreach (spriteGUI sprite in sprites) 
		{
			/* if the name is same as given name */
			if (sprite.name == name)
				return sprite;
		}
		return null;
	}
}
