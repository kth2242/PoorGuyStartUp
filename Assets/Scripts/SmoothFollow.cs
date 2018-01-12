using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
	[SerializeField] private Transform target; // variable to save target's transform reference
	[SerializeField] private float distance; // variable to control the distance
	[SerializeField] private float speed = 2f; // variable to control camera's speed to follow target
	private Camera cam;

	void Start()
	{
		cam = GetComponent<Camera> ();
		distance = cam.orthographicSize;
	}

	void LateUpdate()
	{		
		/* assign the size of the camera with the distance value */
		cam.orthographicSize = distance;

		/* if camera has a target to follow, move position of the camera */
		if (target)
			transform.position = Vector3.Lerp (transform.position, target.position, speed * Time.deltaTime);
	}
}