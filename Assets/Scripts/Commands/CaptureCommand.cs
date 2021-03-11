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

	public void Do(ChessBoard board, bool force) {
		board.DestroyPiece(removeAt.FloorToInt(), force);
		board.MovePiece(moveFrom.FloorToInt(), moveTo.FloorToInt(), force);
	}

	public void Undo(ChessBoard board, bool force) {
		board.MovePiece(moveTo.FloorToInt(), moveFrom.FloorToInt(), force);
		board.GeneratePiece(piece, team, removeAt.FloorToInt(), force);
	}
}
