using UnityEngine;
using System.Collections;

public class Coin : AbstractItem {
	[SerializeField] float existingTime = 5f;

	public override void Touch(PlayerController player) {
		Debug.Log("Collided with: " + this.gameObject);
	}

	internal override void Start() {
		rigidbody.isKinematic = true;
		transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
		base.Start();
	}

	private void Update() {
		if(Time.time >= spawnedTime + existingTime)
			Destroy(gameObject);
	}
}

