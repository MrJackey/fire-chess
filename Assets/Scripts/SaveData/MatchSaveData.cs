using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class MatchSaveData {
	public string playerOneID;
	public string playerTwoID;
	public string playerOneName;
	public string playerTwoName;

	public List<CommandSaveData> commands = new List<CommandSaveData>();
	public string activePlayer;

	public string lastUpdated;

	public string OpponentName => ServiceLocator.Auth.UserID == playerOneID ? playerTwoName : playerOneName;

	public MatchSaveData(string playerOneID, string playerOneName, string playerTwoID, string playerTwoName, bool randomizeTeam) {
		if (randomizeTeam) {
			if (Random.Range(0, 1) < 0.5f) {
				this.playerOneID = playerTwoID;
				this.playerOneName = playerTwoName;
				this.playerTwoID = playerOneID;
				this.playerTwoName = playerOneName;
			}
		}
		else {
			this.playerOneID = playerOneID;
			this.playerOneName = playerOneName;
			this.playerTwoID = playerTwoID;
			this.playerTwoName = playerTwoName;
		}

		this.activePlayer = this.playerOneID;
		this.lastUpdated = DateTime.UtcNow.ToString("u");
	}
}
