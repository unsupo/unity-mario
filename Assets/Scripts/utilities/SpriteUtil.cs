using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.U2D;

public class SpriteUtil {
	SpriteRenderer SpriteRenderer;
	string spritesheet, name;
	Sprite sprite;
	public SpriteUtil(SpriteRenderer spriteRenderer) {
		this.SpriteRenderer = spriteRenderer;
		if(spriteRenderer.sprite.name.Equals(spriteRenderer.sprite.texture.name)) {
			sprite = spriteRenderer.sprite;
			name = spriteRenderer.sprite.name;
		} else {
			Init(AssetDatabase.GetAssetPath(spriteRenderer.sprite.texture), spriteRenderer.sprite.name);
		}
	}
	public SpriteUtil(string spritesheet, string name) {
		Init(spritesheet, name);
	}
	private void Init(string spritesheet, string name) {
		this.name = name;
		this.spritesheet = spritesheet;
		sprite = AssetDatabase.LoadAllAssetsAtPath(spritesheet).OfType<Sprite>().Where(s => s.name.Equals(name)).First();
	}

	public Sprite GetSprite() {
		return sprite;
	}

	public Texture2D GetTexture2D() {
		var croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
		var pixels = sprite.texture.GetPixels((int) sprite.textureRect.x,
												(int) sprite.textureRect.y,
												(int) sprite.textureRect.width,
												(int) sprite.textureRect.height);
		croppedTexture.SetPixels(pixels);
		croppedTexture.Apply();
		return croppedTexture;
	}

	public static Texture2D RotateTexture2D(Texture2D texture, float angle) {
		Texture2D rotatedTexture = new Texture2D(texture.width, texture.height);
		Color32[] pixels = texture.GetPixels32();

		// Rotate each pixel in the texture by the specified angle
		for(int y = 0; y < texture.height; y++) {
			for(int x = 0; x < texture.width; x++) {
				float radAngle = angle * Mathf.Deg2Rad;
				float sinAngle = Mathf.Sin(radAngle);
				float cosAngle = Mathf.Cos(radAngle);

				float centerX = texture.width / 2f;
				float centerY = texture.height / 2f;

				// Translate the pixel coordinates to the center of the texture
				float translatedX = x - centerX;
				float translatedY = y - centerY;

				// Rotate the pixel coordinates around the center of the texture
				float rotatedX = translatedX * cosAngle - translatedY * sinAngle;
				float rotatedY = translatedX * sinAngle + translatedY * cosAngle;

				// Translate the pixel coordinates back to the original position
				int originalX = Mathf.RoundToInt(rotatedX + centerX);
				int originalY = Mathf.RoundToInt(rotatedY + centerY);

				// Clamp the pixel coordinates to the texture bounds
				originalX = Mathf.Clamp(originalX, 0, texture.width - 1);
				originalY = Mathf.Clamp(originalY, 0, texture.height - 1);

				// Get the pixel color from the original texture and set it in the rotated texture
				Color32 pixelColor = pixels[originalY * texture.width + originalX];
				rotatedTexture.SetPixel(x, y, pixelColor);
			}
		}

		// Apply the changes to the rotated texture
		rotatedTexture.Apply();
		return rotatedTexture;
	}
}

