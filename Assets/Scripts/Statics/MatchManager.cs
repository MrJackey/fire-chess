using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class MatchManager {
	private static string matchID;
	public static string MatchID {
		get => matchID;
		set => matchID = value;
	}

	private static MatchSaveData data;

	private static Team team;
	public static Team MyTeam => team;

	public static UnityEvent<MatchSaveData> OnNewData { get; } = new UnityEvent<MatchSaveData>();

	public static bool IsMyTurn => ServiceLocator.Auth.UserID == data.playerOneID
		? data.commands.Count % 2 == 0
		: data.commands.Count % 2 == 1;
	public static bool IsMatchOver => data.status == BoardStatus.Checkmate;

	public static BoardStatus Status {
		get => data.status;
		set => data.status = value;
	}

	public static bool IsItMyTurn(MatchSaveData matchData) {
		return ServiceLocator.Auth.UserID == matchData.playerOneID
			? matchData.commands.Count % 2 == 0
			: matchData.commands.Count % 2 == 1;
	}

	public static async void OpenGame(string newMatchID) {
		matchID = newMatchID;
		try {
			data = await ServiceLocator.DB.GetMatch(newMatchID);
			team = ServiceLocator.Auth.UserID == data.playerOneID ? Team.White : Team.Black;
			SceneManager.LoadScene(2);
		}
		catch (DatabaseException) {
			NotificationManager.Instance.AddNotification("Something went wrong, unable to open the match. Try refreshing");
		}
	}

	public static bool SubscribeToMatchUpdates() {
		if (matchID == default) return false;

		ServiceLocator.DB.SubscribeToMatchUpdates(matchID, HandleMatchUpdate);
		return true;
	}

	public static void UnsubscribeFromMatchUpdates() {
		if (matchID == default) return;

		ServiceLocator.DB.UnsubscribeToMatchUpdates(matchID);
	}

	private static void HandleMatchUpdate(MatchSaveData newData) {
		data = newData;

		Debug.Log("[Match] Received an update");
		OnNewData.Invoke(data);
	}

	public static void UpdateMatch(List<ICommand> commands) {
		data.commands = commands.Select(x => new CommandSaveData(x)).ToList();
		data.lastUpdated = DateTime.UtcNow.Ticks;

		ServiceLocator.DB.UpdateMatch(matchID, data);
	}
}
