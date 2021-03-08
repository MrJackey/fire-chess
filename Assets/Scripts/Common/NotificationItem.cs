using TMPro;
using UnityEngine;

public class NotificationItem : MonoBehaviour {
	[SerializeField] private TMP_Text textElement;

	public void Initialize(string message) {
		textElement.text = message;
		transform.SetAsFirstSibling();
	}
}
