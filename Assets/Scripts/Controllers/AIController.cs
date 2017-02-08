﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the global states of all the enemies
/// It does NOT delegate actions to each enemy
/// </summary>
public class AIController : BaseController {

	// Functionality
	// Deals with pausing enemies when you switch between past and present
	// Spawns enemies, determining their stats, etc... and then adds Enemy controller to delegate actions to them

	public enum Difficulty
	{
		Easy = 0,
		Normal,
		Hard,
		Nope,
		Really_Though
	};

	public static Difficulty difficulty = Difficulty.Normal;
	public static float[] difficultyModifiers = new float[5] {.75f, 1, 1.25f, 2, 3};
	//public static StatInfo[] difficultyBoosts = new StatInfo[System.Enum.GetNames(typeof(Difficulty)).Length];
	public static List<SpawnerController> spawners;

	public override void InternalSetup() {
		spawners = new List<SpawnerController> ();
	}

	public override void ExternalSetup() {
		
	}

	public static float GetDifficultyModifier() {
		return difficultyModifiers [(int)difficulty];
	}

	public static void AddSpawner(SpawnerController spawner) {
		if (!spawners.Contains (spawner)) {
			spawners.Add (spawner);
		}
	}

	public static void RemoveSpawner(SpawnerController spawner) {
		spawners.Remove (spawner);
	}
}

public class StatInfo {
	public static Dictionary<string, float> defaultLevels = new Dictionary<string, float>() {
		{"health",3},
		{"speed",1},
		{"attack",1},
		{"defense",1}
	};

	Dictionary<string, float> statLevels;

	public StatInfo() {
		statLevels = defaultLevels;
	}

	public StatInfo(Dictionary<string, float> stats) {
		statLevels = stats;
	}

	public void ModifyStatsByFactor(float factor) {
		List<string> keys = new List<string>(statLevels.Keys);
		foreach (string key in keys) {
			statLevels[key] *= factor;
		}
	}

	// Raises or lowers stat by amount
	public void ChangeStat(string stat, float amount) {
		statLevels [stat] += amount;
	}

	public void SetStat(string stat, float amount) {
		statLevels [stat] = amount;
	}

	public float GetStat(string stat) {
		return statLevels [stat];
	}
}
