using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthBar : MonoBehaviour {
	
	[SerializeField] private float healthPercentage = 1f; // 1 = 100%, min : 0, max : 1

	private GUIController guiController; // variable to keep GUIController reference
	private GUIController.spriteGUI healthbar; // variable to keep spriteGUI reference
	private float valueStorage; // variable to store origin scale x of the texture
	private PlayerController player; // variable to keep PlayerController reference

	// Use this for initialization
	void Start () {
		guiController = GetComponent<GUIController> ();
		healthbar = guiController.GetSprite ("HealthBar");
		valueStorage = healthbar.textureRect.width;
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController>();
	}

	void OnGUI()
	{
		healthPercentage = (float)player.currentHealth / player.maxHealth;
		healthbar.textureRect.width = valueStorage * healthPercentage;
	}
}
