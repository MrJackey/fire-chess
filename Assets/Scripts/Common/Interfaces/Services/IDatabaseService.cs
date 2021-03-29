using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDatabaseService {
	Task<UserSaveData> GetUser(string userID);
	Task UpdateUser(string userID, UserSaveData data);
	Task RecordVictory(string userID);
	Task<string> CreateMatch(string creatorID, string opponentID, bool randomizeTeams = true);
	Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID);
	Task<MatchSaveData> GetMatch(string matchID);

	void SubscribeToMatchUpdates(string matchID, Action<MatchSaveData> onUpdate);
	void UnsubscribeToMatchUpdates(string matchID);
	void UpdateMatch(string matchID, MatchSaveData data);
}
