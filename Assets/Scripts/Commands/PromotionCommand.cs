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

	public bool DoStep => true;

	public void Do(ChessBoard board) {
		board.DestroyPiece(pawnPosition.AsInt());
		board.GeneratePiece(promoteTo, team, promotePosition.AsInt());
	}

	public void Undo(ChessBoard board) {
		board.DestroyPiece(promotePosition.AsInt());
		board.GeneratePiece(PieceType.Pawn, team, pawnPosition.AsInt());
	}
}
