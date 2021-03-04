using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchBrowser : MonoBehaviour {
	[SerializeField] private GameObject browserContent;
	[SerializeField] private MatchBrowserItem browserMatchItemPrefab;
	[SerializeField] private GameObject noMatchesInfo;

	private bool isRefreshing;

	private void Start() {
		noMatchesInfo.SetActive(false);
		RefreshMatches();
	}

	public async void RefreshMatches() {
		if (isRefreshing) return;
		isRefreshing = true;

		string userID = ServiceLocator.Auth.UserID;
		RemoveAllMatches();
		KeyValuePair<string, MatchSaveData>[] matches = await ServiceLocator.DB.GetMatches(userID);

		Array.Sort(matches, (current, next) =>
			(int)(DateTime.Parse(current.Value.lastUpdated).Ticks - DateTime.Parse(next.Value.lastUpdated).Ticks));

		noMatchesInfo.SetActive(matches.Length < 1);
		foreach ((string matchID, MatchSaveData data) in matches) {
			AddMatch(matchID, data);
		}

		isRefreshing = false;
	}

	private void AddMatch(string matchID, MatchSaveData matchData) {
		MatchBrowserItem item = Instantiate(browserMatchItemPrefab, browserContent.transform);

		item.UpdateMatchData(matchID, matchData);
	}

	private void RemoveAllMatches() {
		foreach (Transform child in browserContent.transform) {
			Destroy(child.gameObject);
		}
	}
}
