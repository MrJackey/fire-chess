using System;
using System.Threading.Tasks;

public interface IMatchmakingService {
	Task<string> CreatePrivateLobby(string userID);
	void JoinPrivateLobby(string lobbyID, Action<string> onSuccess, Action onFailure);
}
