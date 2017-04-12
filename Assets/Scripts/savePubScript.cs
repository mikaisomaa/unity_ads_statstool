using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class savePubScript : MonoBehaviour {

	public InputField nameInput;
	public InputField gameIDsInput;
	public InputField apiKeyInput;

	public class pubData {
		public string name;
		public string APIkey;
		public string gameIDs;
	}
	 
	// Use this for initialization
	void Start () {
		
	}

	public void saveButtonPressed() {
		int pubNumber = 0;
		if (PlayerPrefs.HasKey ("numpubs")) {
			pubNumber = PlayerPrefs.GetInt ("numpubs") +1;
			PlayerPrefs.SetInt ("numpubs", pubNumber);
		} else {
			pubNumber++;
			PlayerPrefs.SetInt ("numpubs", pubNumber);
		}
		Debug.Log ("Saving pub #" + pubNumber.ToString ());
		pubData newpub = new pubData ();
		newpub.name = nameInput.text;
		newpub.APIkey = apiKeyInput.text;
		newpub.gameIDs = gameIDsInput.text;

		PlayerPrefs.SetString ("pub" + pubNumber.ToString (), JsonUtility.ToJson (newpub));
		PlayerPrefs.Save ();
	}

}
