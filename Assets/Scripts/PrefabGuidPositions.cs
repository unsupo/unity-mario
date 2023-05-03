
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

[Serializable]
public class PrefabGuidPositions {
	string[] prefabGuid;
	string[] positions;
	Dictionary<GameObject, List<Vector2>> prefabPositions;

	public PrefabGuidPositions(string[] prefabs, string[] positions) {
		this.prefabGuid = prefabs;
		this.positions = positions;
	}

	public Dictionary<GameObject, List<Vector2>> GetPrefabs() {
		if(prefabPositions == null) {
			prefabPositions = new Dictionary<GameObject, List<Vector2>>();
			for(int i = 0; i < prefabGuid.Length; i++) {
				string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid[i]);
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
				List<Vector2> vectorPositions = positions[i].Split("|").Select(p => {
					int[] ints = p.Split(",").Select(i=>int.Parse(i)).ToArray();
					return new Vector2(ints[0], ints[1]);
				}).ToList();
				prefabPositions.Add(prefab, vectorPositions);
			}
		}
		return prefabPositions;
	}
}