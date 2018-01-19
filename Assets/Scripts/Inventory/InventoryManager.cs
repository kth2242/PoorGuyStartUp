using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {

	#region Singleton

	public static InventoryManager instance; // variable to store the actual instance for the class object

	void Awake()
	{
		/* if object is already created */
		if (instance != null) 
		{
			Debug.LogWarning ("More than one instance of Inventory found!");
			return;
		}
		instance = this;
	}

	#endregion

	public delegate void OnItemChanged ();
	public OnItemChanged onItemChangedCallback;

	private int space = 20; // variable to keep the number of the inventory slot, should be same value as transform.GetChild (0).GetChild (0).GetComponentsInChildren<InventorySlot> ();
	public List<Item> items { get; private set; } // variable to store items which inventory would contain
	private List<SpriteRenderer> itemsSprites; // variable to keep the sprite renderer reference to flip the sprite in real time

	void Start ()
	{
		items = new List<Item>();
		itemsSprites = new List<SpriteRenderer> ();

		for (int i = 0; i < items.Count; ++i) 
		{
			itemsSprites.Add(items [i].transform.GetComponent<SpriteRenderer> ());
		}
	}

	// To use : Inventory.instance.Add(item); (recommended - singleton pattern)
	//          FindObjectofType<Inventory>().Add(item); (not recommended)
	public void Add (Item item)
	{
		/* if there is not enough space to add item in the inventory */
		if (items.Count >= space) 
		{
			Debug.Log ("Not enough room");
			return;
		}

		/* if not, add the item in the inventory */
		items.Add (item);

		/* keep the sprite renderer reference */
		itemsSprites.Add(item.transform.GetComponent<SpriteRenderer> ());

		/* if delegate function exists (InventorySlot::UpdateUI()) */
		if(onItemChangedCallback != null)
			onItemChangedCallback.Invoke ();
	}

	public void Remove (Item item)
	{
		/* delete the item in the list */
		items.Remove (item);

		/* if delegate function exists (InventorySlot::UpdateUI()) */
		if(onItemChangedCallback != null)
			onItemChangedCallback.Invoke ();
	}

	/* Temporary function, may be modified someday */
	public void AllFlipX(bool expression)
	{
		for (int i = 0; i < itemsSprites.Count; ++i) 
		{
			itemsSprites [i].flipX = expression;
		}
	}
}
