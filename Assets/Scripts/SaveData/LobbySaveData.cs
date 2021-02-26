using System;

[Serializable]
public class LobbySaveData {
	public string lobbyOwner;
	public bool isFull;

	public LobbySaveData(string userID) {
		lobbyOwner = userID;
	}
}

