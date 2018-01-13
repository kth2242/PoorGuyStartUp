using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	private Vector3 destination; // variable to save the position of the destination where character would go
	private Vector3 cursorPosition; // variable to save the cursor position every frame
	[SerializeField] private float speed = 5f; // variable to control the character speed
	[SerializeField] private int maxHealth = 100; // variable to control the maximum health value
	[SerializeField] private int currentHealth; // variable to save current health value
	public int damage { get; private set; } // variable to control the damage value
	[SerializeField] private int exp; // variable to control the experience point value
	private float characterDestinationGap = 1f; // variable to control the gap between the character position and the destination position
	private SpriteRenderer characterSprite; // variable to keep character sprite renderer reference
	private SpriteRenderer pantsSprite; // variable to keep pants sprite renderer reference
	private SpriteRenderer shirtSprite; // variable to keep shirt sprite renderer reference
	private SpriteAnimator anim; // variable to keep sprite animator reference
	private bool isFirstAttackPlaying = false; // variable to check if the first attack animation is playing
	private bool isSecondAttackPlaying = false; // variable to check if the second attack animation is playing
	public EnemyController enemy;

	// Use this for initialization
	void Start () 
	{
		damage = 10;
		currentHealth = maxHealth;
		destination = transform.position;
		characterSprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		pantsSprite = transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		shirtSprite = transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>();
		anim = transform.GetChild(0).gameObject.GetComponent<SpriteAnimator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		/* update the mouse cursor position for every frame */
		CursorPositionUpdate ();

		/* update the character */
		CharacterUpdate ();
	}

	void CursorPositionUpdate()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if(Physics.Raycast(ray, out hit, 100f))
		{
			cursorPosition = hit.point;
		}
	}

	void CharacterUpdate()
	{
		/* update the destination position of the character given by mouse clicking */
		DestinationUpdate ();

		/* if attack animation is not playing */
		if (!isFirstAttackPlaying && !isSecondAttackPlaying)
		{
			/* update the character position */
			MovementUpdate ();
		}

		/* update the attack logic */
		AttackUpdate ();
	}

	void DestinationUpdate()
	{
		/* if the mouse right button is clicked */
		if (Input.GetKey (KeyCode.Mouse1))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if(Physics.Raycast(ray, out hit, 100f))
			{
				/* if the place where the mouse is clicked is not the character position */
				if (hit.collider.tag != "Player")
				{
					destination = hit.point; // save the clicked position (destination)
					destination.y = transform.position.y; // don't want to change the y value
				}
			}
		}

		/* if attack animation is playing, hold the character position */
		if (isFirstAttackPlaying || isSecondAttackPlaying)
			destination = transform.position;
	}

	void MovementUpdate()
	{
		/* if character current position is not near the destination position */
		if (Vector3.Distance (transform.position, destination) > characterDestinationGap) 
		{
			/* if mouse position is right side of the character */
			if (CheckQuadrant(transform.position, cursorPosition) == 1 || CheckQuadrant(transform.position, cursorPosition) == 4) 
			{
				characterSprite.flipX = false;
				pantsSprite.flipX = false;
				shirtSprite.flipX = false;
			}
			/* if mouse position is left side of the character */
			else if (CheckQuadrant(transform.position, cursorPosition) == 2 || CheckQuadrant(transform.position, cursorPosition) == 3) 
			{
				characterSprite.flipX = true;
				pantsSprite.flipX = true;
				shirtSprite.flipX = true;
			}

			/* move character position */
			transform.position = Vector3.MoveTowards (transform.position, destination, speed * Time.deltaTime);

			/* play run animation */
			anim.Play("RUN");
		}
		else
		{
			/* play idle animation */
			anim.Play ("IDLE");
		}
	}

	void AttackUpdate()
	{
		/* if the mouse left button is clicked */
		if (Input.GetKeyDown(KeyCode.Mouse0)) 
		{
			/* if first attack animation is not playing */
			if (!isFirstAttackPlaying && !isSecondAttackPlaying) 
			{
				isFirstAttackPlaying = true;
				isSecondAttackPlaying = false;
			}
			/* if first attack animation is playing */
			else if(isFirstAttackPlaying && !isSecondAttackPlaying)
			{
				isFirstAttackPlaying = false;
				isSecondAttackPlaying = true;
			}

			if(enemy)
			{
				/* if enemy position is right side of the character */
				if (CheckQuadrant(transform.position, enemy.transform.position) == 1 || CheckQuadrant(transform.position, enemy.transform.position) == 4) 
				{
					characterSprite.flipX = false;
					pantsSprite.flipX = false;
					shirtSprite.flipX = false;
				}
				/* if enemy position is left side of the character */
				else if (CheckQuadrant(transform.position, enemy.transform.position) == 2 || CheckQuadrant(transform.position, enemy.transform.position) == 3) 
				{
					characterSprite.flipX = true;
					pantsSprite.flipX = true;
					shirtSprite.flipX = true;
				}
			}
		}

		/* if first attack is on, play first attack animation. if second attack is on, play second attack animation */
		if(isFirstAttackPlaying)
			anim.Play ("ATTACK1", false);
		else if(isSecondAttackPlaying)
			anim.Play ("ATTACK2", false);
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_DisableFirstAttack()
	{
		isFirstAttackPlaying = false;
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_DisableSecondAttack()
	{
		isSecondAttackPlaying = false;
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_Impact()
	{
		/* if there is focusing enemy */
		if (enemy)
		{
			/* if enemey is in combat range */
			if (enemy.IsInRange (enemy.combatRange))
				enemy.GetHit ();
		}
	}

	int CheckQuadrant(Vector3 origin, Vector3 target)
	{
		/* the isometric view is always rotated with 45 degree.
		   convert the x-axis and z-axis coordinates to the x-axis and y-axis rotated 45 degree.
		   the calculation method : result (x,y) = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta)) with tan(theta) = b/a
		   (a,b) is the position of the pre-converted coordinates and theta is angle between x-axis and the slope which has tan(theta) value b/a */
		/* variable to calculate efficiently */
		float precalculated_originX = origin.x / Mathf.Sqrt (2);
		float precalculated_originZ = origin.z / Mathf.Sqrt (2);
		float precalculated_targetX = target.x / Mathf.Sqrt (2);
		float precalculated_targetZ = target.z / Mathf.Sqrt (2);

		/* result (x,y)
		  = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta))
		  = ((a+b)/sqrt(2), (a-b)/sqrt(2)) */
		Vector2 originPos = new Vector2(precalculated_originX + precalculated_originZ, precalculated_originX - precalculated_originZ);
		Vector2 targetPos = new Vector2(precalculated_targetX + precalculated_targetZ, precalculated_targetX - precalculated_targetZ);

		/* screen y value : bottom is high, top is low */
		if (targetPos.x > originPos.x && targetPos.y <= originPos.y)
			return 1;
		else if (targetPos.x <= originPos.x && targetPos.y < originPos.y)
			return 2;
		else if (targetPos.x < originPos.x && targetPos.y >= originPos.y)
			return 3;
		else if (targetPos.x >= originPos.x && targetPos.y > originPos.y)
			return 4;
		else // targetPos == originPos
			return 0; // origin
	}
}
