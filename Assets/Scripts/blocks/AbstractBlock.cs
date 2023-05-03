using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractBlock : MonoBehaviour {
	public List<Vector2> position = new List<Vector2>();
	protected BoxCollider2D collider;
	protected Rigidbody2D rigidbody;
	internal virtual void Awake() {
		if(TryGetComponent(out BoxCollider2D collider))
			this.collider = collider;
		else
			this.collider = gameObject.AddComponent<BoxCollider2D>();
		this.collider.sharedMaterial = new PhysicsMaterial2D("NoFriction");
		this.collider.sharedMaterial.friction = 0f;
		//this.collider.edgeRadius = 0.5f;
		//this.collider.size = new Vector2(this.collider.size.x - this.collider.edgeRadius * 2, this.collider.size.y - this.collider.edgeRadius * 2);
		if(TryGetComponent(out Rigidbody2D rigidbody))
			this.rigidbody = rigidbody;
		else
			this.rigidbody = gameObject.AddComponent<Rigidbody2D>();
		this.rigidbody.bodyType = RigidbodyType2D.Kinematic;
	}
}

