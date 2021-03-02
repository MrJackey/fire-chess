using TMPro;
using UnityEngine;

public class MatchBrowserItem : MonoBehaviour {
	[SerializeField] private TMP_Text opponentNameText;
	[SerializeField] private TMP_Text turnCountText;

	private string matchID;
	private MatchSaveData data;

	public void UpdateMatchData(string newMatchID, MatchSaveData newData) {
		matchID = newMatchID;
		data = newData;

		opponentNameText.text = data.OpponentName;
		turnCountText.text = data.turnCount.ToString();
	}

	public void EnterBoard() {
		MatchManager.OpenGame(matchID);
	}
}
