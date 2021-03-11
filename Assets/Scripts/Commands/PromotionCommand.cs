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

	public void Do(ChessBoard board, bool force) {
		board.DestroyPiece(pawnPosition.FloorToInt(), force);
		board.GeneratePiece(promoteTo, team, promotePosition.FloorToInt(), force);
	}

	public void Undo(ChessBoard board, bool force) {
		board.DestroyPiece(promotePosition.FloorToInt(), force);
		board.GeneratePiece(PieceType.Pawn, team, pawnPosition.FloorToInt(), force);
	}
}
