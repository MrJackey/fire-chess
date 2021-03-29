using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Header : MonoBehaviour {
	[SerializeField] private TMP_Text displayNameText;
	[SerializeField] private TMP_Text winCountText;

	private void Start() {
		displayNameText.text = ServiceLocator.Auth.DisplayName;
		winCountText.text = ServiceLocator.Auth.WinCount.ToString();
	}

	public void SignOut() {
		ServiceLocator.Auth.SignOut();
		SceneManager.LoadScene(0);
	}
}
