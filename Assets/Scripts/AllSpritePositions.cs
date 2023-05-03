

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

[Serializable]
public class AllSpritePositions {
	//public string spriteAtlasGUID;
	public string spriteSheetPath;
	public string[] positions;
	SpriteAtlas spriteAtlas;
	List<SpritePositions> spritePositions;

	public AllSpritePositions(string spriteSheetPath, string[] spritePositions) {
		this.spriteSheetPath = spriteSheetPath;
		this.positions = spritePositions;
	}

	//public SpriteAtlas GetSpriteAtlas() {
	//	if(spriteAtlas == null) {
	//		string atlasPath = AssetDatabase.GUIDToAssetPath(spriteAtlasGUID);
	//		spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
	//	}
	//	return spriteAtlas;
	//}

	public List<SpritePositions> GetSpritePositions() {
		if(spritePositions == null) {
			spritePositions = positions.Select(p => {
				Vector2[] pos = p.Split('|').Select(s => {
					int[] xy = s.Split(",").Select(s => int.Parse(s)).ToArray();
					return new Vector2(xy[0], xy[1]);
				}).ToArray();
				return new SpritePositions(GetSprite(pos[0].ToString()), pos);
			}).ToList();
		}
		return spritePositions;
	}

	public Sprite GetSprite(string spriteName) {
		//Sprite spriteClone = GetSpriteAtlas().GetSprite(spriteName);
		//Debug.Log(AssetDatabase.GetAssetPath(spriteClone)+", "+ AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(spriteClone)));
		//string spriteSheet = "Assets/Resources/Spritesheets/NES - Super Mario Bros - World 1-1 (1).png";
		//Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteSheet);
		//Sprite[] allSprites = Resources.LoadAll<Sprite>("Assets/Resources/Spritesheets/NES - Super Mario Bros - World 1-1 (1).png");
		//Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
		//Debug.Log(texture2D.name+", "+allSprites.Length);
		return AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().Where(s => s.name.Equals(spriteName)).First();
	}

	public class SpritePositions {
		public Sprite sprite;
		public List<Vector2> positions;
		private Vector2[] pos;

		public SpritePositions(Sprite sprite, Vector2[] pos) {
			this.sprite = sprite;
			this.pos = pos;
		}
	}
}