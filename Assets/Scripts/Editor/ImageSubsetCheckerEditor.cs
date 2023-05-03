using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
public class ImageSubsetCheckerEditor : Editor {
	[SerializeField] Texture2D levelImage;
	[SerializeField] Sprite subsetImage;
	private LevelGenerator checker;

	private void OnEnable() {
		checker = (LevelGenerator) target;
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		if(GUILayout.Button("Generate Level")) {
			checker.GenerateLevel();
		}
		if(GUILayout.Button("Generate Prefabs")) {
			checker.GeneratePrefabs();
		}
	}
}
