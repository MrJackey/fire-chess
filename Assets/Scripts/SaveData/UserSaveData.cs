using System;
using Firebase.Auth;

[Serializable]
public class UserSaveData {
	public string displayName;

	public UserSaveData(string displayName) {
		this.displayName = displayName;
	}
}
