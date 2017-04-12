using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class findCornerPoints : MonoBehaviour {

	public static findCornerPoints current;


	public Transform RU, RD, LU, LD;
	public LineRenderer[] edgeLines;

	void Awake() {
		current = this;
	}

	// Use this for initialization
	void Start () {
		OnRectTransformDimensionsChange ();
	}

	// This is run each time the screen is updated / orientation or window size changes
	// It marks the screen corners in world space to ensure stats are drawn to the correct position
	IEnumerator updateCorners() {
		yield return null;
		Ray ray1 = Camera.main.ScreenPointToRay (new Vector3 (10f, 10f, 0f));
		Ray ray2 = Camera.main.ScreenPointToRay (new Vector3 (10f, Camera.main.pixelHeight-Camera.main.pixelHeight*0.2f, 0f));
		Ray ray3 = Camera.main.ScreenPointToRay (new Vector3 (Camera.main.pixelWidth-10f, 10f, 0f));
		Ray ray4 = Camera.main.ScreenPointToRay (new Vector3 (Camera.main.pixelWidth-10f, Camera.main.pixelHeight-Camera.main.pixelHeight*0.2f, 0f));
		RU.position = ray4.origin + ray4.direction * 50f;
		RD.position = ray3.origin + ray3.direction * 50f;
		LU.position = ray2.origin + ray2.direction * 50f;
		LD.position = ray1.origin + ray1.direction * 50f;
		for (int i = 0; i < edgeLines.Length; i++) {
			edgeLines [i].SetPosition (0, RD.position);
			edgeLines [i].SetPosition (1, LD.position);
			edgeLines [i].SetPosition (2, LU.position);
		}
		csvLoaderScript.current.drawLines ();
	}

	// Gets called whenever screen bounds are adjusted
	void OnRectTransformDimensionsChange(){
		StartCoroutine (updateCorners ());
	}

}
