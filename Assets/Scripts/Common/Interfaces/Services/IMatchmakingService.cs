using System;
using System.Threading.Tasks;

public interface IMatchmakingService {
	Task<string> CreatePrivateLobby(string userID);
	Task DestroyPrivateLobby(string lobbyID);
	void TryJoinPrivateLobby(string lobbyID, Action<string> onSuccess, Action onFailure);
}
