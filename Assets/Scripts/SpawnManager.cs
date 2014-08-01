using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Spawn
{
	BallBounce,
	BallHit,
	DustSlide,
	MoveCursor,
	CloudEffect,
    Ball,
    Explosion1,
    Explosion2,
    Zombie,
    Exclamation
}

public enum PoolName
{
	Default
}

public class SpawnManager : MonoBehaviour
{
	public static Dictionary<string, GameObject> spawnDict = new Dictionary<string, GameObject> ();

	public static Transform SpawnPrefab (Spawn spawn, Vector3 pos, PoolName pool)
	{
		string poolName = GetPoolName (pool);
		string spawnName = GetSpawnName (spawn);

		Transform instance = PoolManager.Pools [poolName].Spawn (GetSpawn (spawnName).transform, pos, Quaternion.identity);
		instance.eulerAngles = Vector3.zero;
		
		return instance;
	}

	public static Transform SpawnPrefab (Spawn spawn, Vector3 pos)
	{
		string poolName = GetPoolName (PoolName.Default);
		string spawnName = GetSpawnName (spawn);
		
		Transform instance = PoolManager.Pools [poolName].Spawn (GetSpawn (spawnName).transform, pos, Quaternion.identity);
		instance.eulerAngles = Vector3.zero;
		
		return instance;
	}

	public static Transform SpawnPrefab (Spawn spawn, Vector3 pos, Vector3 rot, PoolName pool)
	{
		string poolName = GetPoolName (pool);
		string spawnName = GetSpawnName (spawn);
		
		Transform instance = PoolManager.Pools [poolName].Spawn (GetSpawn (spawnName).transform, pos, Quaternion.identity);
		instance.eulerAngles = rot;
		
		return instance;
	}

	public static void DespawnPrefab (Transform trans, PoolName pool)
	{
		string poolName = GetPoolName (pool);

		PoolManager.Pools [poolName].Despawn (trans);
	}

	#region Helper Functions

	static GameObject GetSpawn (string spawnName)
	{
		GameObject tempObject = null;

		//If Object is in the Dictionary, return value
		if (spawnDict.TryGetValue (spawnName, out tempObject)) {
			return tempObject;
		}
		//If Object isn't in Dictionary. load and add to dict
		else {
			tempObject = (GameObject)Resources.Load (spawnName);
			spawnDict.Add (spawnName, tempObject);
			return tempObject;
		}
	}

	static string GetSpawnName (Spawn spawn)
	{
		switch (spawn) {
		case(Spawn.BallBounce):
			return "BallBounce";
		case(Spawn.BallHit):
			return "BallHit";
		case(Spawn.DustSlide):
			return "DustSlide";
		case(Spawn.MoveCursor):
			return "MoveCursor";
		case(Spawn.CloudEffect):
			return "CloudEffect";
        case(Spawn.Ball):
            return "Ball";
        case(Spawn.Explosion1):
            return "Explosion1";
        case(Spawn.Explosion2):
            return "Explosion2";
        case(Spawn.Zombie):
            return "Zombie";
        case(Spawn.Exclamation):
            return "Exclamation";
		default:
			Debug.LogError ("Couldn't find Spawn Name");
			return "";
		}
	}

	static string GetPoolName (PoolName pool)
	{
		switch (pool) {
		case(PoolName.Default):
			return "Default";
			break;
		default:
			Debug.LogError ("Couldn't find Pool Name");
			return "Default";
		}
	}

	#endregion
}
