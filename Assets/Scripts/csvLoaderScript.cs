using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class csvLoaderScript : MonoBehaviour {

	public static csvLoaderScript current;

	public GameObject loadingCursor;

	public LineRenderer line1;
	public LineRenderer line2;

	// A single point of data on the graph
	public struct dataSet
	{
		public string timeStamp;
		public int adRequests;
		public int available;
		public int started;
		public int views;
		public float revenue;
		public string name;
	};

	// Column numbers for each type of data
	public int f_timeStamp;
	public int f_adRequests = -1;
	public int f_available = -1;
	public int f_started = -1;
	public int f_views = -1;
	public int f_revenue;
	public int f_gameId;
	public int f_name;

	public GameObject plotPrefab;

	public InputField gameIDs;
	public InputField apiKey;

	// sort all data sets based on game ID here
	public Dictionary<int, List<dataSet>> allStats;

	public List<int> allItems;

	public Transform peakRevenueTransform;
	public Text peakRevenueText;
	public Transform bigLineNumberTransform;
	public Text bigAmountText;

	public GameObject queryPanel;

	public Toggle adreqToggle;
	public Toggle availToggle;
	public Toggle startedToggle;
	public Toggle viewsToggle;
	public Toggle revenueToggle;
	public Toggle allToggle;

	public Toggle splitbyGameToggle;
	public Toggle splitbyPlacementToggle;
	public Toggle splitbyCountryToggle;

	public Toggle t_hourToggle;
	public Toggle t_dayToggle;
	public Toggle t_weekToggle;
	public Toggle t_monthToggle;
	public Toggle t_quarterToggle;
	public Toggle t_yearToggle;
	public Toggle t_allToggle;

	public InputField startDateInput;
	public InputField endDateInput;

	private int currentGameNumber = 0;

	private List<int> theGameIDs;

	private List<Transform> points;
	private int currentPoint;

	public int howManyAPIKeys = 0;

	public bool theresStats = false;

	public Text gameName;

	public Dictionary<int,string> gameIdToName;

	public LineRenderer[] lines;
	public GameObject[] thinLines;
	public datapointToolTipScript dataToolTip;
	public Camera themaincamera;

	LineRenderer[] thinLineRenderers;
	LineRenderer[] fatLineRenderers;

	int currentLine = 0;

	int previousDataPoint = 0;

	int firstHorizontalLine = 0;

	Vector3 touchStart;
	DateTime touchStartTime;

	// Sorts based on timestamp
	// STACK ITEMS BASED ON TIMESTAMP
	// 1) check if timestamp = same, if so, combine to single data point
	// 2) if not same, add new data point
	public void addToAllStats(int gameId, dataSet newStatLine){
		dataSet newDataset = new dataSet()
		{
			timeStamp = newStatLine.timeStamp,
			adRequests = newStatLine.adRequests,
			available = newStatLine.available,
			started = newStatLine.started,
			views = newStatLine.views,
			revenue = newStatLine.revenue
		};

		if (!gameIdToName.ContainsKey(gameId)) {
			gameIdToName.Add (gameId, newStatLine.name);
		}

		if (allStats.ContainsKey(gameId))
		{
			List<dataSet> addToStatLine = allStats[gameId];
			int statLines = addToStatLine.Count;
			if (addToStatLine[statLines - 1].timeStamp == newStatLine.timeStamp)
			{
				//dataSet newDataset = new dataSet();
				newDataset.timeStamp = newStatLine.timeStamp;
				if (f_adRequests != -1)
					newDataset.adRequests = addToStatLine[statLines - 1].adRequests + newStatLine.adRequests;
				if (f_available != -1)
					newDataset.available = addToStatLine[statLines - 1].available + newStatLine.available;
				if (f_started != -1)
					newDataset.started = addToStatLine[statLines - 1].started + newStatLine.started;
				if (f_views != -1)
					newDataset.views = addToStatLine[statLines - 1].views + newStatLine.views;
				newDataset.revenue = addToStatLine[statLines - 1].revenue + newStatLine.revenue;
				allStats[gameId][statLines - 1] = newDataset;
			}
			else
			{
				allStats[gameId].Add(new dataSet()
					{
						timeStamp = newStatLine.timeStamp,
						adRequests = newStatLine.adRequests,
						available = newStatLine.available,
						started = newStatLine.started,
						views = newStatLine.views,
						revenue = newStatLine.revenue
					});

			}
		}
		else
		{
			List<dataSet> newDataSetList = new List<dataSet>();
			newDataSetList.Add(newDataset);
			allStats.Add(gameId, newDataSetList);
		}
	}

	void Start () {
		thinLineRenderers = new LineRenderer [thinLines.Length];
		fatLineRenderers = new LineRenderer [thinLines.Length];

		themaincamera = Camera.main;

		dataToolTip.gameObject.SetActive (false);

		// Populate the arrays of line renderers for other stats than revenue
		for (int i = 0; i < thinLineRenderers.Length; i++) {
			thinLineRenderers [i] = thinLines [i].GetComponent<LineRenderer> ();
			fatLineRenderers [i] = thinLines [i].GetComponent<Transform> ().GetChild (0).GetComponent<LineRenderer> ();
		}

		gameIdToName = new Dictionary<int,string> ();
		allStats = new Dictionary<int, List<dataSet>>();
		allItems = new List<int>();
		howManyAPIKeys = PlayerPrefs.GetInt ("howmanyapikeys");
		//StartCoroutine(getCSV());
	}

	public void saveApiKey()
	{
		PlayerPrefs.SetString ("apikey" + howManyAPIKeys.ToString (), apiKey.text);
		PlayerPrefs.SetString ("apikeygames" + apiKey.text, gameIDs.text);
		howManyAPIKeys++;
		PlayerPrefs.SetInt ("howmanyapikeys", howManyAPIKeys);
	}


	// goes through a single line from the csv
	// string array contains comma separated values
	public dataSet parseCsvLine(string[] csvline)
	{
		dataSet newDataset = new dataSet();
		//newDataset.adRequests = int.TryParse( csvline[f_adRequests]);
		if (f_adRequests == -1) {
			newDataset.adRequests = -1;
		} else {
			int.TryParse (csvline [f_adRequests], out newDataset.adRequests);
		}
		newDataset.name = csvline [f_name];

		if (f_available == -1) {
			newDataset.available = -1;
		} else {
			int.TryParse (csvline [f_available], out newDataset.available);
		}

		float.TryParse(csvline[f_revenue].Substring(1,csvline[f_revenue].Length-2),out newDataset.revenue);

		if (f_started == -1) {
			newDataset.started = -1;
		} else {
			int.TryParse (csvline [f_started], out newDataset.started);
		}

		newDataset.timeStamp = csvline[f_timeStamp];

		if (f_views == -1) {
			newDataset.views = -1;
		} else {
			int.TryParse (csvline [f_views], out newDataset.views);
		}

		return newDataset;
	}


	// This function marks what each column in the csv stands for
	public void findHeaderColumns(string header)
	{
		string[] headers = header.Split(","[0]);
		for (int i = 0; i < headers.Length; i++)
		{
			if (headers[i] == "Source game id")
			{
				f_gameId = i;
			}
			if (headers[i] == "adrequests")
			{
				f_adRequests = i;
			}
			if (headers[i] == "available")
			{
				f_available = i;
			}
			if (headers[i] == "started")
			{
				f_started = i;
			}
			if (headers[i] == "views")
			{
				f_views = i;
			}
			if (headers[i] == "revenue")
			{
				f_revenue = i;
			}
			if (headers [i] == "Source game name") {
				f_name = i;
			}
			//if (headers[i] == "Date"){ }
		}
	}

	public void getCSVFrom(string csvUrl) 
	{
		StartCoroutine(getCSV(csvUrl));
	}

	void Awake()
	{
		current = this;
		points = new List<Transform> ();
		for (int i = 0; i < 1000; i++) {
			GameObject newTrans = Instantiate (plotPrefab, Vector3.down * 500f, Quaternion.identity);
			points.Add (newTrans.GetComponent<Transform>());
			points [points.Count - 1].gameObject.SetActive (false);
		}
	}

	void shrinkAllLines() {
		for (int i = 0; i < firstHorizontalLine; i++) {
			lines [i].startWidth = 0.2f;
			lines [i].endWidth = 0.2f;
		}
	}


	// Checks if the swipe was left or right and changes the current game based on that
	// Swipe up/down not specified
	void SwipeNow(Vector3 delta) {
		if (Mathf.Abs (delta.y) > Mathf.Abs (delta.x)) {
			// swiped vertically
		} else {
			// swiped horizontally
			if (delta.x > 0) {
				// to the right
				currentGameNumber = currentGameNumber + 1;
				drawLines ();
			} else {
				// to the left
				currentGameNumber = currentGameNumber - 1;
				if (currentGameNumber < 0) {
					currentGameNumber = allStats.Count - 1;
				}
				drawLines ();
			}
		}
	}

	void Update( ) 
	{
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began)
		{
			touchStart = Input.GetTouch (0).position;
		}
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended)
		{
			TimeSpan timeDifference = DateTime.Now - touchStartTime;
			if (timeDifference.TotalSeconds < 0.5f) {
				Vector3 touchEnd = Input.GetTouch (0).position;
				Vector3 delta = touchEnd - touchStart;
				SwipeNow (delta);
			}
		}

		if (Input.GetMouseButtonDown (0))
		{
			touchStart = Input.mousePosition;
			touchStartTime = DateTime.Now;
		}
		if (Input.GetMouseButtonUp (0))
		{
			TimeSpan timeDifference = DateTime.Now - touchStartTime;
			if (timeDifference.TotalSeconds < 0.5f)
			{
				Vector3 touchEnd = Input.mousePosition;
				Vector3 delta = touchEnd - touchStart;
				SwipeNow (delta);
			}
		}

		// check if the mouse cursor / finger has been is on a specific data column
		bool wasNearPoint = false;
		float shortestDistanceToTap = 10000f;
		int nearestDataPoint = 0;
		for (int i = 0; i < points.Count; i++) {
			points [i].localScale = Vector3.one * 1.5f;
			float newDistance = Vector2.Distance (Input.mousePosition, themaincamera.WorldToScreenPoint (points [i].position));
			if (newDistance < shortestDistanceToTap && newDistance < 100f) {
				shortestDistanceToTap = newDistance;
				nearestDataPoint = i;
				// check where the mouse is on the screen to draw the data tooltip towards the center
				wasNearPoint = true;
			}
		}

		if (!wasNearPoint) {
			dataToolTip.gameObject.SetActive (false);
		} else {
			Vector3 offSetVector;
			shrinkAllLines ();
			lines [nearestDataPoint].startWidth = 2f;
			lines [nearestDataPoint].endWidth = 2f;
			bool mouseIsOnRight = (Input.mousePosition.x > Screen.width * 0.5f);
			bool mouseIsOnTop = (Input.mousePosition.y > Screen.height * 0.5f);

			offSetVector = new Vector3 (150f * (mouseIsOnRight ? -1 : 1), 150f * (mouseIsOnTop ? -1 : 1), 0f);

			dataToolTip.m_trans.position = Input.mousePosition + offSetVector;
			points [nearestDataPoint].localScale = Vector3.one * 5f;

			if (nearestDataPoint != previousDataPoint) {
				previousDataPoint = nearestDataPoint;
				dataToolTip.dateText.text = allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].timeStamp;
				string newDataText = "<b>Ad requests:</b> " + minusOneToNA(allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].adRequests) + '\n';
				newDataText += "<b>Available:</b> " + minusOneToNA(allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].available) + '\n';
				newDataText += "<b>Ads started:</b> " + minusOneToNA(allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].started) + '\n';
				newDataText += "<b>Views:</b> " + minusOneToNA(allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].views) + '\n';
				newDataText += "<b>Revenue:</b> " + allStats [theGameIDs [currentGameNumber]] [nearestDataPoint].revenue + " $" + '\n';
				dataToolTip.dataText.text = newDataText;
				dataToolTip.gameObject.SetActive (true);
			}
		}

		if (theresStats) {

			// test for swipe instead
			if (Input.GetMouseButtonDown (0) && false ) { 
				currentGameNumber = currentGameNumber + 1;
				drawLines ();
			}
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				currentGameNumber = currentGameNumber - 1;
				drawLines ();
			}
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				currentGameNumber = currentGameNumber + 1;
				drawLines ();
			}
		}
	}

	string minusOneToNA(int numberToCheck) {
		if (numberToCheck == -1) 
			return "n/a";
		else 
			return numberToCheck.ToString();
	}

	// Generates the URL for fetching stats from the API and downloads them
	public void getStats() {
		loadingCursor.SetActive (true);
		//theresStats = true;
		string splitBy = "";
		splitBy += (splitbyCountryToggle.isOn || splitbyGameToggle.isOn || splitbyPlacementToggle.isOn) ?
			"&splitBy=" + (splitbyCountryToggle.isOn ? "country" : "") + ((splitbyCountryToggle.isOn&&(splitbyGameToggle.isOn||splitbyPlacementToggle.isOn))?",":"" )+  (splitbyGameToggle.isOn ? "source" : "" )
			+ ((splitbyGameToggle.isOn && splitbyPlacementToggle.isOn)?",":"") + (splitbyPlacementToggle.isOn ? "zone" : "") : "";

		string fields = (adreqToggle.isOn || availToggle.isOn || startedToggle.isOn || viewsToggle.isOn || revenueToggle.isOn || allToggle.isOn) ? "&fields=" : "";
		if (fields != "") {
			int fieldLength = fields.Length;
			fields += (adreqToggle.isOn ? "adrequests" : "");
			int newFieldLenght = fields.Length;
			if (fieldLength != newFieldLenght) {
				fields += ",";
				fieldLength = fields.Length;
				newFieldLenght = fields.Length;
			}
			fields += (availToggle.isOn ? "available" : "");
			newFieldLenght = fields.Length;
			if (fieldLength != newFieldLenght) {
				fields += ",";
				fieldLength = fields.Length;
				newFieldLenght = fields.Length;
			}
			fieldLength = fields.Length;

			fields += (startedToggle.isOn ? "started" : "");
			newFieldLenght = fields.Length;
			if (fieldLength != newFieldLenght) {
				fields += ",";
				fieldLength = fields.Length;
				newFieldLenght = fields.Length;
			}
			fieldLength = fields.Length;
			fields += (viewsToggle.isOn ? "views" : "");
			newFieldLenght = fields.Length;
			if (fieldLength != newFieldLenght) {
				fields += ",";
				fieldLength = fields.Length;
				newFieldLenght = fields.Length;
			}

			fieldLength = fields.Length;
			fields += (revenueToggle.isOn ? "revenue" : "");
			fieldLength = fields.Length;
			if (allToggle.isOn) {
				fields = "all";
			}
		}

		string newUrl = "http://gameads-admin.applifier.com/stats/monetization-api?apikey=" + apiKey.text + splitBy + fields + "&start=" + startDateInput.text + "&end=" + endDateInput.text + "&scale=day&sourceIds=" + gameIDs.text;
		Debug.Log ("NEW URL: " + newUrl);
		queryPanel.SetActive (false);
		getCSVFrom (newUrl);
	}

	public void deletePublisher() {
		int toDelete = loadPubsScript.current.publist.value+1;
		Debug.Log ("TO DELETE: " + toDelete.ToString ());
		int numpubs = PlayerPrefs.GetInt ("numpubs");
		if (toDelete < numpubs) {
			for (int i = toDelete; i < numpubs; i++) {
				PlayerPrefs.SetString ("pub" + i.ToString (), PlayerPrefs.GetString ("pub" + (i + 1).ToString ()));
				Debug.Log ("Moved one");
			}
		}
		PlayerPrefs.DeleteKey ("pub" + (numpubs ).ToString ());
		PlayerPrefs.SetInt ("numpubs", numpubs - 1);
		SceneManager.LoadScene (0);
	}

	// Reads the CSV from URL, marks what each column means and stores the data, then draws the lines
	IEnumerator getCSV(string csvUrl = "")
	{
		//WWW www = new WWW(csvUrl);
		UnityWebRequest www = UnityWebRequest.Get(csvUrl);


		yield return www.Send();
		loadingCursor.SetActive (false);
		theresStats = true;
		string rawCsv = www.downloadHandler.text;
		string[] splitToLines = rawCsv.Split("\n"[0]);
		Debug.Log("Lines: " + splitToLines.Length.ToString());
		bool firstLine = true;
		foreach (string line in splitToLines)
		{
			if (firstLine)
			{
				firstLine = false;
				// Add column header data
				// Date,Source game id,adrequests,available,started,views,revenue
				findHeaderColumns(line);
			}
			else
			{
				// PARSE THE CSV on each line and add it
				string[] splitLine = line.Split(","[0]);
				addToAllStats(int.Parse(splitLine[f_gameId]), parseCsvLine(splitLine));

			}
		}
		Debug.Log("Done!");

		Debug.Log("ALL GAME IDS: " + allStats.Count.ToString());
		theGameIDs = new List<int> ();
		foreach (KeyValuePair<int, List<dataSet>> game in allStats)
		{
			theGameIDs.Add (game.Key);
			Debug.Log("GAME ID: " + game.Key.ToString() + " has " + game.Value.Count.ToString() + " lines.");
		}
		drawLines();
	}

	int findNextRoundNumberDown(float numberToCheck) 
	{
		// if 11 -> 10
		// if 205 -> 200 etc

		int rounded = Mathf.FloorToInt(numberToCheck);
		string numberAsString = rounded.ToString ();
		int numberLength = numberAsString.Length;
		int firstNumber = 0;
		int.TryParse( numberAsString.Substring(0,1), out firstNumber);
		return Mathf.FloorToInt( firstNumber * Mathf.Pow (10, numberLength - 1));
	}

	public void drawLines()
	{
		if (allStats == null || allStats.Count == 0) {
			return;
		}


		foreach (LineRenderer thisLine in lines)
			thisLine.enabled = false;

		currentLine = 0;
		if (!theresStats)
			return;
		for (int i = 0; i < points.Count; i++) {
			points [i].gameObject.SetActive (false);
		}
		int maxRevenuePoint = 0;
		currentPoint = 0;
		if (currentGameNumber >= allStats.Count || currentGameNumber < 0) {
			currentGameNumber = 0;
		}
		List<dataSet> game = allStats [theGameIDs[currentGameNumber]];

		if (!gameIdToName.ContainsKey (theGameIDs [currentGameNumber])) {
			gameIdToName.Add(theGameIDs[currentGameNumber],game[0].name);

		}
		gameName.text = gameIdToName [theGameIDs [currentGameNumber]] + " ID: " + theGameIDs[currentGameNumber].ToString();


		float maxRevenue = 0.0001f;
		float[] revenues = new float[game.Count];
		for (int i = 0; i < game.Count; i++)
		{
			float newRevenue = game[i].revenue;
			revenues[i] = newRevenue;
			if (newRevenue > maxRevenue)
			{
				maxRevenue = newRevenue;
				maxRevenuePoint = i;
			}
		}
		line1.SetVertexCount(game.Count);
		line2.SetVertexCount(game.Count);
		float xSeparation = Mathf.Abs (findCornerPoints.current.RD.position.x - findCornerPoints.current.LD.position.x) / game.Count;
		float ySize = Mathf.Abs (findCornerPoints.current.RD.position.y - findCornerPoints.current.RU.position.y);
		for (int i = 0; i < game.Count; i++)
		{
			float revenue = game[i].revenue;
			//Debug.Log ("REVENUE: " + revenue);

			Vector3 newPosition = findCornerPoints.current.LD.position + new Vector3(i * xSeparation, (revenue / maxRevenue) * ySize, 0f);
			line1.SetPosition(i, newPosition );
			line2.SetPosition(i, newPosition );
			lines [currentLine].enabled = true;
			lines[currentLine].SetPosition(0, new Vector3(newPosition.x, findCornerPoints.current.LD.position.y,newPosition.z));
			lines[currentLine].SetPosition(1, new Vector3(newPosition.x, findCornerPoints.current.LU.position.y,newPosition.z));

			currentLine++;

			Vector3 linepos = Vector3.zero;
			Transform trans = line1.GetComponent<Transform> ();
			if (trans != null) {
				linepos = trans.position;
				Debug.Log ("Linepos: " + linepos);
				Debug.Log ("newpos: " + newPosition);
				points [currentPoint].position = newPosition;
				points [currentPoint].gameObject.SetActive (true);
				currentPoint++;
			}
			if (i == maxRevenuePoint)
			{
				peakRevenueTransform.position = newPosition;
				peakRevenueText.text = "peak revenue: " + maxRevenue.ToString("0.00") + "$ at " + game[i].timeStamp.Substring(1, game[i].timeStamp.Length - 2);
			}
		}

		firstHorizontalLine = currentLine;
		int highestRoundNumber = findNextRoundNumberDown (maxRevenue);
		float bottomY = findCornerPoints.current.LD.position.y;
		float highestBarYPoint = (highestRoundNumber / maxRevenue);
		int howManyLines = 0;
		int.TryParse (highestRoundNumber.ToString ().Substring (0, 1), out howManyLines);
		//howManyLines++;
		if (howManyLines == 1) {
			howManyLines = 10;
		}
		Debug.Log ("LINES: " + howManyLines.ToString ());
		float ySeparation = Mathf.Abs (findCornerPoints.current.LD.position.y - findCornerPoints.current.LU.position.y) * highestBarYPoint;
		float topY = bottomY + ySeparation;
		for (int i = 0; i < howManyLines; i++) {
			bool thickLine = false;
			if (i == howManyLines-1 ) {
				thickLine = true;
			}

			float yValue = ((1000f * i) / howManyLines) / 1000f;
			Debug.Log ("YVALUE : " + yValue.ToString ());
			lines [currentLine].enabled = true;
			float currentY = Mathf.Lerp (bottomY, topY, yValue);
			lines[currentLine].SetPosition(0,new Vector3(findCornerPoints.current.LD.position.x,currentY,0f));
			lines[currentLine].SetPosition(1,new Vector3(findCornerPoints.current.RD.position.x,currentY,0f));

			if (thickLine) {
				lines [currentLine+1].startWidth = 0.6f;
				lines [currentLine+1].endWidth = 0.6f;
				bigLineNumberTransform.position = new Vector3 (findCornerPoints.current.RD.position.x, topY, 0f);
				bigAmountText.text = highestRoundNumber.ToString () + "$";

			} else {
				lines [currentLine].startWidth = 0.3f;
				lines [currentLine].endWidth = 0.3f;
			}
			currentLine++;

		}

		bottomY = topY;
		topY = topY + ySeparation;


		for (int i = 0; i < 10; i++) {
			float newYPos = Mathf.Lerp (bottomY, topY, i / 10f);
			if (newYPos > findCornerPoints.current.RU.position.y) {
				break;
			}
			lines [currentLine].enabled = true;
			lines[currentLine].SetPosition(0,new Vector3(findCornerPoints.current.LD.position.x,newYPos,0f));
			lines[currentLine].SetPosition(1,new Vector3(findCornerPoints.current.RD.position.x,newYPos,0f));
			currentLine++;
		}
		shrinkAllLines ();
	}

}
