using System.Threading.Tasks;

public interface IAuthService {
	Task SignUp(string displayName, string email, string password);
	Task SignIn(string email, string password);
	void SignOut();
	Task FetchUserData();
	void IncrementWins();

bool IsActiveLogin { get; }
	string UserID { get; }
	string DisplayName { get; }
	int WinCount { get; }
}
