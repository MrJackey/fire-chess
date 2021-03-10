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
	[SerializeField] private TMP_Text activePlayerTurn;
	[SerializeField] private Slider slider;
	[SerializeField] private GameObject newMovesNotification;

	[Space]
	[SerializeField] private float showCommandDelay;

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
		slider.maxValue = commands.Count - 1;
		activePlayerTurn.text = MatchManager.IsMyTurn ? "Your Turn" : $"{data.OpponentName}'s Turn";

		if (wasLive || IsLive) {
			if (currentCommandIndex == -1 || MatchManager.IsMyTurn) {
				StartCoroutine(CoShowCommands(commands.Count - 1, 1));
			}
		}
		else {
			NotificationManager.Instance.AddNotification("Your opponent has made a new move");
			newMovesNotification.SetActive(true);
		}

		UpdateSlider();
	}

	public void AddCommand(ICommand command, bool force) {
		if (!IsLive) return;

		newCommands.Add(command);
		command.Do(board, force);
	}

	private void PlayCommands(int target, int direction) {
		for (int i = currentCommandIndex; i != target; i+= direction) {
			ExecuteNextCommand(direction);
		}
	}

	private IEnumerator CoShowCommands(int target, int direction) {
		for (int i = currentCommandIndex; i != target; i+= direction) {
			ExecuteNextCommand(direction);
			yield return new WaitForSeconds(showCommandDelay);
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
		if (IsLive) return;

		ExecuteCommand(commands[currentCommandIndex + 1]);

		if (IsLive) {
			newMovesNotification.SetActive(false);
		}
	}

	private void GoBack() {
		if (currentCommandIndex < 0) return;

		UndoCommand(commands[currentCommandIndex]);
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
		slider.value = currentCommandIndex;
	}

	#region UI Events

	public void ToStart() {
		PrepareReplayUpdate();
		PlayCommands(-1, -1);
	}

	public void StepBackward() {
		PrepareReplayUpdate();
		GoBack();
	}

	public void PlayManually() {
		PrepareReplayUpdate();
		StartCoroutine(CoShowCommands(commands.Count - 1, 1));
	}

	public void StepForward() {
		PrepareReplayUpdate();
		GoForward();
	}

	public void ToEnd() {
		PrepareReplayUpdate();
		PlayCommands(commands.Count - 1, 1);
	}

	private void PrepareReplayUpdate() {
		board.DeselectPiece();
		StopAllCoroutines();
	}

	private void HandleSliderUpdated(float value) {
		int nextTarget = (int)value;
		if (nextTarget == currentCommandIndex) return;

		StopAllCoroutines();
		int replayDirection = Math.Sign(nextTarget - currentCommandIndex);
		PlayCommands(nextTarget, replayDirection);
	}

	#endregion

	public void Save() {
		currentCommandIndex += newCommands.Count;
		MatchManager.UpdateMatch(commands.Concat(newCommands).ToList());

		newCommands[newCommands.Count - 1].Undo(board, true);
		newCommands[newCommands.Count - 1].Do(board);
		newCommands.Clear();
	}

	public void RevertLatestCommand() {
		newCommands[newCommands.Count - 1].Undo(board, true);
		newCommands.RemoveAt(newCommands.Count - 1);
	}
}
