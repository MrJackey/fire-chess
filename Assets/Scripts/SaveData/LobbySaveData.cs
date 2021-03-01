using System;

[Serializable]
public class LobbySaveData {
	public string lobbyOwner;
	public bool isFull;
	public string matchID;

	public LobbySaveData(string userID) {
		lobbyOwner = userID;
	}
}

