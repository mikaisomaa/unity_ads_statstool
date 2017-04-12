using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatorScript : MonoBehaviour {
	float rotateSpeed = 0f;

	Transform m_trans;

	void Awake() {
		m_trans = GetComponent<Transform> ();
	}

	// Use this for initialization

	void OnEnable () {
		rotateSpeed = 50f;
	}

	// Update is called once per frame
	void Update () {
		m_trans.Rotate (-m_trans.forward * Time.deltaTime * rotateSpeed, Space.Self);
		rotateSpeed += 80f * Time.deltaTime;
	}
}
