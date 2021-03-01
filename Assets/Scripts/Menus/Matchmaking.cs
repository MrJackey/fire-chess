using System;
using TMPro;
using UnityEngine;

public class Matchmaking : MonoBehaviour {
	[SerializeField] private MatchBrowser matchBrowser;
	[SerializeField] private TMP_Text lobbyIDText;
	[SerializeField] private TMP_InputField privateLobbyInputField;

	private string lobbyID;

	private async void OnDisable() {
		if (lobbyID != default) {
			await ServiceLocator.Matchmaking.DestroyPrivateLobby(lobbyID);
			lobbyID = default;
		}
	}

	public async void CreatePrivateLobby() {
		if (lobbyID != default) {
			await ServiceLocator.Matchmaking.DestroyPrivateLobby(lobbyID);
			lobbyID = default;
		}

		lobbyID = await ServiceLocator.Matchmaking.CreatePrivateLobby(ServiceLocator.Auth.UserID);
		lobbyIDText.text = lobbyID;
		GUIUtility.systemCopyBuffer = lobbyID;
		NotificationManager.Instance.AddNotification("Your lobby has been copied to clipboard");
	}

	public void JoinPrivateLobby() {
		string otherLobbyID = privateLobbyInputField.text;
		if (otherLobbyID.Trim() == "") return;

		ServiceLocator.Matchmaking.TryJoinPrivateLobby(otherLobbyID, HandleSucceedJoinLobby, HandleFailJoinLobby);
	}

	private async void HandleSucceedJoinLobby(string otherPlayerID) {
		Debug.Log($"Creating game with player: {otherPlayerID}");
		string userID = ServiceLocator.Auth.UserID;
		await ServiceLocator.DB.CreateMatch(userID, otherPlayerID);
		matchBrowser.RefreshMatches();
	}

	private void HandleFailJoinLobby() {
		Debug.Log("Something went wrong when trying to join the lobby, please try again");
	}

	public void FindPublicGame() {

	}
}
