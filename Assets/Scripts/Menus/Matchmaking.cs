using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		await ServiceLocator.Matchmaking.AddMatchToLobby(otherLobbyID, matchID);

		MatchManager.OpenGame(matchID);
	}

	private void HandleFailJoinLobby() {
		Debug.Log("Something went wrong when trying to join the lobby, please try again");
	}

	public void FindPublicGame() {

	}
}
