using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	[SerializeField] private GameObject player; // variable to save player(enemy's opponent) reference
	private PlayerController playerController; // variable to save player controller reference
	[SerializeField] private float speed = 2f; // variable to control the enemy speed
	[SerializeField] private float detectRange = 4.5f; // variable to control the detecting player range
	public float combatRange { get; private set; } // variable to control the combat range
	[SerializeField] private float wanderRange = 1.5f; // variable to control the wandering range
	public int maxHealth { get; private set; } // variable to control the maximum health value
	public int currentHealth { get; private set; } // variable to save current health value
	[SerializeField] private int damage = 10; // variable to control the damage value
	[SerializeField] private int exp = 50; // variable to control the experience point value
	private SpriteRenderer enemySprite; // variable to keep enemy sprite renderer reference
	private SpriteRenderer pantsSprite; // variable to keep pants sprite renderer reference
	private SpriteRenderer shirtSprite; // variable to keep shirt sprite renderer reference
	private Vector3 destination; // variable to keep the destination to wander or patrol
	private Vector3 oldPosition; // variable to keep the initial position
	private SpriteAnimator anim; // variable to keep sprite animator reference
	private SpriteAnimator pantsAnim;
	private SpriteAnimator shirtsAnim;
    private bool isAttackPlaying = false; // variable to check if the attack animation is playing

	// Use this for initialization
	void Start () 
	{
		maxHealth = 100;
		combatRange = 0.49f;
		currentHealth = maxHealth;
		playerController = player.GetComponent<PlayerController> ();
		enemySprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		pantsSprite = transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		shirtSprite = transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>();
		anim = transform.GetChild(0).gameObject.GetComponent<SpriteAnimator>();
		pantsAnim = transform.GetChild (0).GetChild (0).GetComponent<SpriteAnimator> ();
		shirtsAnim = transform.GetChild (0).GetChild (1).GetComponent<SpriteAnimator> ();

        /* set destination position around the enemy position, the destination is within wanderRange radius of the enemy */
        destination = new Vector3(Random.Range(transform.position.x - wanderRange, transform.position.x + wanderRange),
								  transform.position.y,
								  Random.Range(transform.position.z - wanderRange, transform.position.z + wanderRange));

		/* store initial position */
		oldPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!IsDead())
		{
            if (!IsInRange(combatRange) && IsInRange(detectRange))
                Chase();
            else if (!IsInRange(combatRange))
                Wander();
            else if (!playerController.IsDead())
                Attack();
            else
            {
                anim.Play("IDLE");
				pantsAnim.Play ("IDLE");
				shirtsAnim.Play ("IDLE");
            }
        }
		else
			Die ();
	}

	public bool IsInRange(float range)
	{
		if (Vector3.Distance (transform.position, player.transform.position) < range)
			return true;
		else
			return false;
	}

	void Chase()
	{
		/* if attack animation is playing, don't move the enemy */
		if (!isAttackPlaying)
		{
			/* move the enemey to the player position */
			transform.position = Vector3.MoveTowards (transform.position, player.transform.position, speed * Time.deltaTime);

			/* play run animation */
			anim.Play ("RUN");
			pantsAnim.Play ("RUN");
			shirtsAnim.Play ("RUN");
        }

		/* if enemy position is right side of the character */
		if (CheckQuadrant(player.transform.position, transform.position) == 1 || CheckQuadrant(player.transform.position, transform.position) == 4) 
		{
			enemySprite.flipX = true;
			pantsSprite.flipX = true;
			shirtSprite.flipX = true;
		}

		/* if enemy position is left side of the character */
		else if (CheckQuadrant(player.transform.position, transform.position) == 2 || CheckQuadrant(player.transform.position, transform.position) == 3) 
		{
			enemySprite.flipX = false;
			pantsSprite.flipX = false;
			shirtSprite.flipX = false;
		}
	}

	void Wander()
	{
		/* if enemy get to the destination position */
		if (Vector3.Distance (transform.position, destination) < 1f) 
		{
			/* set new destination position */
			destination = new Vector3 (Random.Range (oldPosition.x - wanderRange, oldPosition.x + wanderRange),
							   		   transform.position.y,
									   Random.Range (oldPosition.z - wanderRange, oldPosition.z + wanderRange));
		}
		/* move the enemey to the destination position */
		transform.position = Vector3.MoveTowards (transform.position, destination, speed * Time.deltaTime);

		/* play run animation */
		anim.Play ("RUN");
		pantsAnim.Play ("RUN");
		shirtsAnim.Play ("RUN");

        /* if destination position is right side of the enemy */
        if (CheckQuadrant(transform.position, destination) == 1 || CheckQuadrant(transform.position, destination) == 4) 
		{
			enemySprite.flipX = false;
			pantsSprite.flipX = false;
			shirtSprite.flipX = false;
		}

		/* if destination position is left side of the enemy */
		else if (CheckQuadrant(transform.position, destination) == 2 || CheckQuadrant(transform.position, destination) == 3) 
		{
			enemySprite.flipX = true;
			pantsSprite.flipX = true;
			shirtSprite.flipX = true;
		}
	}

	void Attack()
	{
		/* if player is in combat range, turn on the attack signal */
		if (IsInRange (combatRange))
			isAttackPlaying = true;

        /* if attack signal is on, play attack animation */
        if (isAttackPlaying)
        {
            anim.Play("ATTACK", false);
			pantsAnim.Play ("ATTACK", false);
			shirtsAnim.Play ("ATTACK", false);
        }

	}

	void Die()
	{
		/* play die animation */
		//anim.Play ("DIE", false);
	}

	public void GetHit(int hitDamage)
	{
		/* decrease enemy health by player's damage */
		currentHealth -= hitDamage;

		if (currentHealth <= 0)
			currentHealth = 0;
	}

	bool IsDead()
	{
		if (currentHealth <= 0)
			return true;
		else
			return false;
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_DisableAttack()
	{
		isAttackPlaying = false;
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_Impact()
	{
		/* if player is in combat range */
		if (IsInRange (combatRange))
			playerController.GetHit (damage);
	}

	/* function to be called by sprite animator trigger */
	public void Trigger_Die()
	{
		Destroy (gameObject);
	}

	void OnMouseOver()
	{
		/* if mouse is over the enemy, set that enemy as target enemy */
		playerController.enemy = this;
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
