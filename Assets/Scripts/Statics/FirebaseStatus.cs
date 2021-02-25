using System.Threading.Tasks;
using Firebase;

public static class FirebaseStatus {
	public static Task<DependencyStatus> Initialization { get; }

	static FirebaseStatus() {
		Initialization = FirebaseApp.CheckAndFixDependenciesAsync();
	}
}
