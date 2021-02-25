using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAuthProvider : IAuthService {
	private FirebaseAuth auth;

	public FirebaseAuthProvider() {
		this.auth = FirebaseAuth.DefaultInstance;
	}

	public async Task SignUp(string displayName, string email, string password) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		Debug.Log("[Auth] Creating account");
		FirebaseUser user = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

		UserProfile userProfile = new UserProfile {
			DisplayName = displayName,
		};
		await user.UpdateUserProfileAsync(userProfile);
		Debug.Log("[Auth] Successfully created an account");
	}

	public async Task SignIn(string email, string password) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		Debug.Log("[Auth] Signing in");
		await auth.SignInWithEmailAndPasswordAsync(email, password);

		Debug.Log("[Auth] Successfully signed in");
	}

	public async void SignOut() {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		Debug.Log("[Auth] Signing out");
		auth.SignOut();
	}

	public string GetDisplayName() {
		return auth.CurrentUser.DisplayName;
	}

	public string GetUserID() {
		return auth.CurrentUser.UserId;
	}
}
