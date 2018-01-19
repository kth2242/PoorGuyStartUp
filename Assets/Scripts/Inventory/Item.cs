using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

	//[SerializeField] private string name = "New Item";
	public Sprite icon = null; // variable to show in the inventory slot

	// abstract, should be overrided
	public virtual void Use()
	{
	}

	public void RemoveFromInventory()
	{
		InventoryManager.instance.Remove (this);
	}
}
