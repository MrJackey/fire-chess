using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBrowser : MonoBehaviour {
	[SerializeField] private GameObject browserContent;
	[SerializeField] private MatchBrowserItem browserMatchItemPrefab;

	private void Start() {
		RefreshMatches();
	}

	public async void RefreshMatches() {
		string userID = ServiceLocator.Auth.UserID;
		RemoveAllMatches();
		KeyValuePair<string, MatchSaveData>[] matches = await ServiceLocator.DB.GetMatches(userID);

		foreach ((string matchID, MatchSaveData data) in matches) {
			AddMatch(matchID, data);
		}
	}

	public void AddMatch(string matchID, MatchSaveData matchData) {
		MatchBrowserItem item = Instantiate(browserMatchItemPrefab, browserContent.transform);

		item.UpdateMatchData(matchID, matchData);
	}

	private void RemoveAllMatches() {
		foreach (Transform child in browserContent.transform) {
			Destroy(child.gameObject);
		}
	}
}
