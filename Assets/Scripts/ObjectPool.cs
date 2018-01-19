using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {

	#region Singleton

	public static ObjectPool instance; // variable to store the actual instance for the class object

	void Awake()
	{
		/* if object is already created */
		if (instance != null) 
		{
			Debug.LogWarning ("More than one instance of ObjectPool found!");
			return;
		}
		instance = this;
	}

	#endregion

	[SerializeField] private GameObject[] objects; // variable to keep the object reference, should be assigned in the Inspector
	[SerializeField] private int[] objectNum; // variable to control how many objects would be made
	[SerializeField] private List<GameObject>[] pool; // the actual list of the object pool
	[SerializeField] private Transform playerBody; // variable to keep the transform of the player to create the object relevant to the player

	// Use this for initialization
	void Start () 
	{
		Make ();
		//DeActivateAll ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Make()
	{
		/* variable to keep the reference of the clone object */
		GameObject cloneObject;

		/* allocate the list of the pool with the number of objects */
		pool = new List<GameObject>[objects.Length];

		/* for all objects in the pool */
		for (int i = 0; i < objects.Length; ++i) 
		{
			/* allocate the list of the object in the pool */
			pool [i] = new List<GameObject> ();

			/* for all objects in the object list */
			for (int j = 0; j < objectNum [i]; ++j) 
			{
				/* clone the object */
				cloneObject = Instantiate (objects [i]);

				/* set the parent of the clone object */
				cloneObject.transform.parent = playerBody; //this.transform;

				/* adjust the transform of the clone object */
				cloneObject.transform.localPosition = new Vector3(0, 0, 0);
				cloneObject.transform.localRotation = Quaternion.identity;
				cloneObject.transform.localScale = new Vector3 (1, 1, 1);

				/* add to the pool list */
				pool [i].Add (cloneObject);
			}
		}
	}

	public GameObject Activate(int index)
	{
		/* for applicable objects in the pool */
		for (int i = 0; i < pool [index].Count; ++i) 
		{
			/* if object is not activated */
			if (!pool [index] [i].activeSelf) 
			{
				/* activate the object and return it */
				pool [index] [i].SetActive (true);
				return pool [index] [i];
			}
		}
		/*pool [index].Add (Instantiate (objects [index]));
		pool [index] [pool [index].Count - 1].transform.parent = playerBody; //this.transform;
		pool [index] [pool [index].Count - 1].transform.localPosition = new Vector3(0, 0, 0);
		pool [index] [pool [index].Count - 1].transform.localRotation = Quaternion.identity;
		pool [index] [pool [index].Count - 1].transform.localScale = new Vector3 (1, 1, 1);
		return pool [index] [pool [index].Count - 1];*/
		return null;
	}

	public void DeActivate(GameObject deActivateObject)
	{
		deActivateObject.SetActive (false);
	}

	public void DeActivateAll()
	{
		for (int i = 0; i < pool.Length; ++i) 
		{
			for (int j = 0; j < pool [i].Count; ++j)
				DeActivate (pool[i][j]);
		}
	}

	/* Temporary function, would be modified someday */
	public GameObject GetObject(int objTypeIndex, int objOrderIndex)
	{
		return pool [objTypeIndex] [objOrderIndex];
	}

	/* Temporary function, would be deleted someday */
	public int GetIndex(GameObject obj)
	{
		const int ERROR = 9999999;

		for (int i = 0; i < pool.Length; ++i) 
		{
			if (pool [i][0] == obj) // wtf
				return i;
		}
		return ERROR;
	}
}
