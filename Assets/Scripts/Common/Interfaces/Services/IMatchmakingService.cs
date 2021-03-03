using System;
using System.Threading.Tasks;

public interface IMatchmakingService {
	Task<string> CreatePrivateLobby(string userID, Action<string> onJoin);
	Task DestroyPrivateLobby(string lobbyID);
	void TryJoinPrivateLobby(string lobbyID, Action<string, string> onSuccess, Action onFailure);
	Task AddMatchToPrivateLobby(string lobbyID, string matchID);
	void SearchPublicLobby(string userID, Action<string, string> onOthersLobby, Action<string> onLobbyJoined);
	Task AddMatchToPublicLobby(string lobbyID, string matchID);
	Task StopSearchPublicLobby();
}
