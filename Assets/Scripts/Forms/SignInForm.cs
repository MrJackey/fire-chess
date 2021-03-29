using System;
using Firebase;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignInForm : MonoBehaviour, IForm {
	[Header("Fields")]
	[SerializeField] private TMP_InputField emailField;
	[SerializeField] private TMP_InputField passwordField;

	private async void Start() {
		if (ServiceLocator.Auth.IsActiveLogin) {
			await ServiceLocator.Auth.FetchUserData();
			SceneManager.LoadScene(1);
		}
	}

	public async void Submit() {
		try {
			await ServiceLocator.Auth.SignIn(emailField.text, passwordField.text);
			SceneManager.LoadScene(1);
		}
		catch (AggregateException agg) when (agg.InnerException is FirebaseException e) {
			NotificationManager.Instance.AddNotification(e.Message);
		}
	}
}
