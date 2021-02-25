using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDatabaseService {
	Task CreateMatch(MatchSaveData newMatchData);
	Task<KeyValuePair<string, MatchSaveData>[]> GetMatches(string userID);

	void SubscribeToMatchUpdates(string matchID, Action<MatchSaveData> callback);
	void UnsubscribeToMatchUpdates(string matchID);
	void UpdateMatch(string matchID, MatchSaveData data);
}
