using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignUpForm : MonoBehaviour, IForm {
	[Header("Fields")]
	[SerializeField] private TMP_InputField displayNameField;
	[SerializeField] private TMP_InputField emailField;
	[SerializeField] private TMP_InputField passwordField;

	public async void Submit() {
		string email = emailField.text;
		string password = passwordField.text;

		try {
			await ServiceLocator.Auth.SignUp(displayNameField.text, email, password);
			await ServiceLocator.Auth.SignIn(email, password);
			SceneManager.LoadScene(1);
		}
		catch (Exception e) {
			Debug.LogError(e);
		}
	}
}
