using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Header : MonoBehaviour {
	[SerializeField] private TMP_Text displayNameText;

	private void Start() {
		displayNameText.text = ServiceLocator.Auth.GetDisplayName();
	}

	public void SignOut() {
		ServiceLocator.Auth.SignOut();
		SceneManager.LoadScene(0);
	}
}
