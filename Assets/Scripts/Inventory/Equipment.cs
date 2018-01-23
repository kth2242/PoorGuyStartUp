using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item {

	public EquipmentSlot equipmentSlot; // variable to select the type of the equipment

	public delegate void UpdateEquipmentAnimReference();
	public static UpdateEquipmentAnimReference UpdateEquipmentAnimReferenceCallback;

	public override void Use()
	{
		// base.Use ();
		EquipmentManager.instance.Equip (this);
		RemoveFromInventory ();
		UpdateEquipmentAnimReferenceCallback.Invoke ();
	}
}

public enum EquipmentSlot { Head, Chest, Legs, Weapon, Shield, Feet }