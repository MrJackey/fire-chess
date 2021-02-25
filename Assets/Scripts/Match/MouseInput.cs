using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseInput : MonoBehaviour {
	[SerializeField] private UnityEvent<Vector3> onMouseDown;

	private Camera cam;

	private void Start() {
		cam = Camera.main;
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			onMouseDown.Invoke(cam.ScreenToWorldPoint(Input.mousePosition));
		}
	}
}
