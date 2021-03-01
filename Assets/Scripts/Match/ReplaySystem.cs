using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReplaySystem : MonoBehaviour {
	[SerializeField] private ChessBoard board;

	[Header("UI")]
	[SerializeField] private TMP_Text text;
	[SerializeField] private Slider slider;
	[SerializeField] private GameObject newMovesNotification;

	[Space]
	[SerializeField] private float playCommandDelay;

	private List<ICommand> commands;
	public List<ICommand> Commands => commands;

	private List<ICommand> newCommands;

	private int currentCommandIndex;
	public bool IsLive => currentCommandIndex == commands.Count - 1;

	private void Start() {
		commands = new List<ICommand>();
		newCommands = new List<ICommand>();
		currentCommandIndex = -1;
		newMovesNotification.SetActive(false);
	}

	private void OnEnable() {
		if (!MatchManager.SubscribeToMatchUpdates()) {
			Debug.LogWarning("The MatchManager does not have a matchID. Redirecting to lobby");
			SceneManager.LoadScene(1);
		}

		MatchManager.OnNewData.AddListener(HandleNewData);
		slider.onValueChanged.AddListener(HandleSliderUpdated);
	}

	private void OnDisable() {
		MatchManager.UnsubscribeFromMatchUpdates();
		MatchManager.OnNewData.RemoveListener(HandleNewData);
		MatchManager.MatchID = default;

		slider.onValueChanged.RemoveListener(HandleSliderUpdated);
	}

	private void HandleNewData(MatchSaveData data) {
		bool wasLive = IsLive;
		commands = data.commands.Select(x => x.Deserialized()).ToList();
		slider.maxValue = commands.Count;
		text.text = data.activePlayer;

		if (wasLive) {
			StartCoroutine(CoShowCommands(commands.Count, 1));
		}
		else {
			NotificationManager.Instance.AddNotification("Your opponent has made a new move");
			newMovesNotification.SetActive(true);
		}
	}

	public void AddCommand(ICommand command) {
		if (!IsLive) return;

		newCommands.Add(command);
	}

	private void PlayCommands(int target, int direction) {
		for (int i = currentCommandIndex; i != target; i+= direction) {
			ExecuteNextCommand(direction);
		}
	}

	private IEnumerator CoShowCommands(int target, int direction) {
		for (int i = currentCommandIndex; i != target; i+= direction) {
			ExecuteNextCommand(direction);
			yield return new WaitForSeconds(1f);
		}

		yield return null;
	}

	private void ExecuteNextCommand(int direction) {
		if (direction > 0)
			GoForward();
		else if (direction < 0)
			GoBack();
	}

	private void GoForward() {
		while (true) {
			if (IsLive) return;

			ExecuteCommand(commands[currentCommandIndex + 1]);

			if (IsLive) {
				newMovesNotification.SetActive(false);
			}

			if (currentCommandIndex < commands.Count && commands[currentCommandIndex].DoStep) {
				continue;
			}

			break;
		}
	}

	private void GoBack() {
		while (true) {
			if (currentCommandIndex < 0) return;

			StopAllCoroutines();
			UndoCommand(commands[currentCommandIndex]);

			if (currentCommandIndex >= 0 && commands[currentCommandIndex].DoStep) {
				continue;
			}

			break;
		}
	}

	private void ExecuteCommand(ICommand command) {
		command.Do(board);
		currentCommandIndex++;
		UpdateSlider();
	}

	private void UndoCommand(ICommand command) {
		command.Undo(board);
		currentCommandIndex--;
		UpdateSlider();
	}

	private void UpdateSlider() {
		slider.value = currentCommandIndex + 1;
	}

	#region UI Events

	public void ToStart() {
		StopAllCoroutines();
		PlayCommands(-1, -1);
	}

	public void StepBackward() {
		StopAllCoroutines();
		GoBack();
	}

	public void PlayManually() {
		StopAllCoroutines();
		StartCoroutine(CoShowCommands(commands.Count - 1, 1));
	}

	public void StepForward() {
		StopAllCoroutines();
		GoForward();
	}

	public void ToEnd() {
		StopAllCoroutines();
		PlayCommands(commands.Count - 1, 1);
	}

	private void HandleSliderUpdated(float value) {
		int nextTarget = (int)value - 1;
		if (nextTarget == currentCommandIndex) return;

		StopAllCoroutines();
		int replayDirection = Math.Sign(nextTarget - currentCommandIndex);
		PlayCommands(nextTarget, replayDirection);
	}

	#endregion

	public void Save() {
		MatchManager.UpdateMatch(commands.Concat(newCommands).ToList());
		newCommands.Clear();
	}
}
