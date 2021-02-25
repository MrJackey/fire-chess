using System.Threading.Tasks;

public interface IAuthService {
	Task SignUp(string displayName, string email, string password);
	Task SignIn(string email, string password);
	void SignOut();

	string GetDisplayName();
	string GetUserID();
}
