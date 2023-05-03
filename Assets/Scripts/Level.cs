using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using static Cinemachine.DocumentationSortingAttribute;
using UnityEditor;
using System.IO;

public class Level : MonoBehaviour {
	[SerializeField] public string[] prefabGuids;
	[SerializeField] public string positionsPath;

	public void CreateLevel() {
		AllSpritePositions allSpritePositions = JsonUtility.FromJson<AllSpritePositions>(File.ReadAllText(positionsPath));
		PrefabGuidPositions SpritePositions = new PrefabGuidPositions(prefabGuids, allSpritePositions.positions);
		// create world from level
		foreach(KeyValuePair<GameObject, List<Vector2>> prefabPositions in SpritePositions.GetPrefabs()) {
			GameObject prefab = prefabPositions.Key;
			foreach(Vector2 pos in prefabPositions.Value) {
				GameObject c = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				//GameObject c = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
				c.transform.localPosition = new Vector3(pos.x, pos.y, 0);
				c.transform.SetParent(gameObject.transform);
				c.transform.localScale = new Vector3(100, 100, 100);
			}
		}
	}
}

