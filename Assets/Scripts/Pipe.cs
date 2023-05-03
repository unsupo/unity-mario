using UnityEngine;
using System.Collections;

public class Pipe : MonoBehaviour {
	[SerializeField] public GameObject destination;
	[SerializeField] public GameObject cameraDestination;

	private void OnDrawGizmos() {
		if(destination != null) {
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, destination.transform.position);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(destination.transform.position, cameraDestination.transform.position);
		}
	}
}

