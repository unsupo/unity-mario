//using UnityEditor;
//using UnityEngine;
//using UnityEngine.U2D;
//using System.Collections.Generic;
//using UnityEditor.U2D;
//using System;
//using System.Security.Cryptography;
//using System.Text;

//public class RemoveDuplicateSprites : EditorWindow {
//	//[MenuItem("Tools/Remove Duplicate Sprites")]
//	//public static void RemoveDuplicates() {
//	//	// Get the active object in the hierarchy
//	//	SpriteAtlas spriteAtlas = Selection.activeObject as SpriteAtlas;
//	//	if(spriteAtlas == null) {
//	//		Debug.Log("Please select a sprite atlas in the hierarchy.");
//	//		return;
//	//	}

//	//	// Get all the sprites in the atlas
//	//	Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
//	//	spriteAtlas.GetSprites(sprites);
//	//	int count = 0;

//	//	// Check each sprite for duplicates
//	//	for(int i = 0; i < sprites.Length; i++) {
//	//		for(int j = i + 1; j < sprites.Length; j++) {
//	//			if(AreSpritesEqual(sprites[i], sprites[j])) {
//	//				Debug.Log("Duplicate sprite found: " + sprites[j].name);
//	//				spriteAtlas.Remove(new UnityEngine.Object[] { sprites[j] });
//	//				count++;
//	//			} else
//	//				Debug.Log("These are not duplicates: " + sprites[i]+", " + sprites[j]);
//	//		}
//	//	}

//	//	Debug.Log(count+" sprites removed from " + spriteAtlas.name + ".");
//	//}

//	//private static bool AreSpritesEqual(Sprite sprite1, Sprite sprite2) {
//	//	// Compare the pixel data of the sprites
//	//	Texture2D texture1 = sprite1.texture;
//	//	Texture2D texture2 = sprite2.texture;

//	//	if(texture1.width != texture2.width || texture1.height != texture2.height) {
//	//		Debug.Log("Not same size: " + sprite1 + ", " + sprite2);
//	//		return false;
//	//	}

//	//	Color[] firstPix = texture1.GetPixels();
//	//	Color[] secondPix = texture2.GetPixels();
//	//	if(firstPix.Length != secondPix.Length) {
//	//		Debug.Log("Heights not same size: " + sprite1 + ", " + sprite2);
//	//		return false;
//	//	}
//	//	for(int i = 0; i < firstPix.Length; i++) {
//	//		if(firstPix[i] != secondPix[i]) {
//	//			Debug.Log(i+"th row not same size: " + sprite1 + ", " + sprite2);
//	//			return false;
//	//		}
//	//	}

//	//	return true;
//	//}


//	[ContextMenu("Remove Duplicate Sprites", true)]
//	private static bool IsSpriteAtlasSelected() {
//		// Check if selected asset is a sprite atlas
//		return Selection.activeObject != null && Selection.activeObject.GetType() == typeof(UnityEngine.U2D.SpriteAtlas);
//	}


//	[ContextMenu("Remove Duplicate Sprites")]
//	static void RemoveDuplicateSpritesFromAtlas(SpriteAtlas atlas) {
//		SpriteAtlas spriteAtlas = Selection.activeObject as SpriteAtlas;
//		if(spriteAtlas == null)
//			return;
//		Dictionary<string, Color[]> uniqueSprites = new Dictionary<string, Color[]>();

//		// Loop through all sprites in the atlas
//		for(int i = 0; i < atlas.spriteCount; i++) {
//			Sprite sprite = atlas.GetSprite(i);
//			Texture2D texture = sprite.texture;

//			// Get the pixels of the sprite
//			Color[] pixels = texture.GetPixels((int) sprite.rect.x, (int) sprite.rect.y, (int) sprite.rect.width, (int) sprite.rect.height);

//			// Generate a unique identifier for the sprite based on its pixels
//			string id = GenerateSpriteID(pixels);

//			// Add the sprite to the hash table if it is unique
//			if(!uniqueSprites.ContainsKey(id)) {
//				uniqueSprites.Add(id, pixels);
//			} else {
//				atlas.Remove(new UnityEngine.Object[] { sprite });
//			}
//		}
//	}

//	static string GenerateSpriteID(Color[] pixels) {
//		// Convert the pixels to a byte array
//		byte[] bytes = new byte[pixels.Length * 4];
//		Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);

//		// Compute the MD5 hash of the byte array
//		using(var md5 = MD5.Create()) {
//			byte[] hash = md5.ComputeHash(bytes);

//			// Convert the hash to a hexadecimal string
//			StringBuilder sb = new StringBuilder();
//			for(int i = 0; i < hash.Length; i++) {
//				sb.Append(hash[i].ToString("X2"));
//			}
//			return sb.ToString();
//		}
//	}

//}

//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using UnityEngine.U2D;
//using System.Security.Policy;

//public class RemoveDuplicateSprites : EditorWindow {
//	[ContextMenu("Remove Duplicate Sprites", true)]
//	static bool ValidateSelection() {
//		return Selection.activeObject is SpriteAtlas;
//	}

//	[ContextMenu("Remove Duplicate Sprites")]
//	static void RemoveDuplicateSpritesFromMenu() {
//		SpriteAtlas spriteAtlas = Selection.activeObject as SpriteAtlas;

//		if(spriteAtlas == null) {
//			Debug.LogError("Selected object is not a SpriteAtlas.");
//			return;
//		}

//		// Show loading screen
//		EditorUtility.DisplayProgressBar("Removing Duplicate Sprites", "Loading...", 0.0f);

//		// Get all the sprites in the atlas
//		Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
//		spriteAtlas.GetSprites(sprites);

//		// Create a dictionary to keep track of unique sprites
//		Dictionary<Color[], Sprite> uniqueSprites = new Dictionary<Color[], Sprite>(new ColorArrayComparer());
//		int removed = 0;
//		// Iterate over all sprites in the atlas and add them to the dictionary
//		for(int i = 0; i < sprites.Length; i++) {
//			// Get the pixels of the current sprite
//			Texture2D texture = sprites[i].texture;
//			Color[] pixels = texture.GetPixels((int) sprites[i].textureRect.x,
//												(int) sprites[i].textureRect.y,
//												(int) sprites[i].textureRect.width,
//												(int) sprites[i].textureRect.height);

//			// Check if the pixels have been seen before
//			if(!uniqueSprites.ContainsKey(pixels)) {
//				// Add the sprite to the dictionary
//				uniqueSprites.Add(pixels, sprites[i]);
//			} else {
//				// Remove the duplicate sprite from the atlas
//				Debug.Log("Removing: " + sprites[i]);
//				//spriteAtlas.Remove(new UnityEngine.Object[] { sprites[i] });

//				// Destroy the duplicate sprite asset
//				DestroyImmediate(sprites[i], true);
//				removed++;
//			}

//			// Update the progress bar
//			float progress = (float) i / (float) sprites.Length;
//			EditorUtility.DisplayProgressBar("Removing Duplicate Sprites", "Processing Sprites...", progress);
//		}

//		// Save the changes to the Sprite Atlas
//		EditorUtility.SetDirty(spriteAtlas);
//		AssetDatabase.SaveAssets();

//		// Hide loading screen
//		EditorUtility.ClearProgressBar();

//		// Display a message when done
//		Debug.Log(removed+" Duplicate sprites removed from Sprite Atlas: " + spriteAtlas.name);
//	}
//}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RemoveDuplicateSprites : EditorWindow {
	public GUISkin guiSkin;
	private GUIStyle myStyle;

	[MenuItem("Tools/Remove Duplicate Sprites")]
	static void Init() {
		RemoveDuplicateSprites window = (RemoveDuplicateSprites) EditorWindow.GetWindow(typeof(RemoveDuplicateSprites));
		window.Show();
	}

	void OnGUI() {
		GUI.skin = guiSkin;

		if(myStyle == null) {
			myStyle = new GUIStyle(GUI.skin.label);
			myStyle.normal.textColor = Color.white;
			myStyle.fontSize = 16;
		}
		if(GUILayout.Button("Remove Duplicate Sprites")) {
			RemoveDuplicates();
		}
	}

	private Sprite[] GetSprites(Texture2D splicedTexture) {
		List<Sprite> spriteList = new List<Sprite>();

		string path = AssetDatabase.GetAssetPath(splicedTexture);
		TextureImporter textureImporter = (TextureImporter) AssetImporter.GetAtPath(path);
		textureImporter.isReadable = true;

		//SpriteMetaData[] spriteMetaData = textureImporter.spritesheet;

		//for(int i = 0; i < spriteMetaData.Length; i++) {
		//	Sprite sprite = Sprite.Create(splicedTexture, spriteMetaData[i].rect, new Vector2(0.5f, 0.5f));
		//	sprite.name = spriteMetaData[i].name;
		//	spriteList.Add(sprite);
		//}

		return spriteList.ToArray();
	}

	void RemoveDuplicates() {
		Sprite[] sprites = GetSprites(Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)[0]);

		if(sprites.Length > 0) {
			// Create a dictionary to keep track of unique sprites
			Dictionary<Color[], Sprite> uniqueSprites = new Dictionary<Color[], Sprite>(new ColorArrayComparer());
			int removed = 0;
			float progress = 0f;
			float progressStep = 1f / sprites.Length;
			// Iterate over all sprites in the atlas and add them to the dictionary
			for(int i = 0; i < sprites.Length; i++) {
				// Get the pixels of the current sprite
				Texture2D texture = sprites[i].texture;
				Color[] pixels = texture.GetPixels((int) sprites[i].textureRect.x,
													(int) sprites[i].textureRect.y,
													(int) sprites[i].textureRect.width,
													(int) sprites[i].textureRect.height);

				// Check if the pixels have been seen before
				if(!uniqueSprites.ContainsKey(pixels)) {
					// Add the sprite to the dictionary
					uniqueSprites.Add(pixels, sprites[i]);
				} else {
					// Remove the duplicate sprite from the atlas
					Debug.Log("Removing: " + sprites[i]);
					//spriteAtlas.Remove(new UnityEngine.Object[] { sprites[i] });

					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprites[i]));
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					// Destroy the duplicate sprite asset
					DestroyImmediate(sprites[i], true);
					removed++;
				}

				// Update the progress bar
				EditorUtility.DisplayProgressBar("Removing Duplicate Sprites", "Processing Sprites...", progress);
				progress += progressStep;

				string progressBarText = string.Format("{0}/{1} ({2:P1})", i + 1, sprites.Length, progress);

				bool cancel = EditorUtility.DisplayCancelableProgressBar("Removing Duplicate Sprites", progressBarText, progress);

				if(cancel) {
					EditorUtility.ClearProgressBar();
					return;
				}

			}

			//int removedCount = sprites.Length - uniqueSprites.Count;

			//if(removedCount > 0) {
			//	if(EditorUtility.DisplayDialog("Duplicate Sprites Found", removedCount + " duplicate sprite(s) found. Remove them?", "Yes", "No")) {
			//		foreach(Sprite sprite in sprites) {
			//			if(!uniqueSprites.ContainsValue(sprite)) {
			//				Debug.Log(sprite + ", " + AssetDatabase.GetAssetPath(sprite));
			//				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sprite));
			//			}
			//		}

			//		AssetDatabase.SaveAssets();
			//		AssetDatabase.Refresh();
			//	}
			//} else {
			//	EditorUtility.DisplayDialog("No Duplicate Sprites Found", "No duplicate sprites found in selection.", "OK");
			//}
		} else {
			EditorUtility.DisplayDialog("No Sprites Found", "No sprites found in selection.  found: " + Selection.activeObject, "OK");
		}
	}
}


// Custom comparer for comparing arrays of colors
public class ColorArrayComparer : IEqualityComparer<Color[]> {
	public bool Equals(Color[] x, Color[] y) {
		if(x.Length != y.Length) {
			return false;
		}

		for(int i = 0; i < x.Length; i++) {
			if(x[i] != y[i]) {
				return false;
			}
		}

		return true;
	}

	public int GetHashCode(Color[] obj) {
		int hash = 17;
		foreach(Color color in obj) {
			hash = hash * 23 + color.GetHashCode();
		}
		return hash;
	}
}
