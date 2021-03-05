using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class MouseInput : MonoBehaviour {
	[SerializeField] private UnityEvent<Vector3> onMouseDown;

	private Camera cam;
	private LayerMask rayLayerMask = 1 << 6;

	private void Start() {
		cam = GetComponent<Camera>();
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, rayLayerMask)) {
				onMouseDown.Invoke(hit.point);
			}
		}
	}
}
