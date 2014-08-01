using UnityEngine;
using System.Collections;

public class Despawn : MonoBehaviour {

	public	bool					anim = true;
	private tk2dSpriteAnimator		_spriteAnim;
	public	float					lifeTime = 0.5f;	
	public	PoolName				poolName = PoolName.Default;

	void OnSpawned () {

		_spriteAnim = GetComponent<tk2dSpriteAnimator>();

		if (anim)
			_spriteAnim.PlayFromFrame(0);

		StartCoroutine (TimedDespawn());
	}

	IEnumerator	TimedDespawn()
	{
		yield return new WaitForSeconds(lifeTime);
		SpawnManager.DespawnPrefab(this.transform, poolName);
	}
}
