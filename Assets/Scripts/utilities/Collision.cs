
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEditor.PlayerSettings;

[Serializable]
public class Collision {
	public enum Position {
		top, bottom, left, right
	}
	Vector2[] positions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
	public Collider2D collider;
	public Collision2D collision;
	float angle = 0f;
	Vector2 size; // = collider.bounds.size;
	float distance = 0.05f;
	Dictionary<Position, RaycastHit2D> isHitDict = new Dictionary<Position, RaycastHit2D>();
	Dictionary<Position, bool> isHitDictBool = new Dictionary<Position, bool>();
	int layer;

	public Collision(Collision2D collision) {
		int i = 0;
		this.collision = collision;
		this.collider = collision.collider;
		Vector2 collDir = CollisionUtils.GetCollisionDirection(collision);
		foreach(Vector2 pos in positions)
			isHitDictBool.Add((Position) (i++), collDir.Equals(pos));
	}

	public Collision(Collider2D collider, int groundLayer) {
		this.layer = groundLayer;
		this.collider = collider;
		this.size = collider.bounds.size;
		int i = 0;
		foreach(Vector2 pos in positions) {
			RaycastHit2D r = Physics2D.BoxCast(collider.bounds.center, size, angle, pos, distance, groundLayer);
			isHitDict.Add((Position) (i++), r);
			if(r)
				this.collider = r.collider;
		}
		//CollisionUtils.GetCollisionDirection(collider.);
	}
	public override string ToString() {
		return string.Join(", ", isHitDict.Select(kvp => kvp.Key + "=" + (kvp.Value.collider == null ? "" : kvp.Value.collider.gameObject)))+ ", " + (collision != null && collision.collider != null ? collision.collider.gameObject : "");
	}

	public GameObject GetHit(Position p) {
		return Physics2D.BoxCastAll(collider.bounds.center, size, angle, positions[(int) p], distance, layer).First().collider.gameObject;
	}

	private Collider2D Get(Position p) {
		return isHitDict.ContainsKey(p) ? isHitDict[p].collider : isHitDictBool.ContainsKey(p) ? collider : null;
	}

	public Collider2D Top => Get(Position.top);
	public Collider2D Bottom => Get(Position.bottom);
	public Collider2D Left => Get(Position.left);
	public Collider2D Right => Get(Position.right);
}