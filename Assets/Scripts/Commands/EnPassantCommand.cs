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
	public void Do(ChessBoard board) {
		pawnMove.Do(board);
		pawnCapture.Do(board);
	}

	public void Undo(ChessBoard board) {
		pawnMove.Undo(board);
		pawnCapture.Undo(board);
	}
}
