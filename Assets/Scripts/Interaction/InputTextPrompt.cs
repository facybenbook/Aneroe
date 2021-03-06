﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AneroeInputs;

public class InputTextPrompt : TextPrompt
{
	public string[] andInputNames;
	public string[] orInputNames;
	public int[] andPromptKeys;
	public int[] orPromptKeys;

	bool[] andsPressed;
	bool andPressed, orPressed;

	protected override void Awake() {
		base.Awake ();
		ResetAndOrTrackers ();
	}

	void ResetAndOrTrackers() {
		if (orInputNames.Length == 0)
			orPressed = true;
		else
			orPressed = false;

		andsPressed = new bool[andInputNames.Length];
		for (int i = 0; i < andInputNames.Length; i++) {
			andsPressed [i] = false;
		}
		andPressed = false;
	}

	public override void CheckToContinue() {
		InputEventArgs e = PromptController.lastInputs;
		if (AllInputsRegistered (e) && AnyInputsRegistered (e)) {
			ResetAndOrTrackers ();
			ContinuePrompt ();
		}
	}

	bool AllInputsRegistered(InputEventArgs e) {
		bool allInputed = true;
		if (e == null) {
			return andPressed;
		}
		for (int i = 0; i < andInputNames.Length; i++) {
			if (andPromptKeys [i] != promptIndex)
				continue;
			if (e.WasPressed (andInputNames [i]))
				andsPressed [i] = true;
			else if (!andsPressed[i])
				allInputed = false;
		}
		if (allInputed)
			andPressed = true;
		return allInputed;
	}

	bool AnyInputsRegistered(InputEventArgs e) {
		int numOrs = 0;
		if (orPressed)
			return true;
		for (int i = 0; i < orInputNames.Length; i++) {
			if (orPromptKeys [i] != promptIndex)
				continue;
			numOrs++;
			if (e.WasPressed (orInputNames [i])) {
				orPressed = true;
				return true;
			}
		}
		// If no ors encountered, return true. 
		// Otherwise, getting this far means you didn't meet or requirements, so return false
		return numOrs == 0;
	}
}

