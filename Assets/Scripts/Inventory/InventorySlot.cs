using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour {
	
	private Item item; // variable to keep the actual item
	private UnityEngine.UI.Image icon; // variable to keep the icon image to show in the inventory
	private UnityEngine.UI.Button removeButton; // variable to keep the remove button reference

	void Awake ()
	{
		icon = transform.GetChild (0).GetChild (0).GetComponent<UnityEngine.UI.Image> ();
		removeButton = transform.GetChild (1).GetComponent<UnityEngine.UI.Button> ();
	}

	public void ChangeItem (Item newItem)
	{
		item = newItem;
		icon.sprite = item.icon;
		icon.enabled = true;
		removeButton.interactable = true;
	}

	public void ClearSlot()
	{
		item = null;
		icon.sprite = null;
		icon.enabled = false;
		removeButton.interactable = false;
	}

	public void OnRemoveButton()
	{
		InventoryManager.instance.Remove (item);
	}

	public void UseItem()
	{
		if (item != null) 
		{
			item.Use ();
		}
	}
}
