using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {

	#region Singleton

	public static EquipmentManager instance; // variable to store the actual instance for the class object

	void Awake()
	{
		/* if object is already created */
		if (instance != null) 
		{
			Debug.LogWarning ("More than one instance of EquipmentManager found!");
			return;
		}
		instance = this;
	}

	#endregion

	private Equipment[] currentEquipment; // variable to indicate the current equipment
	private InventoryManager inventory; // variable to keep the InventoryManager reference

	public delegate void OnEquipmentChanged(Equipment newItem, Equipment oldItem);
	public OnEquipmentChanged onEquipmentChangedCallback;

	// Use this for initialization
	void Start () 
	{
		/* Head, Chest, Legs, Weapon, Shield, Feet : numOfSlots = 6 */
		int numOfSlots = System.Enum.GetNames (typeof(EquipmentSlot)).Length;

		/* allocate the equipment slot */
		currentEquipment = new Equipment[numOfSlots];

		/* store the InventoryManager reference */
		inventory = InventoryManager.instance;
	}

	public void Equip (Equipment newItem)
	{
		/* variable to indicate which slot would be used depending on the type of the equipment */
		int slotIndex = (int)newItem.equipmentSlot;

		/* variable to keep previous equipped item */
		Equipment oldItem = null;

		/* if current equipment slot is already occupied (character is already equipped) */
		if (currentEquipment [slotIndex] != null)
		{
			/* store the equipment to take off */
			oldItem = currentEquipment [slotIndex];

			/* add old equipment to the inventory */
			inventory.Add (oldItem);

			/* deactivate old equipment */
			ObjectPool.instance.DeActivate (oldItem.gameObject);
		}

		/* if delegate function exists (currently not used, 2018.01.20) */
		if (onEquipmentChangedCallback != null) 
		{
			onEquipmentChangedCallback.Invoke (newItem, oldItem);
		}

		/* change the current equipment slot to the new item */
		currentEquipment [slotIndex] = newItem;

		/* Activate new item */
		int resultIndex = ObjectPool.instance.GetIndex (newItem.gameObject);
		if(resultIndex != 9999999) // ERROR_CODE, wtf
			ObjectPool.instance.Activate (resultIndex);
	}

	public void Unequip (int slotIndex)
	{
		/* if current equipment slot is already occupied (character is already equipped) */
		if (currentEquipment [slotIndex] != null) 
		{
			/* variable to keep previous equipped item */
			Equipment oldItem = currentEquipment [slotIndex];

			/* add old equipment to the inventory */
			inventory.Add (oldItem);

			/* change the current equipment slot to empty */
			currentEquipment [slotIndex] = null;

			/* if delegate function exists (currently not used, 2018.01.20) */
			if (onEquipmentChangedCallback != null) 
			{
				onEquipmentChangedCallback.Invoke (null, oldItem);
			}
		}
	}

	public void UnequipAll()
	{
		/* for all the equipment that character wear, unequip */
		for (int i = 0; i < currentEquipment.Length; ++i) 
		{
			Unequip (i);
		}
	}
		
	public Equipment GetEquipment(EquipmentSlot slot)
	{
		return currentEquipment [(int)slot];
	}
}
