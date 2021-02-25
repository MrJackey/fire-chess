using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchSaveData {
	// Don't change name to playerOneID, it breaks firebase!!!
	public string playerOne;
	public string playerTwo;

	public List<CommandSaveData> commands = new List<CommandSaveData>();
	public int turnCount;
	public string activePlayer;

	public MatchSaveData(string pOneID, string pTwoID) {
		this.activePlayer = pOneID;
		this.playerOne = pOneID;
		this.playerTwo = pTwoID;
	}
}
