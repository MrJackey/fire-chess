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

	// TODO: Implement actual matchmaking / or use similar for private lobbies
	public async void CreateNewMatch() {
		string userID = ServiceLocator.Auth.GetUserID();
		MatchSaveData newMatch = new MatchSaveData(userID, "o8Ew44UQPAOOjmrsRhsAkS4XutR2");
		await ServiceLocator.DB.CreateMatch(newMatch);
	}

	public async void RefreshMatches() {
		string userID = ServiceLocator.Auth.GetUserID();
		RemoveAllMatches();
		KeyValuePair<string, MatchSaveData>[] matches = await ServiceLocator.DB.GetMatches(userID);
		AddMatches(matches);
	}

	private void AddMatches(KeyValuePair<string, MatchSaveData>[] matches) {
		foreach ((string matchID, MatchSaveData data) in matches) {
			MatchBrowserItem item = Instantiate(browserMatchItemPrefab, browserContent.transform);

			item.UpdateMatchData(matchID, data);
		}
	}

	private void RemoveAllMatches() {
		foreach (Transform child in browserContent.transform) {
			Destroy(child.gameObject);
		}
	}
}
