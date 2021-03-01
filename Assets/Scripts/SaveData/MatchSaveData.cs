using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class MatchSaveData {
	// Don't change name to playerOneID, it breaks firebase!!!
	public string playerOne;
	public string playerTwo;

	public List<CommandSaveData> commands = new List<CommandSaveData>();
	public int turnCount;
	public string activePlayer;

	public string lastUpdated;

	public MatchSaveData(string playerOneID, string playerTwoID, bool randomizeOrder) {
		if (randomizeOrder) {
			if (Random.Range(0, 1) < 0.5f) {
				this.playerOne = playerTwoID;
				this.playerTwo = playerOneID;
			}
		}
		else {
			this.playerOne = playerOneID;
			this.playerTwo = playerTwoID;
		}

		this.activePlayer = playerOne;
		this.lastUpdated = DateTime.UtcNow.ToString("u");
	}
}
