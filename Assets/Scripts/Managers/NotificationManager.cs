using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NotificationManager : MonoBehaviour {
	[SerializeField] private NotificationItem notificationItemPrefab;
	[SerializeField] private int maxAllowedNotifications = 1;
	[SerializeField] private float defaultNotificationDuration = 3;

	public static NotificationManager Instance { get; set; }

	private List<NotificationItem> notificationItems;
	private Queue<string> waitingNotifications;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}

		DontDestroyOnLoad(gameObject);
	}

	private void Start() {
		notificationItems = new List<NotificationItem>();
		waitingNotifications = new Queue<string>();

		for (int i = 0; i < maxAllowedNotifications; i++) {
			NotificationItem item = Instantiate(notificationItemPrefab, transform);
			notificationItems.Add(item);
			item.gameObject.SetActive(false);
		}
	}

	public void AddNotification(string message) {
		NotificationItem item = notificationItems.FirstOrDefault(x => !x.gameObject.activeSelf);
		if (item == null) {
			waitingNotifications.Enqueue(message);
			return;
		}

		item.gameObject.SetActive(true);
		StartCoroutine(ShowNotification(item, message));
	}

	private IEnumerator ShowNotification(NotificationItem item, string message) {
		item.Initialize(message);
		yield return new WaitForSeconds(defaultNotificationDuration);

		while (waitingNotifications.Count > 0) {
			item.Initialize(waitingNotifications.Dequeue());
			yield return new WaitForSeconds(defaultNotificationDuration);
		}
		item.gameObject.SetActive(false);
	}
}
