using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class FindCameraForCanvas : MonoBehaviour {
	void Start() {
		GetComponent<Canvas>().worldCamera = Camera.main;
	}
}
