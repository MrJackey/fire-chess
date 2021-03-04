using System;
using UnityEngine;

[Serializable]
public class CaptureCommand : ICommand {
	[SerializeField] private Vector2 moveFrom;
	[SerializeField] private Vector2 moveTo;
	[SerializeField] private Vector2 removeAt;
	[SerializeField] private Team team;
	[SerializeField] private PieceType piece;

	public CaptureCommand(Vector2 moveFrom, Vector2 moveTo, Vector2 removeAt, ChessPiece capturedPiece) {
		this.moveFrom = moveFrom;
		this.moveTo = moveTo;
		this.removeAt = removeAt;
		this.team = capturedPiece.Team;
		this.piece = capturedPiece.Type;
	}

	public void Do(ChessBoard board) {
		board.DestroyPiece(removeAt.FloorToInt());
		board.MovePiece(moveFrom.FloorToInt(), moveTo.FloorToInt());
	}

	public void Undo(ChessBoard board) {
		board.MovePiece(moveTo.FloorToInt(), moveFrom.FloorToInt());
		board.GeneratePiece(piece, team, removeAt.FloorToInt());
	}
}
