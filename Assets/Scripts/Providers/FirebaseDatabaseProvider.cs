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

	public async Task<UserSaveData> GetUser(string userID) {
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

	public async Task RecordVictory(string userID) {
		await db.GetReference($"users/{userID}/winCount").RunTransaction(userWinData => {
			if (userWinData.Value == null) return TransactionResult.Success(null);

			int winCount = (int)(long)userWinData.Value;
			userWinData.Value = winCount + 1;

			return TransactionResult.Success(userWinData);
		});
		ServiceLocator.Auth.IncrementWins();
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
			// .OrderByChild(nameof(MatchSaveData.playerOneID))
			// .EqualTo(userID)
			.GetValueAsync();

		// DataSnapshot snap2 = await db.GetReference("matches")
		// 	.OrderByChild(nameof(MatchSaveData.playerTwoID))
		// 	.EqualTo(userID)
		// 	.GetValueAsync();

		Dictionary<string, MatchSaveData> matchesAsPlayerOne = snap1.HasChildren
			? JsonConvert.DeserializeObject<Dictionary<string, MatchSaveData>>(snap1.GetRawJsonValue())
			: new Dictionary<string, MatchSaveData>();

		// Dictionary<string, MatchSaveData> matchesAsPlayerTwo =
		// 	JsonConvert.DeserializeObject<Dictionary<string, MatchSaveData>>(snap2.GetRawJsonValue());

		// Client side querying instead of server-side
		return matchesAsPlayerOne
			.Where(pair => pair.Value.playerOneID == userID || pair.Value.playerTwoID == userID)
			.GroupBy(pair => pair.Key)
			.Select(group => group.First())
			.ToArray();
		// If serverside querying works
		// return matchesAsPlayerOne.Concat(matchesAsPlayerTwo).ToArray();
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

			// If the other player removes the match while subscribed
			if (args.Snapshot.Value == null) {
				return;
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

	public async void DeleteMatch(string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.GetReference($"matches/{matchID}").RemoveValueAsync();
	}
}

