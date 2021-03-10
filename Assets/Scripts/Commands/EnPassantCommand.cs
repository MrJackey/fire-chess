using System;
using UnityEngine;

[Serializable]
public class EnPassantCommand : ICommand {
	[SerializeField] private MoveCommand pawnMove;
	[SerializeField] private CaptureCommand pawnCapture;

	public EnPassantCommand(MoveCommand pawnMove, CaptureCommand pawnCapture) {
		this.pawnMove = pawnMove;
		this.pawnCapture = pawnCapture;
	}
	public void Do(ChessBoard board, bool force) {
		pawnMove.Do(board, force);
		pawnCapture.Do(board, force);
	}

	public void Undo(ChessBoard board, bool force) {
		pawnMove.Undo(board, force);
		pawnCapture.Undo(board, force);
	}
}
