﻿using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

	// Components
	SpriteRenderer sRend;
	Collider2D pickupCollider;

	// Properties

	// If true, item can be put into deep pocket
	public bool smallItem;

	// Weight of item. Prevents too many items from being held in a backpack
	public float weight;

	void Start () {
	}

	void Update () {
	
	}

	// If a Entity picks up an item, it is moved in the heirarchy to a child of their gameobject
	public virtual void PickupItem(Entity c) {
		// Add code to add the item to the backpack
		transform.parent = c.transform;
	}
}
