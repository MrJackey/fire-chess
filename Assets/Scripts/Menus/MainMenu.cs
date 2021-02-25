using UnityEngine;

public class MainMenu : MonoBehaviour {
	public void QuitGame() {
	#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode();
	#else
		Application.Quit();
	#endif
	}
}
