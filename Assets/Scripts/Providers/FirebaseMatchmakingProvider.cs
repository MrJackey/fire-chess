using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;

public class FirebaseMatchmakingProvider : IMatchmakingService {
	private FirebaseDatabase db;

	private EventHandler<ValueChangedEventArgs> privateLobbySubscription;

	public FirebaseMatchmakingProvider() {
		db = FirebaseDatabase.DefaultInstance;
	}

	public async Task<string> CreatePrivateLobby(string userID, Action<string> onJoin) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DatabaseReference lobbyRef = db.RootReference.Child("lobbies").Child("private").Push();

		string json = JsonConvert.SerializeObject(new LobbySaveData(userID));

		await lobbyRef.SetRawJsonValueAsync(json);

		async void HandleJoin(object obj, ValueChangedEventArgs args) {
			if (args.DatabaseError != null) {
				throw args.DatabaseError.ToException();
			}

			if (args.Snapshot.Value is string matchID) {
				onJoin(matchID);
				await DestroyPrivateLobby(args.Snapshot.Key);
			}
		}

		lobbyRef.Child(nameof(LobbySaveData.matchID)).ValueChanged += HandleJoin;
		privateLobbySubscription = HandleJoin;

		return lobbyRef.Key;
	}

	public async Task DestroyPrivateLobby(string lobbyID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		db.RootReference.Child("lobbies").Child("private").Child(lobbyID).ValueChanged -= privateLobbySubscription;
		privateLobbySubscription = null;

		await db.RootReference.Child("lobbies").Child("private").Child(lobbyID).RemoveValueAsync();
	}

	public async void TryJoinPrivateLobby(string lobbyID, Action<string, string> onSuccess, Action onFailure) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DatabaseReference lobbyRef = db.RootReference.Child("lobbies").Child("private").Child(lobbyID);

		try {
			DataSnapshot transactionResult = await lobbyRef.RunTransaction(lobbyData => {
				if (!lobbyData.HasChildren) return TransactionResult.Success(lobbyData);

				if ((bool)lobbyData.Child(nameof(LobbySaveData.isFull)).Value == false) {
					lobbyData.Child(nameof(LobbySaveData.isFull)).Value = true;
					return TransactionResult.Success(lobbyData);
				}

				return TransactionResult.Abort();
			});

			if (transactionResult.Value != null) {
				onSuccess(lobbyID, transactionResult.Child(nameof(LobbySaveData.lobbyOwnerID)).Value.ToString());
				return;
			}
		}
		catch (DatabaseException e) {
			Debug.LogError(e.Message);
		}

		onFailure();
	}

	public async Task AddMatchToLobby(string lobbyID, string matchID) {
		await db.RootReference.Child("lobbies").Child("private").Child(lobbyID).Child(nameof(LobbySaveData.matchID)).SetValueAsync(matchID);
	}
}
