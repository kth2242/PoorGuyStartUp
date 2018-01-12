using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	private Vector3 destination; // variable to save the position of the destination where character would go
	private Vector3 cursorPosition; // variable to save the cursor position every frame
	[SerializeField] private float speed = 5f; // variable to control the character speed
	private float characterDestinationGap = 1f; // variable to control the gap between the character position and the destination position
	private SpriteRenderer characterSprite; // variable to keep character sprite renderer reference
	private SpriteRenderer pantsSprite; // variable to keep pants sprite renderer reference
	private SpriteRenderer shirtSprite; // variable to keep shirt sprite renderer reference
	private Animation anim;
	[SerializeField] private AnimationClip idleClip;
	[SerializeField] private AnimationClip runClip;

	// Use this for initialization
	void Start () 
	{
		destination = transform.position;
		characterSprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		pantsSprite = transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
		shirtSprite = transform.GetChild(0).GetChild(1).gameObject.GetComponent<SpriteRenderer>();
		anim = transform.GetChild(0).gameObject.GetComponent<Animation>();
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

		if(Physics.Raycast(ray, out hit, 1000f))
		{
			cursorPosition = hit.point;
		}
	}

	void CharacterUpdate()
	{
		/* update the destination position of the character given by mouse clicking */
		DestinationUpdate ();

		/* update the character position */
		MovementUpdate ();
	}

	void DestinationUpdate()
	{
		/* if the mouse right button is clicked */
		if (Input.GetKey (KeyCode.Mouse1))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if(Physics.Raycast(ray, out hit, 1000f))
			{
				/* if the place where the mouse is clicked is not the character position */
				if (hit.collider.tag != "Player")
				{
					destination = hit.point; // save the clicked position (destination)
					destination.y = transform.position.y; // don't want to change the y value
				}
			}
		}
	}

	void MovementUpdate()
	{
		/* if character current position is not near the destination position */
		if (Vector3.Distance (transform.position, destination) > characterDestinationGap) 
		{
			/* if mouse position is right side of the character */
			if (CheckQuadrant () == 1 || CheckQuadrant () == 4) 
			{
				characterSprite.flipX = false;
				pantsSprite.flipX = false;
				shirtSprite.flipX = false;
			}
			/* if mouse position is left side of the character */
			else if (CheckQuadrant () == 2 || CheckQuadrant () == 3) 
			{
				characterSprite.flipX = true;
				pantsSprite.flipX = true;
				shirtSprite.flipX = true;
			}

			/* move character position */
			transform.position = Vector3.MoveTowards (transform.position, destination, speed * Time.deltaTime);

			/* play run animation */
			anim.CrossFade (runClip.name);
		}
		else
			/* play idle animation */
			anim.CrossFade (idleClip.name);
	}

	int CheckQuadrant()
	{
		/* the isometric view is always rotated with 45 degree.
		   convert the x-axis and z-axis coordinates to the x-axis and y-axis rotated 45 degree.
		   the calculation method : result (x,y) = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta)) with tan(theta) = b/a
		   (a,b) is the position of the pre-converted coordinates and theta is angle between x-axis and the slope which has tan(theta) value b/a */
		/* variable to calculate efficiently */
		float precalculated_charX = transform.position.x / Mathf.Sqrt (2);
		float precalculated_charZ = transform.position.z / Mathf.Sqrt (2);
		float precalculated_mouseX = cursorPosition.x / Mathf.Sqrt (2);
		float precalculated_mouseZ = cursorPosition.z / Mathf.Sqrt (2);

		/* result (x,y)
		  = (a*sec(theta)*cos(pi/4-theta), a*sec(theta)*sin(pi/4-theta))
		  = ((a+b)/sqrt(2), (a-b)/sqrt(2)) */
		Vector2 charPos = new Vector2(precalculated_charX + precalculated_charZ, precalculated_charX - precalculated_charZ);
		Vector2 mousePos = new Vector2(precalculated_mouseX + precalculated_mouseZ, precalculated_mouseX - precalculated_mouseZ);

		/* screen y value : bottom is high, top is low */
		if (mousePos.x > charPos.x && mousePos.y <= charPos.y)
			return 1;
		else if (mousePos.x <= charPos.x && mousePos.y < charPos.y)
			return 2;
		else if (mousePos.x < charPos.x && mousePos.y >= charPos.y)
			return 3;
		else if (mousePos.x >= charPos.x && mousePos.y > charPos.y)
			return 4;
		else // mousePos == charPos
			return 0; // origin
	}
}
