using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour {
	
	private InventoryManager inventory; // variable to keep Inventorymanager reference
	private InventorySlot[] slots; // variable to store the inventory slots
	private GameObject inventoryUI; // variable to keep the Inventory gameobject reference

	// Use this for initialization
	void Start ()
	{
		inventory = InventoryManager.instance;
		slots = transform.GetChild (0).GetChild (0).GetComponentsInChildren<InventorySlot> ();
		inventoryUI = transform.GetChild (0).gameObject;

		/* assign the delegate function with UpdateUI() function */
		inventory.onItemChangedCallback += UpdateUI;
	}
	
	// Update is called once per frame
	void Update () 
	{
		/* if key for "Inventory" is pressed (could be changed in [Edit]-[Project Settings]-[Input]) */
		if (Input.GetButtonDown ("Inventory")) 
		{
			/* Toggle the visibility of the inventory */
			inventoryUI.SetActive (!inventoryUI.activeSelf);

			/* update the item existence */
			UpdateUI ();
		}		
	}

	void UpdateUI()
	{
		/* if inventory is shown */
		if(inventoryUI.activeSelf)
		{
			/* for all inventory slots */
			for (int i = 0; i < slots.Length; ++i)
			{
				/* for occupied slot, fill with the item, for non-occupied slot, clear the slot */
				if (i < inventory.items.Count)
				{
					slots [i].ChangeItem (inventory.items [i]);
				}
				else
				{
					slots [i].ClearSlot ();
				}
			}
		}
	}
}
