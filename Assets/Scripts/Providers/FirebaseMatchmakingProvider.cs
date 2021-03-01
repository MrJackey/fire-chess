using System;
using System.Threading.Tasks;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;

public class FirebaseMatchmakingProvider : IMatchmakingService {
	private FirebaseDatabase db;

	public FirebaseMatchmakingProvider() {
		db = FirebaseDatabase.DefaultInstance;
	}

	public async Task<string> CreatePrivateLobby(string userID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DatabaseReference lobby = db.RootReference.Child("lobbies").Child("private").Push();

		string json = JsonConvert.SerializeObject(new LobbySaveData(userID));

		await lobby.SetRawJsonValueAsync(json);

		return lobby.Key;
	}

	public async Task DestroyPrivateLobby(string lobbyID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.RootReference.Child("lobbies").Child("private").Child(lobbyID).RemoveValueAsync();
	}

	public async void TryJoinPrivateLobby(string lobbyID, Action<string> onSuccess, Action onFailure) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		DatabaseReference lobbyRef = db.RootReference.Child("lobbies").Child("private").Child(lobbyID);

		try {
			DataSnapshot transactionResult = await lobbyRef.RunTransaction(lobbyData => {
				if (!lobbyData.HasChildren) return TransactionResult.Success(lobbyData);

				if ((bool)lobbyData.Child(nameof(LobbySaveData.isJoinable)).Value == false) {
					lobbyData.Child(nameof(LobbySaveData.isJoinable)).Value = true;
					return TransactionResult.Success(lobbyData);
				}

				return TransactionResult.Abort();
			});

			if (transactionResult.Value != null) {
				// await lobbyRef.RemoveValueAsync();
				onSuccess(transactionResult.Child("lobbyOwner").Value.ToString());
				return;
			}
		}
		catch (DatabaseException e) {
			Debug.LogError(e.Message);
		}

		onFailure();
	}
}
