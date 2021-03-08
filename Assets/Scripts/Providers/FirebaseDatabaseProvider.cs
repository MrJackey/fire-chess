using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Newtonsoft.Json;

public class FirebaseDatabaseProvider : IDatabaseService {
	private readonly FirebaseDatabase db;

	private EventHandler<ValueChangedEventArgs> matchSubscription;
	private DatabaseReference matchReference;

	public FirebaseDatabaseProvider() {
		this.db = FirebaseDatabase.DefaultInstance;
	}

	private async Task<UserSaveData> GetUser(string userID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DataSnapshot snap = await db.GetReference($"users/{userID}").GetValueAsync();
		return JsonConvert.DeserializeObject<UserSaveData>(snap.GetRawJsonValue());
	}

	public async Task UpdateUser(string userID, UserSaveData data) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.GetReference($"users/{userID}").SetRawJsonValueAsync(JsonConvert.SerializeObject(data));
	}

	public async Task<string> CreateMatch(string creatorID, string opponentID, bool randomizeTeams = true) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		UserSaveData opponentData = await GetUser(opponentID);
		MatchSaveData newMatchData = new MatchSaveData(creatorID, ServiceLocator.Auth.DisplayName, opponentID, opponentData.displayName, randomizeTeams);

		string json = JsonUtility.ToJson(newMatchData);

		DatabaseReference matchRef = db.GetReference("matches").Push();
		string key = matchRef.Key;
		await matchRef.SetRawJsonValueAsync(json);

		return key;
	}

	public async Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DataSnapshot snap1 = await db.GetReference("matches")
	// #if !UNITY_EDITOR
			// .OrderByChild(nameof(MatchSaveData.playerOneID))
			// .EqualTo(userID)
	// #endif
			.GetValueAsync();

	// #if !UNITY_EDITOR
		// DataSnapshot snap2 = await db.GetReference("matches")
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

	public async Task<MatchSaveData> GetMatch(string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DataSnapshot snap = await db.GetReference($"matches/{matchID}").GetValueAsync();

		return JsonConvert.DeserializeObject<MatchSaveData>(snap.GetRawJsonValue());
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

		matchReference = db.GetReference($"matches/{matchID}");
		matchReference.ValueChanged += HandleMatchUpdate;
		matchSubscription = HandleMatchUpdate;
	}

	public async void UnsubscribeToMatchUpdates(string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		matchReference.ValueChanged -= matchSubscription;
		matchReference = null;
		matchSubscription = null;
	}

	public async void UpdateMatch(string matchID, MatchSaveData data) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.GetReference($"matches/{matchID}").SetRawJsonValueAsync(JsonConvert.SerializeObject(data));
	}
}
