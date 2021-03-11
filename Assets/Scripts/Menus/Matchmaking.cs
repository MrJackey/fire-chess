using TMPro;
using UnityEngine;

public class Matchmaking : MonoBehaviour {
	[SerializeField] private TMP_Text lobbyIDText;
	[SerializeField] private TMP_InputField privateLobbyInputField;
	[SerializeField] private TMP_Text searchGameButtonText;

	private string lobbyID;
	private bool isSettingUpSearch;
	private bool isSearchingActive;
	private bool isCreatingPrivateLobby;

	private async void OnDisable() {
		if (lobbyID != default) {
			await ServiceLocator.Matchmaking.DestroyPrivateLobby();
			lobbyID = default;
		}

		if (isSettingUpSearch || isSearchingActive) {
			await ServiceLocator.Matchmaking.StopSearchPublicLobby();
		}
	}

	public async void CreatePrivateLobby() {
		if (isCreatingPrivateLobby) return;
		if (lobbyID != default) {
			isCreatingPrivateLobby = true;
			await ServiceLocator.Matchmaking.DestroyPrivateLobby();
			lobbyID = default;
		}

		isCreatingPrivateLobby = true;
		lobbyID = await ServiceLocator.Matchmaking.CreatePrivateLobby(ServiceLocator.Auth.UserID, MatchManager.OpenGame);

		lobbyIDText.text = lobbyID;
		GUIUtility.systemCopyBuffer = lobbyID;
		NotificationManager.Instance.AddNotification("Your lobby has been copied to clipboard");
		isCreatingPrivateLobby = false;
	}

	public void JoinPrivateLobby() {
		string otherLobbyID = privateLobbyInputField.text;
		if (otherLobbyID.Trim() == "") return;
		if (otherLobbyID == lobbyID) {
			NotificationManager.Instance.AddNotification("You can't join your own lobbies");
			return;
		}

		ServiceLocator.Matchmaking.TryJoinPrivateLobby(otherLobbyID, HandleFailJoinLobby);
	}

	private void HandleFailJoinLobby() {
		NotificationManager.Instance.AddNotification("Something went wrong when trying to join the lobby, please try again");
	}

	public async void FindPublicGame() {
		if (isSettingUpSearch) return;

		isSettingUpSearch = true;
		if (isSearchingActive) {
			await ServiceLocator.Matchmaking.StopSearchPublicLobby();
			isSettingUpSearch = false;
			isSearchingActive = false;

			searchGameButtonText.text = "Search match";
			return;
		}

		string userID = ServiceLocator.Auth.UserID;

		await ServiceLocator.Matchmaking.SearchPublicLobby(userID, MatchManager.OpenGame);
		isSearchingActive = true;
		isSettingUpSearch = false;

		searchGameButtonText.text = "Searching...";
	}
}
