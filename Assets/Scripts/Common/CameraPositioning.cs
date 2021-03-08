using UnityEngine;

public class CameraPositioning : MonoBehaviour {
	[SerializeField] private Transform playerOneTransform;
	[SerializeField] private Transform playerTwoTransform;


	public void Start() {
		if (MatchManager.MyTeam == Team.White) {
			transform.SetPositionAndRotation(playerOneTransform.position, playerOneTransform.rotation);
		}
		else {
			transform.SetPositionAndRotation(playerTwoTransform.position, playerTwoTransform.rotation);
		}
	}
}
