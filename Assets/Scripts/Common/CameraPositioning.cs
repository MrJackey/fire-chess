using UnityEngine;

public class CameraPositioning : MonoBehaviour {
	[SerializeField] private Transform playerOneTransform;
	[SerializeField] private Transform playerTwoTransform;

	private void OnEnable() {
		MatchManager.OnNewData.AddListener(Reposition);
	}

	private void OnDisable() {
		MatchManager.OnNewData.RemoveListener(Reposition);
	}

	public void Reposition(MatchSaveData data) {
		if (MatchManager.MyTeam == Team.White) {
			transform.SetPositionAndRotation(playerOneTransform.position, playerOneTransform.rotation);
		}
		else {
			transform.SetPositionAndRotation(playerTwoTransform.position, playerTwoTransform.rotation);
		}
	}
}
