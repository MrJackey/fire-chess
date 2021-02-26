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

	public MatchSaveData(string playerOneID, string playerTwoID, bool randomizeOrder) {
		if (randomizeOrder) {
			if (Random.Range(0, 1) < 0.5f) {
				this.playerOne = playerTwoID;
				this.playerTwo = playerOneID;
				return;
			}
		}
		this.playerOne = playerOneID;
		this.playerTwo = playerTwoID;

		this.activePlayer = playerOneID;
	}
}
