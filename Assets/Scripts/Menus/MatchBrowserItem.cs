using TMPro;
using UnityEngine;

public class MatchBrowserItem : MonoBehaviour {
	[SerializeField] private TMP_Text opponentNameText;
	[SerializeField] private TMP_Text turnCountText;
	[SerializeField] private TMP_Text statusText;
	[SerializeField] private TMP_Text whoseTurnText;

	private string matchID;
	private MatchSaveData data;

	public void UpdateMatchData(string newMatchID, MatchSaveData newData) {
		matchID = newMatchID;
		data = newData;

		opponentNameText.text = data.OpponentName;
		turnCountText.text = data.commands.Count.ToString();
		whoseTurnText.text = MatchManager.IsItMyTurn(data) ? "Your turn" : "Waiting for opponent";
		statusText.text = data.status.ToString();
	}

	public void EnterBoard() {
		MatchManager.OpenGame(matchID);
	}

	public void DeleteBoard() {
		ServiceLocator.DB.DeleteMatch(matchID);
		Destroy(gameObject);
	}
}
