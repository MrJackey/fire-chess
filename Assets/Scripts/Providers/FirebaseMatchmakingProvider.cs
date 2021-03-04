using System;
using System.Threading.Tasks;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;

public class FirebaseMatchmakingProvider : IMatchmakingService {
	private readonly FirebaseDatabase db;

	private EventHandler<ValueChangedEventArgs> privateLobbySubscription;
	private EventHandler<ValueChangedEventArgs> publicLobbySubscription;

	private string publicLobbyID;

	public FirebaseMatchmakingProvider() {
		db = FirebaseDatabase.DefaultInstance;
	}

	public async Task<string> CreatePrivateLobby(string userID, Action<string> onJoin) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		string lobbyID = await CreateLobby(userID, "lobbies/private");

		async void HandleJoin(object obj, ValueChangedEventArgs args) {
			if (args.DatabaseError != null) {
				throw args.DatabaseError.ToException();
			}

			if (args.Snapshot.Child(nameof(LobbySaveData.matchID)).Value is string matchID) {
				onJoin(matchID);
				await DestroyPrivateLobby(args.Snapshot.Key);
			}
		}

		db.RootReference.Child("lobbies").Child("private").Child(lobbyID).ValueChanged += HandleJoin;
		privateLobbySubscription = HandleJoin;
		return lobbyID;
	}

	public async Task DestroyPrivateLobby(string lobbyID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await DestroyLobby($"lobbies/private/{lobbyID}", privateLobbySubscription);
		privateLobbySubscription = null;
	}

	public async Task AddMatchToPrivateLobby(string lobbyID, string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await AddMatchToLobby($"lobbies/private/{lobbyID}", matchID);
	}

	public async Task AddMatchToPublicLobby(string lobbyID, string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await AddMatchToLobby($"lobbies/public/{lobbyID}", matchID);
	}

	private async Task AddMatchToLobby(string path, string matchID) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		await db.RootReference.Child(path).Child(nameof(LobbySaveData.matchID)).SetValueAsync(matchID);
	}

	private async Task<string> CreateLobby(string userID, string path) {
		DatabaseReference lobbyRef = db.RootReference.Child(path).Push();
		string json = JsonConvert.SerializeObject(new LobbySaveData(userID));

		await lobbyRef.SetRawJsonValueAsync(json);

		return lobbyRef.Key;
	}

	private async Task DestroyLobby(string path, EventHandler<ValueChangedEventArgs> savedSubscription) {
		db.RootReference.Child(path).ValueChanged -= savedSubscription;

		await db.RootReference.Child(path).RemoveValueAsync();
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

	public async Task SearchPublicLobby(string userID, Action<string> onJoin) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		if (publicLobbyID != default) {
			await StopSearchPublicLobby();
		}

		DatabaseReference lobbiesRef = db.RootReference.Child("lobbies").Child("public");
		string lobbyID = null;
		string lobbyOwnerID = null;

		try {
			await lobbiesRef.RunTransaction(lobbiesData => {
				lobbyID = null;
				lobbyOwnerID = null;
				if (lobbiesData.Value == null) return TransactionResult.Success(null);

				foreach (MutableData data in lobbiesData.Children) {
					if ((bool)data.Child(nameof(LobbySaveData.isFull)).Value == false) {
						string lobbyOwner = (string)data.Child(nameof(LobbySaveData.lobbyOwnerID)).Value;

						if (lobbyOwner != userID) {
							data.Child(nameof(LobbySaveData.isFull)).Value = true;
							lobbyID = data.Key;
							lobbyOwnerID = lobbyOwner;
							return TransactionResult.Success(lobbiesData);
						}
					}
				}

				return TransactionResult.Abort();
			});

			if (lobbyID != null) {
					string matchID = await ServiceLocator.DB.CreateMatch(userID, lobbyOwnerID);
					await AddMatchToLobby($"lobbies/public/{lobbyID}", matchID);
					onJoin(matchID);
			}
			else {
				await CreatePublicLobby(userID, onJoin);
			}
		}
		catch (DatabaseException) {
			await CreatePublicLobby(userID, onJoin);
		}
	}

	private async Task CreatePublicLobby(string userID, Action<string> onLobbyJoined) {
		publicLobbyID = await CreateLobby(userID, "lobbies/public");

		async void HandleJoin(object obj, ValueChangedEventArgs args) {
			if (args.DatabaseError != null) {
				throw args.DatabaseError.ToException();
			}

			if (args.Snapshot.Child(nameof(LobbySaveData.matchID)).Value is string matchID) {
				await DestroyLobby($"lobbies/public/{args.Snapshot.Key}", publicLobbySubscription);
				onLobbyJoined(matchID);
			}
		}

		db.RootReference.Child("lobbies").Child("public").Child(publicLobbyID).ValueChanged += HandleJoin;
		publicLobbySubscription = HandleJoin;
	}

	public async Task StopSearchPublicLobby() {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		if (publicLobbyID == default) return;

		await DestroyLobby($"lobbies/public/{publicLobbyID}", publicLobbySubscription); ;
		publicLobbySubscription = null;
		publicLobbyID = default;
	}
}
