using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	[SerializeField] private GameObject player;
	[SerializeField] private float speed = 2f;
	[SerializeField] private float detectRange = 4.5f;
	[SerializeField] private float combatRange = 1f;
	[SerializeField] private int maxHealth = 100;
	[SerializeField] private int currentHealth;
	[SerializeField] private int damage = 10;
	[SerializeField] private int exp = 50;
	private SpriteRenderer enemySprite; // variable to keep enemy sprite renderer reference
	private SpriteRenderer pantsSprite; // variable to keep pants sprite renderer reference
	private SpriteRenderer shirtSprite; // variable to keep shirt sprite renderer reference

	// Use this for initialization
	void Start () 
	{
		currentHealth = maxHealth;
		enemySprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		pantsSprite = transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		shirtSprite = transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!IsInRange (combatRange) && IsInRange(detectRange))
			Chase ();
	}

	bool IsInRange(float range)
	{
		if (Vector3.Distance (transform.position, player.transform.position) < range)
			return true;
		else
			return false;
	}

	void Chase()
	{
		transform.position = Vector3.MoveTowards (transform.position, player.transform.position, speed * Time.deltaTime);

		/* if enemy position is right side of the character */
		if (CheckQuadrant () == 1 || CheckQuadrant () == 4) 
		{
			enemySprite.flipX = true;
			pantsSprite.flipX = true;
			shirtSprite.flipX = true;
		}

		/* if enemy position is left side of the character */
		else if (CheckQuadrant () == 2 || CheckQuadrant () == 3) 
		{
			enemySprite.flipX = false;
			pantsSprite.flipX = false;
			shirtSprite.flipX = false;
		}
	}

	int CheckQuadrant()
	{
		/* the isometric view is always rotated with 45 degree.
		   convert the x-axis and z-axis coordinates to the x-axis and y-axis rotated 45 degree.
		   the calculation method : result (x,y) = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta)) with tan(theta) = b/a
		   (a,b) is the position of the pre-converted coordinates and theta is angle between x-axis and the slope which has tan(theta) value b/a */
		/* variable to calculate efficiently */
		float precalculated_charX = player.transform.position.x / Mathf.Sqrt (2);
		float precalculated_charZ = player.transform.position.z / Mathf.Sqrt (2);
		float precalculated_enemyX = transform.position.x / Mathf.Sqrt (2);
		float precalculated_enemyZ = transform.position.z / Mathf.Sqrt (2);

		/* result (x,y)
		  = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta))
		  = ((a+b)/sqrt(2), (a-b)/sqrt(2)) */
		Vector2 charPos = new Vector2(precalculated_charX + precalculated_charZ, precalculated_charX - precalculated_charZ);
		Vector2 enemyPos = new Vector2(precalculated_enemyX + precalculated_enemyZ, precalculated_enemyX - precalculated_enemyZ);

		/* screen y value : bottom is high, top is low */
		if (enemyPos.x > charPos.x && enemyPos.y <= charPos.y)
			return 1;
		else if (enemyPos.x <= charPos.x && enemyPos.y < charPos.y)
			return 2;
		else if (enemyPos.x < charPos.x && enemyPos.y >= charPos.y)
			return 3;
		else if (enemyPos.x >= charPos.x && enemyPos.y > charPos.y)
			return 4;
		else // enemyPos == charPos
			return 0; // origin
	}
}
