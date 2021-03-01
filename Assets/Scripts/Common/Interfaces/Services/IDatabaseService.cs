using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDatabaseService {
	Task<string> CreateMatch(string playerOne, string playerTwo, bool randomizeTeams = true);
	Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID);

	void SubscribeToMatchUpdates(string matchID, Action<MatchSaveData> onUpdate);
	void UnsubscribeToMatchUpdates(string matchID);
	void UpdateMatch(string matchID, MatchSaveData data);
}
