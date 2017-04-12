using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadPubsScript : MonoBehaviour {

	public static loadPubsScript current;

	public Dropdown publist;

	public List<savePubScript.pubData> publishers;
	public InputField gameIDSInput;
	public InputField apikeyInput;


	public void init() {
		//PlayerPrefs.DeleteAll ();
		publishers = new List<savePubScript.pubData> ();
		publist.ClearOptions ();
		List<string> newOptions = new List<string>();
		if (PlayerPrefs.HasKey ("numpubs")) {
			int numpubs = PlayerPrefs.GetInt ("numpubs");
			Debug.Log ("NUMPUBS " + numpubs.ToString ());
			for (int i = 1; i < numpubs+1; i++) {
				Debug.Log ("I : " + i.ToString ());

				savePubScript.pubData newpub = new savePubScript.pubData ();
				string getNewPub = "pub" + i.ToString ();
				Debug.Log ("New publisher: " + getNewPub);
				newpub = JsonUtility.FromJson<savePubScript.pubData>(PlayerPrefs.GetString("pub"+i.ToString()));
				if (newpub != null) {
					publishers.Add (newpub);
					Debug.Log (newpub);
					newOptions.Add (newpub.name);
				}
			}
			publist.AddOptions (newOptions);
			selectionChanged (publist);
		}
	}

	public void loadPubNumber(int pubnum) {
		gameIDSInput.text = publishers [pubnum].gameIDs;
		apikeyInput.text = publishers [pubnum].APIkey;
	}

	public void selectionChanged(Dropdown publistdropdown) {
		Debug.Log ("Selection changed to: " + publist.value.ToString ());
		if (publistdropdown.options.Count > 0) {
			loadPubNumber (publistdropdown.value);
		}
	}

	void Awake( ) {
		current = this;
	}

	// Use this for initialization
	void Start () {
		//PlayerPrefs.DeleteAll ();
		init ();
	}

}
