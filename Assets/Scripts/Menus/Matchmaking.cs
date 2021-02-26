using TMPro;
using UnityEngine;

public class Matchmaking : MonoBehaviour {
	[SerializeField] private MatchBrowser matchBrowser;
	[SerializeField] private TMP_Text lobbyIDText;
	[SerializeField] private TMP_InputField privateLobbyInputField;

	public async void CreatePrivateLobby() {
		string lobbyID = await ServiceLocator.Matchmaking.CreatePrivateLobby(ServiceLocator.Auth.UserID);
		lobbyIDText.text = lobbyID;
		GUIUtility.systemCopyBuffer = lobbyID;
	}

	public void JoinPrivateLobby() {
		string lobbyID = privateLobbyInputField.text;
		if (lobbyID.Trim() == "") return;

		ServiceLocator.Matchmaking.JoinPrivateLobby(lobbyID, HandleSucceedJoinLobby, HandleFailJoinLobby);
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
