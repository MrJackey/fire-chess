using System;
using System.Threading.Tasks;

public interface IMatchmakingService {
	Task<string> CreatePrivateLobby(string userID, Action<string> onJoin);
	Task DestroyPrivateLobby();
	void TryJoinPrivateLobby(string lobbyID, Action onFailure);
	Task SearchPublicLobby(string userID, Action<string> onJoin);
	Task StopSearchPublicLobby();
}
