using UnityEngine;
using System.Collections;
using static PlayerController;

public class Star : AbstractItem {
	[SerializeField] float invincibleTime = 1f;

	private PlayerController player;
	public override void Touch(PlayerController player) {
		Debug.Log("Touched: " + this);
		this.player = player;
		player.SetInvincible(true);
	}

	private void Update() {
		if(Time.time >= spawnedTime + invincibleTime)
			player.SetInvincible(false);
	}
}

