using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAuthProvider : IAuthService {
	private readonly FirebaseAuth auth;
	private UserSaveData userData;

	public bool IsActiveLogin => auth.CurrentUser != null;
	public string UserID => auth.CurrentUser.UserId;
	public string DisplayName => auth.CurrentUser.DisplayName;
	public int WinCount => userData.winCount;

	public FirebaseAuthProvider() {
		this.auth = FirebaseAuth.DefaultInstance;
	}

	public async Task SignUp(string displayName, string email, string password) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		FirebaseUser user = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

		UserProfile userProfile = new UserProfile {
			DisplayName = displayName,
		};
		await user.UpdateUserProfileAsync(userProfile);

		await ServiceLocator.DB.UpdateUser(UserID, new UserSaveData(userProfile.DisplayName));
	}

	public async Task SignIn(string email, string password) {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		Debug.Log("[Auth] Signing in");
		await auth.SignInWithEmailAndPasswordAsync(email, password);

		Debug.Log("[Auth] Successfully signed in");
		await FetchUserData();
	}

	public async void SignOut() {
		if (!FirebaseStatus.Initialization.IsCompleted) {
			await FirebaseStatus.Initialization;
		}

		Debug.Log("[Auth] Signing out");
		auth.SignOut();
		userData = null;
	}

	public async Task FetchUserData() {
		userData = await ServiceLocator.DB.GetUser(UserID);
	}

	public void IncrementWins() {
		userData.winCount++;
	}
}
