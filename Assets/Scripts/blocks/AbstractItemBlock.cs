using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEditor;

public class AbstractItemBlock : AbstractBlock, IActivateFromBelow {
	[SerializeField] bool isHidden;
	[SerializeField] AbstractItem item;
	[SerializeField] int itemCount = 0;
	[SerializeField] private float hitThreshold = 0.01f;
	[SerializeField] AbstractBlock TransformBlock;
	Vector3 itemSpawnPoint;

	internal override void Awake() {
		base.Awake();
		if(isHidden && TryGetComponent(out SpriteRenderer renderer)) {
			Color color = renderer.material.color;
			color.a = 0.0f;
			renderer.material.color = color;

			GetComponent<BoxCollider2D>().usedByEffector = true;

			// Add PlatformEffector2D component
			PlatformEffector2D platformEffector = gameObject.AddComponent<PlatformEffector2D>();
			platformEffector.surfaceArc = 45f;
			platformEffector.rotationalOffset = 180;
			platformEffector.useOneWay = true;
			platformEffector.sideArc = 0f;
		}
		itemSpawnPoint = transform.localPosition;
	}


	public virtual bool Hit(PlayerController player, Collision2D collision) {
		if(item && (!isHidden || collision.collider.enabled && collision.relativeVelocity.y < hitThreshold)) {
			itemCount -= 1;
			Debug.Log("Hit " + (isHidden ? "hidden-" : "") + "quesitonmark, " + collision.relativeVelocity+", "+itemCount);
			// if the itemcount is 0 then transform it otherwise let more items come out of it
			if(itemCount <= 0) {
				AbstractBlock trasnformBlock;
				if(TransformBlock) {
					trasnformBlock = Instantiate(TransformBlock);
					trasnformBlock.transform.localPosition = transform.localPosition;
					trasnformBlock.transform.localScale = transform.localScale;
				}
			}
			AbstractItem itemSpawn;
			if(item) {
				itemSpawn = Instantiate(item);
				itemSpawn.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y+ collider.bounds.size.y, transform.localPosition.z);
				itemSpawn.transform.localScale = transform.localScale;
			}
			if(itemCount <= 0)
				Destroy(gameObject);
			return true;
		}
		return false;
	}

	private void OnDrawGizmos() {
		if(item && item.TryGetComponent(out SpriteRenderer spriteRenderer)) {
			//string path = AssetDatabase.GetAssetPath(sprite.sprite)+"/" + sprite.name;
			//Debug.Log("Drawing item: " + sprite.name);
			//Gizmos.DrawGUITexture(transform.position, sprite.name, true);
			float size = 16f*.75f;
			Texture sprite = new SpriteUtil(spriteRenderer).GetTexture2D() as Texture;
			//Debug.Log(sprite.name+", "+ sprite.texture);
			//Debug.Log(sprite);
			
			if(sprite == null)
				return;
			var rect = new Rect(transform.position.x - size / 2f, transform.position.y - size/2f, size, size);

			// Draw the sprite as a GUI texture
			//GUI.color = iconColor;
			Gizmos.DrawGUITexture(rect, SpriteUtil.RotateTexture2D(sprite as Texture2D, 180) as Texture);
			//GUI.DrawTexture(rect, sprite, ScaleMode.StretchToFill, true, 1f, Color.white, 0f, 0f);
			//Gizmos.DrawCube(transform.position, Vector3.one * 16f);
		}
	}
}

