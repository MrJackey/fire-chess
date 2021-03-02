using System;

[Serializable]
public class LobbySaveData {
	public string lobbyOwnerID;
	public bool isFull;
	public string matchID;

	public LobbySaveData(string userID) {
		this.lobbyOwnerID = userID;
	}
}

