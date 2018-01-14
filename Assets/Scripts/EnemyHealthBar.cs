using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour {

	[SerializeField] private float healthPercentage = 1f; // 1 = 100%, min : 0, max : 1

	private EnemyController enemyController; // variable to keep EnemyController reference
	private SpriteRenderer spriteRenderer; // variable to keep SpriteRenderer reference

	/* [developer log]
	   I don't know how to find the sprite UI width that is not on the canvas.
	   I didn't use canvas because of the cost of using canvas. It's too high.
	   I already tried code below.

	   spriteRender.size.magnitude
	   spriteRenderer.transform.localScale.x
	   spriteRenderer.transform.localScale.magnitude
	   spriteRenderer.sprite.border.magnitude
	   spriteRenderer.sprite.bounds.extents.x
	   spriteRenderer.sprite.bounds.extents.magnitude
	   spriteRenderer.sprite.bounds.max.x - spriteRenderer.sprite.bounds.min.x
	   spriteRenderer.sprite.bounds.max.magnitude - spriteRenderer.sprite.bounds.min.magnitude
	   spriteRenderer.sprite.bounds.size.magnitude
	   spriteRenderer.sprite.rect.width
	   spriteRenderer.sprite.rect.size.magnitude
	   spriteRenderer.sprite.rect.max.x - spriteRenderer.sprite.rect.min.x
	   spriteRenderer.sprite.rect.max.magnitude - spriteRenderer.sprite.rect.min.magnitude
	   spriteRenderer.sprite.rect.xMax - spriteRenderer.sprite.rect.xMin;
	   spriteRenderer.sprite.texture.width
	   spriteRenderer.sprite.textureRect.size.magnitude
	   spriteRenderer.sprite.textureRect.width
	   spriteRenderer.sprite.textureRect.max.x - spriteRenderer.sprite.textureRect.min.x
	   spriteRenderer.sprite.textureRect.max.magnitude - spriteRenderer.sprite.textureRect.min.magnitude
	   spriteRenderer.sprite.textureRect.xMax - spriteRenderer.sprite.textureRect.xMin
	   spriteRenderer.sprite.textureRect.x

	   All code above doesn't work properly, and I found out that 1.7f is pretty fit well on the features.
	   Possible reason : Because this game is isometric game, the sprite should be rotated as 30 degree x-axis and -45 degree y-axis
	                     Sprite width can be distorted while rotating (personal opinion)
	*/	
	private float adjustValue = 1.7f; // sprite width

	// Use this for initialization
	void Start () 
	{
		enemyController = transform.parent.parent.GetComponent<EnemyController> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	void OnGUI()
	{
		healthPercentage = (float)enemyController.currentHealth / enemyController.maxHealth;

		/* GUI.DrawTexture function draws sprites with reference to top-left origin
		   However, enemy's GUI is not using GUI.DrawTexture method.
		   So, origin is the center of the sprite image
		   If scale of the sprite is shrinked, both left and right side is move toward the center of the sprite */
		transform.localScale = new Vector3 (healthPercentage, 1f, 1f);
		transform.localPosition = new Vector3(-(1-transform.localScale.x) * adjustValue, 0, 0);
	}
}
