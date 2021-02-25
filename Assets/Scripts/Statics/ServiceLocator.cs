public static class ServiceLocator {
	private static IAuthService auth;
	public static IAuthService Auth => auth;

	private static IDatabaseService db;
	public static IDatabaseService DB => db;

	static ServiceLocator() {
		auth = new FirebaseAuthProvider();
		db = new FirebaseDatabaseProvider();
	}
}
