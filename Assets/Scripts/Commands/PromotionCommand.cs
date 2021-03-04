using System;
using UnityEngine;

[Serializable]
public class PromotionCommand : ICommand {
	[SerializeField] private PieceType promoteTo;
	[SerializeField] private Vector2 pawnPosition;
	[SerializeField] private Vector2 promotePosition;
	[SerializeField] private Team team;

	public PromotionCommand(PieceType promoteTo, Vector2Int pawnPosition, Vector2Int promotePosition, Team team) {
		this.promoteTo = promoteTo;
		this.pawnPosition = pawnPosition;
		this.promotePosition = promotePosition;
		this.team = team;
	}

	public void Do(ChessBoard board) {
		board.DestroyPiece(pawnPosition.FloorToInt());
		board.GeneratePiece(promoteTo, team, promotePosition.FloorToInt());
	}

	public void Undo(ChessBoard board) {
		board.DestroyPiece(promotePosition.FloorToInt());
		board.GeneratePiece(PieceType.Pawn, team, pawnPosition.FloorToInt());
	}
}
