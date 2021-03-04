using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Newtonsoft.Json;

public class FirebaseDatabaseProvider : IDatabaseService {
	private readonly FirebaseDatabase db;
	private List<EventHandler<ValueChangedEventArgs>> matchSubscriptions;

	public FirebaseDatabaseProvider() {
		this.db = FirebaseDatabase.DefaultInstance;
		matchSubscriptions = new List<EventHandler<ValueChangedEventArgs>>();
	}

	private async Task<UserSaveData> GetUser(string userID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DataSnapshot snap = await db.RootReference.Child("users").Child(userID).GetValueAsync();
		return JsonConvert.DeserializeObject<UserSaveData>(snap.GetRawJsonValue());
	}

	public async Task UpdateUser(string userID, UserSaveData data) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.RootReference.Child("users").Child(userID).SetRawJsonValueAsync(JsonConvert.SerializeObject(data));
	}

	public async Task<string> CreateMatch(string creatorID, string opponentID, bool randomizeTeams = true) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		UserSaveData opponentData = await GetUser(opponentID);
		MatchSaveData newMatchData = new MatchSaveData(creatorID, ServiceLocator.Auth.DisplayName, opponentID, opponentData.displayName, randomizeTeams);

		string json = JsonUtility.ToJson(newMatchData);

		DatabaseReference matchRef = db.RootReference.Child("matches").Push();
		string key = matchRef.Key;
		await matchRef.SetRawJsonValueAsync(json);

		return key;
	}

	public async Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DataSnapshot snap1 = await db.RootReference.Child("matches")
	// #if !UNITY_EDITOR
			// .OrderByChild(nameof(MatchSaveData.playerOneID))
			// .EqualTo(userID)
	// #endif
			.GetValueAsync();

	// #if !UNITY_EDITOR
		// DataSnapshot snap2 = await db.RootReference.Child("matches")
		// 	.OrderByChild(nameof(MatchSaveData.playerTwoID))
		// 	.EqualTo(userID)
		// 	.GetValueAsync();
	// #endif

		Dictionary<string, MatchSaveData> matchesAsPlayerOne =
			JsonConvert.DeserializeObject<Dictionary<string, MatchSaveData>>(snap1.GetRawJsonValue());
	// #if !UNITY_EDITOR
		// Dictionary<string, MatchSaveData> matchesAsPlayerTwo =
		// 	JsonConvert.DeserializeObject<Dictionary<string, MatchSaveData>>(snap2.GetRawJsonValue());
	// #endif

	// #if UNITY_EDITOR
		// Client side querying instead of server-side
		return matchesAsPlayerOne
			.Where(pair => pair.Value.playerOneID == userID || pair.Value.playerTwoID == userID)
			.GroupBy(pair => pair.Key)
			.Select(group => group.First())
			.ToArray();
	// #else
		// If serverside querying works
		// return matchesAsPlayerOne.Concat(matchesAsPlayerTwo).ToArray();
	// #endif
	}

	public async void SubscribeToMatchUpdates(string matchID, Action<MatchSaveData> onUpdate) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		void HandleMatchUpdate(object sender, ValueChangedEventArgs args) {
			if (args.DatabaseError != null) {
				throw args.DatabaseError.ToException();
			}

			onUpdate(JsonConvert.DeserializeObject<MatchSaveData>(args.Snapshot.GetRawJsonValue()));
		}

		db.RootReference.Child("matches").Child(matchID).ValueChanged += HandleMatchUpdate;
		matchSubscriptions.Add(HandleMatchUpdate);
	}

	public async void UnsubscribeToMatchUpdates(string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DatabaseReference matchReference = db.RootReference.Child("matches").Child(matchID);
		foreach (EventHandler<ValueChangedEventArgs> subscription in matchSubscriptions) {
			matchReference.ValueChanged -= subscription;
		}

		matchSubscriptions.Clear();
	}

	public async void UpdateMatch(string matchID, MatchSaveData data) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.RootReference.Child("matches").Child(matchID).SetRawJsonValueAsync(JsonConvert.SerializeObject(data));
	}
}
