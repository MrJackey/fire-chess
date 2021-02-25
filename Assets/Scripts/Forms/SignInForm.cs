using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignInForm : MonoBehaviour, IForm {
	[Header("Fields")]
	[SerializeField] private TMP_InputField emailField;
	[SerializeField] private TMP_InputField passwordField;

	public async void Submit() {
		try {
			await ServiceLocator.Auth.SignIn(emailField.text, passwordField.text);
			SceneManager.LoadScene(1);
		}
		catch (Exception e) {
			Debug.LogError(e);
		}
	}
}
