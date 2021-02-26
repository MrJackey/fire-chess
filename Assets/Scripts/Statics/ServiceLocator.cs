public static class ServiceLocator {
	private static IAuthService auth;
	public static IAuthService Auth => auth;

	private static IDatabaseService db;
	public static IDatabaseService DB => db;

	private static IMatchmakingService matchmaking;
	public static IMatchmakingService Matchmaking => matchmaking;

	static ServiceLocator() {
		auth = new FirebaseAuthProvider();
		db = new FirebaseDatabaseProvider();
		matchmaking = new FirebaseMatchmakingProvider();
	}
}
