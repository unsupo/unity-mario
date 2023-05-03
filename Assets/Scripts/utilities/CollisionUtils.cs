using UnityEngine;

public static class CollisionUtils {
	public static Vector2 GetCollisionDirection(Collision2D collision) {
		Vector2 direction = Vector2.zero;

		// Check if there is at least one contact point
		if(collision.contactCount > 0) {
			// Get the average normal of all the contact points
			foreach(ContactPoint2D contact in collision.contacts) {
				direction += contact.normal;
			}
			direction /= collision.contactCount;
		}

		// Return the normalized direction
		return direction.normalized * -1;
	}
}
