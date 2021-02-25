using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseTest : MonoBehaviour {
	void Start() {
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
			if (task.Exception != null) {
				Debug.LogError(task.Exception);
			}

			//Run this the first time, then run the "SignIn" coroutine instead.
			StartCoroutine(RegUser("test@test.test", "password"));
			// StartCoroutine(SignIn("test@test.test", "password"));
		});
	}

	private IEnumerator RegUser(string email, string password) {
		Debug.Log("Starting Registration");
		var auth = FirebaseAuth.DefaultInstance;
		var regTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
		yield return new WaitUntil(() => regTask.IsCompleted);

		if (regTask.Exception != null)
			Debug.LogWarning(regTask.Exception);
		else
			Debug.Log("Registration Complete");
	}

	private IEnumerator SignIn(string email, string password) {
		Debug.Log("Attempting to log in");
		var auth = FirebaseAuth.DefaultInstance;
		var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
		yield return new WaitUntil(() => loginTask.IsCompleted);

		if (loginTask.Exception != null)
			Debug.LogWarning(loginTask.Exception);
		else
			Debug.Log("login completed");

		StartCoroutine(DataTest(FirebaseAuth.DefaultInstance.CurrentUser.UserId, "TestWrite"));
	}

	private IEnumerator DataTest(string userID, string data) {
		Debug.Log("Trying to write data");
		var db = FirebaseDatabase.DefaultInstance;
		var dataTask = db.RootReference.Child("users").Child(userID).SetValueAsync(data);

		yield return new WaitUntil(() => dataTask.IsCompleted);

		if (dataTask.Exception != null)
			Debug.LogWarning(dataTask.Exception);
		else
			Debug.Log("DataTestWrite: Complete");
	}
}
