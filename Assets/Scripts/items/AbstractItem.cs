using UnityEngine;
using System.Collections;
using System;

public abstract class AbstractItem : AbstractBlock, ITouched {
	public enum Movement {
		None, Normal, Bounce, Up
	}

	[SerializeField] Movement movement = Movement.None;
	[SerializeField] float speed = 20f;
	[SerializeField] float gravityScale = 20f;
	[SerializeField] bool disapearOnContact = true; // this is so we can reuse the script for bad guys and have them not dissapear unless jumped on
	bool isRight = true;
	protected float spawnedTime;

	internal virtual void Start() {
		spawnedTime = Time.time;
		if(movement != Movement.None) {
			rigidbody.bodyType = RigidbodyType2D.Dynamic;
			rigidbody.freezeRotation = true;
			rigidbody.gravityScale = gravityScale;
		}
		// todo all movements except for None and Up have a grown from block animation before moving, seems like the animaiton should play from the block
		switch(movement) {
			case Movement.Normal:
				rigidbody.velocity += Vector2.right * speed;
				break;
			case Movement.Up:
				rigidbody.AddForce(Vector2.up * speed * rigidbody.mass, ForceMode2D.Impulse);
				break;
			case Movement.Bounce:
				rigidbody.velocity += new Vector2(1f, 2f) * speed;
				// Create a new PhysicsMaterial2D with bouncy properties
				PhysicsMaterial2D bouncyMat = new PhysicsMaterial2D("Bouncy");
				bouncyMat.bounciness = 1f;  // set the bounciness to maximum
				bouncyMat.friction = 0f;    // set the friction to zero

				// Set the PhysicsMaterial2D on the Rigidbody2D component
				collider.sharedMaterial = bouncyMat;

				break;
			default:
				break;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if(disapearOnContact && collision.gameObject.TryGetComponent(out PlayerController player))
			Destroy(gameObject); // items that collide with the player dissapear
		Debug.Log("Collision: " + this + ", " + collision.gameObject+","+IsFalling() + ", " + rigidbody.velocity+", "+ CollisionUtils.GetCollisionDirection(collision));
		if(!IsFalling() && Math.Abs(CollisionUtils.GetCollisionDirection(collision).x) > 0) {
			Debug.Log("rotate");
			rigidbody.velocity = (isRight ? Vector2.right : Vector2.left) * speed;
			isRight = !isRight;
		}
	}

	private bool IsFalling() {
		return rigidbody.velocity.y < 0;
	}

	public abstract void Touch(PlayerController player);
}

