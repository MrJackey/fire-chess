using System;
using UnityEngine;

[Serializable]
public class EnPassantCommand : ICommand {
	[SerializeField] private MoveCommand pawnMove;
	[SerializeField] private RemoveCommand pawnRemove;

	public bool DoStep => false;

	public EnPassantCommand(MoveCommand pawnMove, RemoveCommand pawnRemove) {
		this.pawnMove = pawnMove;
		this.pawnRemove = pawnRemove;
	}
	public void Do(ChessBoard board) {
		pawnMove.Do(board);
		pawnRemove.Do(board);
	}

	public void Undo(ChessBoard board) {
		pawnMove.Undo(board);
		pawnRemove.Undo(board);
	}
}
