using System;
using UnityEngine;

[Serializable]
public class CaptureAndPromoteCommand : ICommand {
	[SerializeField] private CaptureCommand captureCommand;
	[SerializeField] private PromotionCommand promotionCommand;

	public CaptureAndPromoteCommand(CaptureCommand captureCommand, PromotionCommand promotionCommand) {
		this.captureCommand = captureCommand;
		this.promotionCommand = promotionCommand;
	}

	public void Do(ChessBoard board) {
		captureCommand.Do(board);
		promotionCommand.Do(board);
	}

	public void Undo(ChessBoard board) {
		promotionCommand.Undo(board);
		captureCommand.Undo(board);
	}
}
