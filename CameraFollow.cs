﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject target;

	Vector3 offset;

	void Start() {
		offset = transform.position - target.transform.position;
	}

	void LateUpdate() {
		transform.position = target.transform.position + offset;
	}
}
