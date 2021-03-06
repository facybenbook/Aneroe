﻿using System;
using System.Collections.Generic;
using System.Collections;
using AneroeInputs;
using PlayerEvents;
using SaveData;
using UIEvents;
using UnityEngine;

public class PlayerController : EntityController
{
	public static PlayerEntity activeCharacter;
	List<GameObject> characterPrefabs;

    private static PlayerEntity[] characters;

	// Extremely hacky (ONCE SCENES ARE ALIGNED, THIS WON'T BE NECESSARY)
	// private Vector3[] questLinePositions;
	// private Vector3 pastToPresent;

    private static int characterIndex;
    private string[] directions;

    public static event EventHandler<PlayerHealthChangedEventArgs> PlayerHealthChanged;

    public override void InternalSetup()
    {
        actionResponses = new Dictionary<string, Action<Entity>>
        {
            {"die", RestartGame},
            {"health", OnHealthChanged}
        };
		directions = new string[4] {"up", "right", "down", "left"};

		// Necessary instantiation
		// pastToPresent = characters [1].transform.position - characters [0].transform.position;
    }

    public override void ExternalSetup()
    {
		//SceneController.mergedNewScene += UpdateRelativePositions;
        InputController.iEvent.inputed += ReceiveInput;
		GameController.fileLoaded += Load;
		GameController.fileSaving += Save;
        // Subscribe to the inventory controller events to handle UI events appropriately.
        InventoryController.ItemMoved += OnItemMoved;
    }

    public override void RemoveEventListeners()
	{
		//SceneController.mergedNewScene -= UpdateRelativePositions;
        InputController.iEvent.inputed -= ReceiveInput;
		GameController.fileLoaded -= Load;
		GameController.fileSaving -= Save;
		InventoryController.ItemMoved -= OnItemMoved;
    }

    private void FixedUpdate()
    {
        if (activeCharacter != null)
            activeCharacter.DoFixedUpdate();
    }

    public void ReceiveInput(object sender, InputEventArgs e)
    {
		if (InputController.mode == InputInfo.InputMode.Free) {
			// Hotkey functionality
			if (e.WasPressed ("equip")) {
				var newEquipped = e.GetTrigger ("equip");
				if (newEquipped != "") {
					activeCharacter.TryItemEquip (int.Parse (newEquipped) - 1);
				}
			}

			// Inputs prioritized as such (by order of check):
			// Attacking, Walking, Switching character
			activeCharacter.Quicken (e.IsHeld ("quicken"));
			activeCharacter.Slowen (e.IsHeld ("slowen"));

			// See if a direction was input and log it
			var dirChosen = false;
			var dirActive = new bool[4];
			var dirTapped = new bool[4];
			for (var i = 0; i < directions.Length; i++) {
				dirTapped [i] = e.WasPressed (directions [i]);
				dirActive [i] = dirTapped [i] || e.IsHeld (directions [i]);
				dirChosen = dirChosen || dirActive [i] || dirTapped [i];
			}
			activeCharacter.SetDirections (dirActive, dirTapped);

			if (e.WasPressed ("attack")) {
				activeCharacter.TryInteracting ();
			} 
			if (e.IsHeld ("attack")) {
				activeCharacter.TryAttacking ();
			} else if (e.IsHeld ("defend")) {
				activeCharacter.TryBlocking ();
			}
			// Most likely going to filter out a seperate interact button altogether
	        /*else if (e.WasPressed("interact"))
	        {
	            activeCharacter.TryInteracting();
	        }*/
	        else if (e.WasPressed ("switch character") && activeCharacter.CanSwitchFrom ()) {
				SwitchActiveCharacters ();
			} else if (dirChosen) {
				activeCharacter.TryWalk ();
			}
		}
    }

	public static void SwitchActiveCharacters() {
		var oldC = activeCharacter;
		//int oldIndex = characterIndex;
		characterIndex = (characterIndex + 1) % characters.Length;
		activeCharacter = characters[characterIndex];

		// Check if the player is aligning the new active character to old active character
		/*
		if ((e.WasPressed ("realign") || e.IsHeld ("realign"))) {
			// Pick correct position conversion
			Vector3 nextPos = pastToPresent + oldC.transform.position;
			if (oldC.name.ToLower ().Contains ("present"))
				nextPos = -pastToPresent + oldC.transform.position;

			// If the player can transport to the other player's position
			if (!activeCharacter.WouldCollideAt (nextPos))
				activeCharacter.transform.position = nextPos;
		} else {
			// If the player can transport to their original quest line position

			if (!activeCharacter.WouldCollideAt (questLinePositions [characterIndex]))
				activeCharacter.transform.position = questLinePositions [characterIndex];
		}
		*/

		// Save the old character's position as their new quest line position
		//questLinePositions [oldIndex] = oldC.transform.position;
		SceneController.ChangeActiveCharacter(oldC, activeCharacter);
	}

	/* UNUSED FOR NOW, BUT DO NOT DELETE.
	void UpdateRelativePositions(object sender, SceneSwitchEventArgs e) {
		// Extremely hacky (ONCE SCENES ARE ALIGNED, THIS WON'T BE NECESSARY)
		Transform spaceConversionObj = GameObject.Find ("PastPresentDistance").transform;
		Transform child1 = spaceConversionObj.GetChild (0);
		if (child1.name.ToLower ().Contains ("past"))
			pastToPresent = -child1.position + spaceConversionObj.GetChild (1).position;
		else
			pastToPresent = -spaceConversionObj.GetChild (1).position + child1.position;
	}
	*/

    /// <summary>Event handler for the itemMoved event provided by ItemController.</summary>
    /// <param name="source">Originator of itemMoved event.</param>
    /// <param name="eventArgs">Useful context of the itemMoved event.</param>
    public void OnItemMoved(object source, ItemMovedEventArgs eventArgs)
    {
        activeCharacter.OnItemMoved(eventArgs);
    }

    /// <summary>Publishes the HealthChanged event with the correct values.</summary>
    /// <param name="e">The entity whose health changed.</param>
    public void OnHealthChanged(Entity e)
    {
        if (PlayerHealthChanged != null)
            PlayerHealthChanged(this, new PlayerHealthChangedEventArgs(e));
    }

    public PlayerEntity[] GetPlayers()
    {
        return characters;
    }

    private void RestartGame(Entity e)
    {
        GameObject.Find("Control").GetComponent<SceneController>().ReloadBaseScene();
    }

    public void Save(object sender, EventArgs e)
    {
        for (var i = 0; i < characters.Length; i++)
			GameController.SetSaveValue(SaveKeys.players[i], characters[i].Save());
    }

    public void Load(object sender, SceneSwitchEventArgs e)
	{
		if (e.loadStandaloneScene) {
			InitializePlayers ();
		}
		if (e.newPlaythrough) {
			for (int i = 0; i < characters.Length; i++) {
				characters [i].LoadFirstTime ();
			}
			return;
		}
        for (int i = 0; i < characters.Length; i++)
        {
			Hashtable esd;
			GameController.GetSaveValue(SaveKeys.players[i], out esd);
			if (esd == default(Hashtable))
				continue;
			//else if (e.loadControl) { // If we're booting up the game, loading controllers involves loading the player
    		characters[i].Load(esd);
			//}
        }
    }

	void InitializePlayers() {
		var obj = new GameObject();
		obj.name = "PlayerHolder";

		// Load player prefabs from scene defaults
		int numCharacters = 0;
		characterPrefabs = new List<GameObject> ();
		List<Vector3> startPositions = new List<Vector3>();
		for (int i = 0; i < SceneLoadDefaults.currentObjGroup.Length; i++) {
			if (SceneLoadDefaults.currentObjGroup [i].Equals ("Character")) {
				characterPrefabs.Add (Resources.Load<GameObject> ("Prefabs/Player/" + SceneLoadDefaults.currentObjName [i]));
				startPositions.Add (SceneLoadDefaults.currentObjStartPos [i]);
				numCharacters++;
			}
		}

		characters = new PlayerEntity[characterPrefabs.Count];
		for (var i = 0; i < characterPrefabs.Count; i++)
		{
			characters[i] = Instantiate(characterPrefabs[i], startPositions[0], Quaternion.identity, obj.transform).GetComponent<PlayerEntity>();
			characters[i].Setup();
			startPositions.RemoveAt (0);
		}
		characterIndex = 0;
		activeCharacter = characters[0];
	}
}
