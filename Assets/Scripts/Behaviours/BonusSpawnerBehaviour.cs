﻿using UnityEngine;
using System.Collections;

/**
 * This behaviour will possibly spawn bonus items.
 * */
public class BonusSpawnerBehaviour : MonoBehaviour {
	[SerializeField]
	private GameObject _bonusPrefab;

	private bool _hasBonus = true;

	void Start() {
		_hasBonus = Random.value > 0.5f;
	}

	public bool spawnBonusOnDestroy(Vector3 spot) {
		if (_hasBonus) {
			GameObject bonus = Instantiate(_bonusPrefab) as GameObject;
			bonus.transform.position = spot;

			CollectableBehaviour collectable = bonus.GetComponent<CollectableBehaviour>();
			if (collectable) {
				//not quite loosely coupled this is.. TODO better
				collectable.ReceivableType = CollectableBehaviour.ReceivedCollectable.Bonus;

			}
			return true;
		}
		return false;
	}

}