using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEditor;

public class LevelGenerator : MonoBehaviour {
	[SerializeField] Texture2D image;
    [SerializeField] SpriteAtlas atlas;

	public void GenerateLevel() {
		Debug.Log("TEST");
	}

	public void DigitizeImage() {

	}

	public void GeneratePrefabs() {
		GameObject prefabContainer = new GameObject("PrefabContainer");
		
        // Get all sprites in the atlas
        Sprite[] sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);

        // Loop over all sprites and do something with them
        foreach (Sprite sprite in sprites) {
			// Create a new GameObject as a child of the container GameObject
			GameObject spriteObj = new GameObject(sprite.name);
			spriteObj.transform.SetParent(prefabContainer.transform);

			// Add a Sprite Renderer component to the new GameObject
			SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();

			// Set the sprite for the Sprite Renderer component
			spriteRenderer.sprite = sprite;

			// Optionally, set other properties of the Sprite Renderer component
			spriteRenderer.sortingOrder = 0;
			spriteRenderer.color = Color.white;

			// Create a prefab from the new GameObject and save it to the "Assets" folder
			PrefabUtility.SaveAsPrefabAsset(spriteObj, "Assets/" + sprite.name + ".prefab");

			// Destroy the new GameObject
			Destroy(spriteObj);
		}

	}
}

