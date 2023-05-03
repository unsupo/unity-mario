using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Rendering;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEditor.U2D.Sprites;
using static Codice.Client.BaseCommands.Import.Commit;
using Unity.Plastic.Newtonsoft.Json;
using static LevelEditor;
using UnityEngine.SocialPlatforms;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;
using UnityEngine.U2D;
using UnityEditor.U2D;

public class LevelEditor : ScriptableObject {
	static string seperator = "|";

	[MenuItem("Tools/Create Level", true)]
	private static bool ValidateGenerateSpritesAndPrefabsFromImage() {
		Texture2D texture = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)[0];
		return (texture != null);
	}

	[MenuItem("Tools/Create Level")]
	static void GenerateSpritesAndPrefabsFromImage(MenuCommand command) {
		Texture2D selected = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)[0];
		// create the spritesheet
		AllSpritePositions spritePositions = CreateUniqueSpriteSheet(selected); // issue is this operation requires user interaction to save...
		GameObject temp = new GameObject("temp");
		List<string> prefabs = new List<string>();             // create the prefabs
		foreach(AllSpritePositions.SpritePositions spritePosition in spritePositions.GetSpritePositions()) {
			Sprite sprite = spritePosition.sprite;
			Debug.Log("prefab loop: "+sprite.name+", "+sprite.texture);
			// todo find by pefab guid in case user moves or renames it
			string resourcesPath = "Prefabs/" + sprite.name + ".prefab";
			string prefabPath = "Assets/Resources/" + resourcesPath;
			GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if(existingPrefab == null) {
				// Create a new GameObject and add a SpriteRenderer component
				GameObject go = new GameObject(sprite.name);

				// Set the sprite on the SpriteRenderer component
				SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
				sr.sprite = sprite;

				// add the block component 
				AbstractBlock b = go.AddComponent<AbstractBlock>();
				go.transform.SetParent(temp.transform);
				// Create a new prefab from the GameObject
				existingPrefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
				// Destroy the temporary GameObject
				//DestroyImmediate(go);
				//EditorUtility.SetDirty(existingPrefab);
			}
			prefabs.Add(AssetDatabase.AssetPathToGUID(prefabPath));
		}
		DestroyImmediate(temp);

		// add to world - must pass in prefab guid and positions not spriteatlas guid
		GameObject level = CreateLevelPrefab(selected, spritePositions, prefabs);
		level.GetComponent<Level>().CreateLevel();

		//AssetDatabase.SaveAssets();
		//AssetDatabase.Refresh();
		//AssetDatabase.AllowAutoRefresh();
	}

	public static AllSpritePositions CreateUniqueSpriteSheet(Texture2D selected) {
		string selectedPath = "Assets/" + selected.name + ".json";
		string atlasPath = "Assets/Resources/Spritesheets/" + selected.name + "-SpriteAtlas.spriteatlas";
		if(!File.Exists(selectedPath)) {
			Dictionary<Color32[], TexturePosition> subdividedTextures = SubdivideImage(selected, 16, 16);
			// Create a new TextureImporter object
			string path = AssetDatabase.GetAssetPath(selected);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.mipmapEnabled = false;
			textureImporter.spriteImportMode = SpriteImportMode.Multiple;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

			SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
			factory.Init();
			var dataProvider = factory.GetSpriteEditorDataProviderFromObject(selected);
			dataProvider.InitSpriteEditorDataProvider();

			// set the rects
			List<SpriteRect> spriteRects = new List<SpriteRect>();
			var nameFileIdDataProvider = dataProvider.GetDataProvider<ISecondaryTextureDataProvider>();
			var spriteTextures = nameFileIdDataProvider.textures;
			// Note: This section is only for Unity 2021.2 and newer
			// Register the new Sprite Rect's name and GUID with the ISpriteNameFileIdDataProvider
			var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
			var nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();
			List<string> spritePositions = new List<string>();
			foreach(TexturePosition texturePosition in subdividedTextures.Values) {
				SpriteRect newSprite = new SpriteRect() {
					name = texturePosition.positions[0].ToString(),
					spriteID = GUID.Generate(),
					rect = new Rect(texturePosition.positions[0].x, texturePosition.positions[0].y, 16, 16)
				};
				spriteRects.Add(newSprite);
				nameFileIdPairs.Add(new SpriteNameFileIdPair(newSprite.name, newSprite.spriteID));
				spritePositions.Add(string.Join(seperator, texturePosition.positions.Select(vector => vector.x + "," + vector.y)));
			}
			// Write the updated data back to the data provider
			dataProvider.SetSpriteRects(spriteRects.ToArray());
			spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);
			EditorUtility.SetDirty(textureImporter);
			// End of Unity 2021.2 and newer section

			// Apply the changes
			dataProvider.Apply();

			var assetImporter = dataProvider.targetObject as AssetImporter;
			// Import the modified asset with the updated import settings
			//AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

			// Create a new Sprite Atlas asset
			SpriteAtlas spriteAtlas = new SpriteAtlas();
			// Set filter mode to "Point"
			// Set the packing mode for the atlas
			spriteAtlas.SetPackingSettings(new SpriteAtlasPackingSettings() {
				enableRotation = false,
			});

			// Set the texture settings for the atlas
			spriteAtlas.SetTextureSettings(new SpriteAtlasTextureSettings() {
				readable = true,
				generateMipMaps = false,
				sRGB = true,
				filterMode = FilterMode.Point,
			});
			AssetDatabase.CreateAsset(spriteAtlas, atlasPath);

			string spriteSheet = AssetDatabase.GetAssetPath(selected);
			Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet)
				.OfType<Sprite>().ToArray();

			// Add the Sprites to the Sprite Atlas
			spriteAtlas.Add(sprites);

			// Pack the Sprite Atlas
			SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);

			assetImporter.SaveAndReimport();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			AssetDatabase.AllowAutoRefresh();
			// Convert the list of objects to a JSON string
			AllSpritePositions root = new AllSpritePositions(AssetDatabase.GetAssetPath(selected), spritePositions.ToArray());
			string jsonString = JsonConvert.SerializeObject(root, Formatting.Indented);
			// Write the JSON string to a file
			File.WriteAllText(selectedPath, jsonString);
			return root;
		}
		return JsonUtility.FromJson<AllSpritePositions>(File.ReadAllText(selectedPath));
	}

	private static GameObject CreateLevelPrefab(Texture2D selected, AllSpritePositions spritePositions, List<string> prefabs) {
		string resourcesPath = "Prefabs/" + selected.name + ".prefab";
		string prefabPath = "Assets/Resources/" + resourcesPath;
		GameObject prefab;
		if(!File.Exists(prefabPath)) {
			GameObject parent = new GameObject(selected.name);
			prefab = Instantiate(parent);
			Level l = prefab.AddComponent<Level>();
			l.prefabGuids = prefabs.ToArray();
			l.positionsPath = "Assets/" + selected.name + ".json";
			PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
			DestroyImmediate(prefab);
			DestroyImmediate(parent);
		}
		//Debug.Log("saved spritepositions: "+prefab.GetComponent<Level>().SpritePositions.GetPrefabs().Count);
		return PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
	}

	public static Dictionary<Color32[], TexturePosition> SubdivideImage(Texture2D image, int subWidth, int subHeight) {
		int imageWidth = image.width;
		int imageHeight = image.height;
		int totalSize = (imageWidth / subWidth) * (imageHeight / subHeight);
		Dictionary<Color32[], TexturePosition> uniqueSprites = new Dictionary<Color32[], TexturePosition>(new ColorArrayComparer());

		Color32[] pixels = image.GetPixels32();
		for(int y = 0; y < imageHeight; y += subHeight) {
			for(int x = 0; x < imageWidth; x += subWidth) {
				Color32[] subPixels = new Color32[subWidth * subHeight];
				int pixelIndex = 0;
				for(int j = y; j < y + subHeight; j++) {
					for(int i = x; i < x + subWidth; i++) {
						subPixels[pixelIndex++] = pixels[j * imageWidth + i];
					}
				}
				if(uniqueSprites.ContainsKey(subPixels)) {
					uniqueSprites[subPixels].AddPosition(new Vector2(x, y));
				} else {
					Texture2D subImage = new Texture2D(subWidth, subHeight);
					subImage.SetPixels32(subPixels);
					subImage.Apply();
					uniqueSprites.Add(subPixels, new TexturePosition(subImage, new Vector2(x, y)));
				}
			}
		}
		return uniqueSprites;
	}

	public static void CreateLevel() {
		Texture2D selected = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets)[0];
		//string path = Application.dataPath + "/" + selected.name + "-prefablist.json";
		//SerializableList<string> prefabPaths = new SerializableList<string>();
		List<string> prefabs = new List<string>();
		Dictionary<Color32[], TexturePosition> subdividedTextures = SubdivideImage(selected, 16, 16);
		//if(!File.Exists(path)) {
		foreach(TexturePosition texturePosition in subdividedTextures.Values) {
			string spritePath = "Assets/Resources/Sprites/";
			string spriteName = string.Join(", ", texturePosition.positions[0]);
			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
			if(sprite == null) {
				Texture2D texture = texturePosition.texture;
				// save the texture as a png
				File.WriteAllBytes(spritePath + spriteName + ".png", ImageConversion.EncodeToPNG(sprite.texture));

				// Create a new TextureImporter object
				TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath + spriteName + ".png") as TextureImporter;

				textureImporter.filterMode = FilterMode.Point;
				textureImporter.textureType = TextureImporterType.Sprite;
				textureImporter.mipmapEnabled = false; // Turn off mip maps because they are *gross* 

				//// Set filter mode for maximum quality
				//texture.filterMode = FilterMode.Point;

				// Create a new sprite from the texture
				sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
				//var ti = AssetImporter.GetAtPath(spritePath) as TextureImporter;
				//ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
				//ti.mipmapEnabled = false;
				//ti.textureType = TextureImporterType.Sprite;

				//EditorUtility.SetDirty(ti);
				//ti.SaveAndReimport();
				// Save the sprite as a new asset
				EditorUtility.SetDirty(sprite);
				AssetDatabase.CreateAsset(sprite, spritePath + spriteName + ".asset");
				AssetDatabase.SaveAssets();
			}
			string resourcesPath = "Prefabs/" + sprite.name + ".prefab";
			string prefabPath = "Assets/Resources/" + resourcesPath;
			GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if(existingPrefab == null) {
				// Create a new GameObject and add a SpriteRenderer component
				GameObject go = new GameObject(sprite.name);
				SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
				AbstractBlock b = go.AddComponent<BaseBlock>();
				b.position = texturePosition.positions;

				// Set the sprite on the SpriteRenderer component
				sr.sprite = sprite;

				// Create a new prefab from the GameObject
				existingPrefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

				// Destroy the temporary GameObject
				DestroyImmediate(go);
				EditorUtility.SetDirty(existingPrefab);
			}
			prefabs.Add(prefabPath);
		}
		//	string prefabList = JsonUtility.ToJson(prefabPaths);
		//	File.WriteAllText(path, prefabList);
		//} else {
		//	Debug.Log("reading json path: " + path);
		//	string prefabList = File.ReadAllText(path);
		//	prefabPaths = JsonUtility.FromJson<SerializableList<string>>(prefabList);
		//	Debug.Log("Got: "+string.Join(", ",prefabPaths));
		//}

		//GameObject temp = new GameObject("temp");
		GameObject parent = new GameObject(selected.name + "-world");
		Image image = parent.AddComponent<Image>();
		image.sprite = Sprite.Create(selected, new Rect(0, 0, selected.width, selected.height), new Vector2(0.5f, 0.5f));
		//Debug.Log(prefabPaths.Count);
		foreach(string prefabPath in prefabs) {
			Debug.Log(prefabPath);
			GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject; //Resources.Load(prefabPath) as GameObject;
																											 //GameObject p = Instantiate(prefab);
																											 //p.transform.SetParent(parent.transform);
			if(!prefab) {
				Debug.Log(prefabPath);
				continue;
			}
			AbstractBlock block = prefab.GetComponent<AbstractBlock>();
			Debug.Log(prefab + ", " + block.position.Count);
			foreach(Vector2 pos in block.position) {
				GameObject c = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				//GameObject c = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
				c.transform.localPosition = new Vector3(pos.x, pos.y, 0);
				c.transform.SetParent(parent.transform);
				c.transform.localScale = new Vector3(100, 100, 100);
			}
		}
		//DestroyImmediate(temp);


		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		AssetDatabase.AllowAutoRefresh();
	}

	public class TexturePosition {
		public Texture2D texture;
		public GUID guid;
		public SerializableList<Vector2> positions = new SerializableList<Vector2>();
		public TexturePosition(Texture2D texture, Vector2 position) {
			this.texture = texture;
			positions.Add(position);
		}
		public void AddPosition(Vector2 position) {
			positions.Add(position);
		}
	}

	// Custom comparer for comparing arrays of colors
	public class ColorArrayComparer : IEqualityComparer<Color32[]> {
		public bool Equals(Color32[] x, Color32[] y) {
			if(x.Length != y.Length) {
				return false;
			}

			for(int i = 0; i < x.Length; i++) {
				//if(x[i] != y[i]) {
				if(!x[i].Equals(y[i])) {
					return false;
				}
			}

			return true;
		}

		public int GetHashCode(Color32[] obj) {
			int hash = 17;
			foreach(Color32 color in obj) {
				hash = hash * 23 + color.GetHashCode();
			}
			return hash;
		}
	}
}

