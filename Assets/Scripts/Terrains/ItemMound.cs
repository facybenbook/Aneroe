﻿using UnityEngine;
using System.Collections;

public class ItemMound : Tile {

	// Item held in tile
	Item buriedItem;

	// Player can interact with this tile when colliding with this
	Collider2D digRangeCollider;

	// Array of 2 sprites:
	// first sprite for when no item is buried 
	// second sprite for when an item is buried
	public Sprite emptyTileSprite, fullTileSprite;
	SpriteRenderer sRend;

	// Use this for initialization
	void Start () {
		sRend = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Bury item in mound
	public override void UseItem(Item item) {
		buriedItem = item;
		sRend.sprite = fullTileSprite;
	}

	// Other tile calls this to affect it with item buried in other tile
	public override void IndirectUseItem(Item item) {}

	// Dig up item in mound
	public Item RetrieveItem(Entity c) {
		sRend.sprite = emptyTileSprite;
		Item i = buriedItem;
		buriedItem = null;
		return i;
	}
}
