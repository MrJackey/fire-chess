using System;
using UnityEngine;

[Serializable]
public class CastlingCommand : ICommand {
	[SerializeField] private Vector2 moveOneFrom;
	[SerializeField] private Vector2 moveOneTo;
	[SerializeField] private Vector2 moveTwoFrom;
	[SerializeField] private Vector2 moveTwoTo;

	public CastlingCommand(Vector2 moveOneFrom, Vector2 moveOneTo, Vector2 moveTwoFrom, Vector2 moveTwoTo) {
		this.moveOneFrom = moveOneFrom;
		this.moveOneTo = moveOneTo;
		this.moveTwoFrom = moveTwoFrom;
		this.moveTwoTo = moveTwoTo;
	}

	public void Do(ChessBoard board, bool force) {
		board.MovePiece(moveOneFrom.FloorToInt(), moveOneTo.FloorToInt(), force);
		board.MovePiece(moveTwoFrom.FloorToInt(), moveTwoTo.FloorToInt(), force);
	}

	public void Undo(ChessBoard board, bool force) {
		board.MovePiece(moveOneTo.FloorToInt(), moveOneFrom.FloorToInt(), force);
		board.MovePiece(moveTwoTo.FloorToInt(), moveTwoFrom.FloorToInt(), force);
	}
}
