using System;
using System.Threading.Tasks;

public interface IMatchmakingService {
	Task<string> CreatePrivateLobby(string userID, Action<string> onJoin);
	Task DestroyPrivateLobby(string lobbyID);
	void TryJoinPrivateLobby(string lobbyID, Action<string, string> onSuccess, Action onFailure);
	Task AddMatchToPrivateLobby(string lobbyID, string matchID);
	Task SearchPublicLobby(string userID, Action<string> onJoin);
	Task StopSearchPublicLobby();
}
