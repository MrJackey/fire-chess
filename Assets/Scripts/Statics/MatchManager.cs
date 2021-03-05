using System;
using System.Collections.Generic;
using System.Linq;
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

	public static bool IsMyTurn => ServiceLocator.Auth.UserID == data.activePlayer;

	public static void OpenGame(string newMatchID) {
		matchID = newMatchID;
		SceneManager.LoadScene(3);
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
		team = ServiceLocator.Auth.UserID == data.playerOneID ? Team.White : Team.Black;

		Debug.Log("[Match] Received an update");
		OnNewData.Invoke(data);
	}

	public static void UpdateMatch(List<ICommand> commands) {
		data.commands = commands.Select(x => new CommandSaveData(x)).ToList();
		data.turnCount++;
		data.activePlayer = data.activePlayer == data.playerOneID
			? data.playerTwoID
			: data.playerOneID;
		data.lastUpdated = DateTime.UtcNow.ToString("u");

		ServiceLocator.DB.UpdateMatch(matchID, data);
	}
}
