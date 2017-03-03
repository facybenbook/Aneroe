﻿using UnityEngine;
using System.Collections;

public class SceneUnloader : MonoBehaviour
{
	public string sceneName;

	void OnTriggerEnter2D(Collider2D other) {
		if (other.GetComponent<PlayerEntity> () != null) {
			Debug.Log ("Removing Level");
			SceneController.RemoveScene (sceneName);
		}
	}
}

