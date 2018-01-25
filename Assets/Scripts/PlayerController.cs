using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	private Vector3 destination; // variable to save the position of the destination where character would go
	private Vector3 cursorPosition; // variable to save the cursor position every frame
	[SerializeField] private float speed = 5f; // variable to control the character speed
	public int maxHealth { get; private set; } // variable to control the maximum health value
	public int currentHealth { get; private set; } // variable to save current health value
	public int damage { get; private set; } // variable to control the damage value
	[SerializeField] private int exp; // variable to control the experience point value
	private float characterDestinationGap = 1f; // variable to control the gap between the character position and the destination position
	private SpriteRenderer characterSprite; // variable to keep character sprite renderer reference
	private SpriteAnimator anim; // variable to keep character sprite animator reference
	private SpriteAnimator[] equipmentAnim; // variable to keep equipment sprite animator reference
	private bool isFirstAttackPlaying = false; // variable to check if the first attack animation is playing
	private bool isSecondAttackPlaying = false; // variable to check if the second attack animation is playing
	public EnemyController enemy; // variable to keep enemy controller reference
	private InventoryManager inventory; // variable to keep InventoryManager reference

	// Use this for initialization
	void Start () 
	{
		maxHealth = 100;
		damage = 10;
		currentHealth = maxHealth;
		destination = transform.position;
		characterSprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		anim = transform.GetChild(0).gameObject.GetComponent<SpriteAnimator>();
		equipmentAnim = transform.GetChild (0).GetComponentsInChildren<SpriteAnimator> ();
		Equipment.UpdateEquipmentAnimReferenceCallback += UpdateAnimReference;

		inventory = InventoryManager.instance;

		/* temporary code */
		inventory.Add (ObjectPool.instance.GetObject (1, 0).GetComponent<Equipment> ());
		inventory.Add (ObjectPool.instance.GetObject (2, 0).GetComponent<Equipment> ());
		inventory.Add (ObjectPool.instance.GetObject (6, 0).GetComponent<Equipment> ());
		inventory.Add (ObjectPool.instance.GetObject (7, 0).GetComponent<Equipment> ());

		inventory.items [0].Use (); // shirts equip
		inventory.items [0].Use (); // pants equip
		/* temporary code */
	}
	
	// Update is called once per frame
	void Update ()
	{
		/* update the mouse cursor position for every frame */
		CursorPositionUpdate ();

		/* if player is not dead */
		if (!IsDead ()) 
		{
			/* update the character */
			CharacterUpdate ();
		}
		else
			Die ();
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
		/* if the mouse is not over the UI */
		if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ()) 
		{
			/* update the destination position of the character given by mouse clicking */
			DestinationUpdate ();

			/* update the attack logic */
			AttackUpdate ();
		}

		/* if attack animation is not playing */
		if (!isFirstAttackPlaying && !isSecondAttackPlaying)
		{
			/* update the character position */
			MovementUpdate ();
		}
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
				inventory.AllFlipX (false);
			}
			/* if mouse position is left side of the character */
			else if (CheckQuadrant(transform.position, cursorPosition) == 2 || CheckQuadrant(transform.position, cursorPosition) == 3) 
			{
				characterSprite.flipX = true;
				inventory.AllFlipX (true);
			}

			/* move character position */
			transform.position = Vector3.MoveTowards (transform.position, destination, speed * Time.deltaTime);

			/* play run animation */
			anim.Play("RUN");

			/* play run animation for all equipment character equipped */
			for(int i = 0; i < equipmentAnim.Length; ++i)
				equipmentAnim [i].Play ("RUN", true, anim.currentFrame);
		}
		else
		{
			/* play idle animation */
			anim.Play ("IDLE");

			/* play idle animation for all equipment character equipped */
			for(int i = 0; i < equipmentAnim.Length; ++i)
				equipmentAnim [i].Play ("IDLE", true, anim.currentFrame);
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
					inventory.AllFlipX (false);
				}
				/* if enemy position is left side of the character */
				else if (CheckQuadrant(transform.position, enemy.transform.position) == 2 || CheckQuadrant(transform.position, enemy.transform.position) == 3) 
				{
					characterSprite.flipX = true;
					inventory.AllFlipX (true);
				}
			}
		}

		/* if first attack is on */ 
		if (isFirstAttackPlaying) 
		{
			/* play first attack animation */
			anim.Play ("ATTACK1", false);

			/* play first attack animation for all equipment character equipped */
			for(int i = 0; i < equipmentAnim.Length; ++i)
				equipmentAnim [i].Play ("ATTACK1", false, anim.currentFrame);
		}
		/* if second attack is on */
		else if(isSecondAttackPlaying)
		{
			/* play second attack animation */
			anim.Play ("ATTACK2", false);

			/* play first attack animation for all equipment character equipped */
			for(int i = 0; i < equipmentAnim.Length; ++i)
				equipmentAnim [i].Play ("ATTACK2", false, anim.currentFrame);
		}
	}

	void Die()
	{
		/* play die animation */
		//anim.Play ("DIE", false);

		/* play first attack animation for all equipment character equipped */
		//for(int i = 0; i < equipmentAnim.Length; ++i)
		//	equipmentAnim [i].Play ("DIE", false);
	}

	public void GetHit(int hitDamage)
	{
		/* decrease player's health by enemy's damage */
		currentHealth -= hitDamage;

		if (currentHealth <= 0)
			currentHealth = 0;
	}

	public bool IsDead()
	{
		if (currentHealth <= 0)
			return true;
		else
			return false;
	}



	/* function to be called by Equipment::Use() */
	void UpdateAnimReference()
	{
		equipmentAnim = transform.GetChild (0).GetComponentsInChildren<SpriteAnimator> ();

		/* if body animation is playing (to make both body and equipment play simultaneously) */
		if (anim.playing) 
		{
			anim.ForcePlay (anim.currentAnimation.name, false, anim.currentFrame);

			/* play current playing animation for all equipment character equipped */
			for (int i = 0; i < equipmentAnim.Length; ++i)
				equipmentAnim [i].ForcePlay (anim.currentAnimation.name, false, anim.currentFrame);
		}
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
				enemy.GetHit (damage);
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
