using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

	public void ChangeSceneTo(int buildIndex) {
		SceneManager.LoadScene(buildIndex);
	}
}
