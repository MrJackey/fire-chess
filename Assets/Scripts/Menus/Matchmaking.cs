using TMPro;
using UnityEngine;

public class Matchmaking : MonoBehaviour {
	[SerializeField] private TMP_Text lobbyIDText;
	[SerializeField] private TMP_InputField privateLobbyInputField;
	[SerializeField] private TMP_Text searchGameButtonText;

	private string lobbyID;
	private bool searching;

	private async void OnDisable() {
		if (lobbyID != default) {
			await ServiceLocator.Matchmaking.DestroyPrivateLobby(lobbyID);
			lobbyID = default;
		}

		if (searching) {
			await ServiceLocator.Matchmaking.StopSearchPublicLobby();
		}
	}

	public async void CreatePrivateLobby() {
		if (lobbyID != default) {
			await ServiceLocator.Matchmaking.DestroyPrivateLobby(lobbyID);
			lobbyID = default;
		}

		lobbyID = await ServiceLocator.Matchmaking.CreatePrivateLobby(ServiceLocator.Auth.UserID, MatchManager.OpenGame);

		lobbyIDText.text = lobbyID;
		GUIUtility.systemCopyBuffer = lobbyID;
		NotificationManager.Instance.AddNotification("Your lobby has been copied to clipboard");
	}

	public void JoinPrivateLobby() {
		string otherLobbyID = privateLobbyInputField.text;
		if (otherLobbyID.Trim() == "") return;
		if (otherLobbyID == lobbyID) {
			NotificationManager.Instance.AddNotification("You can't join your own lobbies");
			return;
		}

		ServiceLocator.Matchmaking.TryJoinPrivateLobby(otherLobbyID, HandleSucceedJoinLobby, HandleFailJoinLobby);
	}

	private async void HandleSucceedJoinLobby(string otherLobbyID, string otherPlayerID) {
		string userID = ServiceLocator.Auth.UserID;
		string matchID = await ServiceLocator.DB.CreateMatch(userID, otherPlayerID);
		await ServiceLocator.Matchmaking.AddMatchToPrivateLobby(otherLobbyID, matchID);

		MatchManager.OpenGame(matchID);
	}

	private void HandleFailJoinLobby() {
		NotificationManager.Instance.AddNotification("Something went wrong when trying to join the lobby, please try again");
	}

	public async void FindPublicGame() {
		if (searching) {
			await ServiceLocator.Matchmaking.StopSearchPublicLobby();
			searching = false;
			searchGameButtonText.text = "Search match";
			return;
		}
		searching = true;

		searchGameButtonText.text = "Searching...";
		string userID = ServiceLocator.Auth.UserID;
		ServiceLocator.Matchmaking.SearchPublicLobby(userID, async (otherLobbyID, otherPlayerID) => {
			string matchID = await ServiceLocator.DB.CreateMatch(userID, otherPlayerID);
			await ServiceLocator.Matchmaking.AddMatchToPublicLobby(otherLobbyID, matchID);
			MatchManager.OpenGame(matchID);
		}, MatchManager.OpenGame);
	}
}
