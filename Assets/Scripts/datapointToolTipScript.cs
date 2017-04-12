using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class datapointToolTipScript : MonoBehaviour {

	public Transform m_trans;
	public Text dateText;
	public Text dataText;

	void Awake() {
		m_trans = GetComponent<Transform> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	

}
