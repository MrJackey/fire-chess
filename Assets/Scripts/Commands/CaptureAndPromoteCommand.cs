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

	public void Do(ChessBoard board, bool force) {
		captureCommand.Do(board, force);
		promotionCommand.Do(board, force);
	}

	public void Undo(ChessBoard board, bool force) {
		promotionCommand.Undo(board, force);
		captureCommand.Undo(board, force);
	}
}
