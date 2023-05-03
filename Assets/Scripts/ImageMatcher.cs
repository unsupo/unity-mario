using System.Buffers.Text;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ImageMatcher : MonoBehaviour {
	[SerializeField] private Texture2D baseImage;
	[SerializeField] private Sprite searchImage;
	[SerializeField] private GameObject world;
	//[SerializeField] private float threshold = 1;

	public bool IsSubset() {
		if(baseImage == null || searchImage == null) {
			Debug.LogError("Base image and search image must be assigned.");
			return false;
		}

		Texture2D searchTexture = SpriteToTexture2D(searchImage);
		//Texture2D searchTexture = GetTexture2D(searchImage);

		Rect baseRect = new Rect(0, 0, baseImage.width, baseImage.height);
		Rect searchRect = new Rect(0, 0, searchTexture.width, searchTexture.height);

		List<Vector2Int> location = FindSubimageLocations(baseImage, searchTexture);
		if(location.Count > 0) {
			Debug.Log("matched: ");
			foreach(Vector2Int match in location) {
				RenderSpriteAt(searchImage, match);
				Debug.Log(match);
			}
			return true;
		}
		Debug.Log("No match found.");
		return false;
	}

	private float MatchImages(Texture2D baseTex, Texture2D searchTex, Vector2 position) {
		float matchValue = 0;
		int matchCount = 0;

		for(int x = 0; x < searchTex.width; x++) {
			for(int y = 0; y < searchTex.height; y++) {
				Color basePixel = baseTex.GetPixel(Mathf.FloorToInt(position.x + x), Mathf.FloorToInt(position.y + y));
				Color searchPixel = searchTex.GetPixel(x, y);

				if(ColorsMatch(basePixel, searchPixel)) {
					matchCount++;
				}
			}
		}

		matchValue = (float) matchCount / (searchTex.width * searchTex.height);
		return matchValue;
	}

	private bool ColorsMatch(Color c1, Color c2) {
		return Mathf.Abs(c1.r - c2.r) < 0.01f &&
			   Mathf.Abs(c1.g - c2.g) < 0.01f &&
			   Mathf.Abs(c1.b - c2.b) < 0.01f &&
			   Mathf.Abs(c1.a - c2.a) < 0.01f;
	}
	private Texture2D SpriteToTexture2D(Sprite sprite) {
		Texture2D texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height, sprite.texture.format, false);
		texture.SetPixels(sprite.texture.GetPixels((int) sprite.textureRect.x, (int) sprite.textureRect.y, (int) sprite.textureRect.width, (int) sprite.textureRect.height));
		texture.Apply();
		return texture;
	}
	public static List<Vector2Int> FindSubImage(Texture2D baseImage, Texture2D searchImage, float threshold) {
		List<Vector2Int> matchLocations = new List<Vector2Int>();

		int baseWidth = baseImage.width;
		int baseHeight = baseImage.height;

		int searchWidth = searchImage.width;
		int searchHeight = searchImage.height;

		Color[] basePixels = baseImage.GetPixels();
		Color[] searchPixels = searchImage.GetPixels();

		for(int y = 0; y <= baseHeight - searchHeight; y++) {
			for(int x = 0; x <= baseWidth - searchWidth; x++) {
				float matchPercentage = 0f;

				for(int searchY = 0; searchY < searchHeight; searchY++) {
					for(int searchX = 0; searchX < searchWidth; searchX++) {
						int baseIndex = (y + searchY) * baseWidth + x + searchX;
						int searchIndex = searchY * searchWidth + searchX;

						Color basePixel = basePixels[baseIndex];
						Color searchPixel = searchPixels[searchIndex];

						if(Mathf.Abs(basePixel.r - searchPixel.r) < threshold
							&& Mathf.Abs(basePixel.g - searchPixel.g) < threshold
							&& Mathf.Abs(basePixel.b - searchPixel.b) < threshold) {
							matchPercentage++;
						}
					}
				}

				matchPercentage /= searchWidth * searchHeight;

				if(matchPercentage >= threshold) {
					matchLocations.Add(new Vector2Int(x, y));
				}
			}
		}

		return matchLocations;
	}
	public static List<Vector2Int> FindSubimageLocations(Texture2D baseImage, Texture2D searchImage) {
		List<Vector2Int> locations = new List<Vector2Int>();

		Color32[] basePixels = baseImage.GetPixels32();
		Color32[] searchPixels = searchImage.GetPixels32();

		int baseWidth = baseImage.width;
		int baseHeight = baseImage.height;

		int searchWidth = searchImage.width;
		int searchHeight = searchImage.height;

		for(int y = 0; y < baseHeight - searchHeight + 1; y++) {
			for(int x = 0; x < baseWidth - searchWidth + 1; x++) {
				if(IsMatch(basePixels, searchPixels, baseWidth, searchWidth, searchHeight, x, y)) {
					locations.Add(new Vector2Int(x, y));
				}
			}
		}

		return locations;
	}

	private static bool IsMatch(Color32[] basePixels, Color32[] searchPixels, int baseWidth, int searchWidth, int searchHeight, int x, int y) {
		for(int j = 0; j < searchHeight; j++) {
			for(int i = 0; i < searchWidth; i++) {
				if(basePixels[(y + j) * baseWidth + x + i].Equals(searchPixels[j * searchWidth + i]) == false) {
					return false;
				}
			}
		}

		return true;
	}

	private void RenderSpriteAt(Sprite sprite, Vector2Int position) {
		// Calculate world position from pixel location
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 0f));
		worldPos.z = 0;

		// Create a new GameObject with a SpriteRenderer component
		GameObject spriteObj = new GameObject("Sprite");
		SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = sprite;

		// Set the position of the GameObject to the world position
		//spriteObj.transform.position = worldPos;
		spriteObj.transform.position = world.transform.position + worldPos; // Set relative position
		spriteObj.transform.localScale = world.transform.localScale;
	}

}
