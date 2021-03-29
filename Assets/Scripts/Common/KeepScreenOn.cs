using UnityEngine;

public class KeepScreenOn : MonoBehaviour {
	void Start() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void OnDestroy() {
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}
}
