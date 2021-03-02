using System;
using UnityEngine;

[Serializable]
public class RemoveCommand : ICommand {
	[SerializeField] private Vector2 position;
	[SerializeField] private Team team;
	[SerializeField] private PieceType piece;

	public RemoveCommand(Vector2 position, ChessPiece piece) {
		this.position = position;
		this.team = piece.Team;
		this.piece = piece.Type;
	}

	public bool DoStep => true;

	public void Do(ChessBoard board) {
		board.DestroyPiece(position.FloorToInt());
	}

	public void Undo(ChessBoard board) {
		board.GeneratePiece(piece, team, position.FloorToInt());
	}
}
