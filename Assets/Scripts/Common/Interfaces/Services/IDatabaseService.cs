using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDatabaseService {
	Task UpdateUser(string userID, UserSaveData data);
	Task<string> CreateMatch(string creatorID, string opponentID, bool randomizeTeams = true);
	Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID);

	void SubscribeToMatchUpdates(string matchID, Action<MatchSaveData> onUpdate);
	void UnsubscribeToMatchUpdates(string matchID);
	void UpdateMatch(string matchID, MatchSaveData data);
}
