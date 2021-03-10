using System;
using UnityEngine;

[Serializable]
public class DoubleStepCommand : ICommand {
	[SerializeField] private Vector2 moveFrom;
	[SerializeField] private Vector2 moveTo;
	[SerializeField] private Team pawnTeam;
	[SerializeField] private Vector2 skippedPosition;

	public DoubleStepCommand(Vector2 moveFrom, Vector2 moveTo, Team pawnTeam) {
		this.moveFrom = moveFrom;
		this.moveTo = moveTo;
		this.pawnTeam = pawnTeam;
		this.skippedPosition = this.moveTo - Vector2Int.up * (int)pawnTeam;
	}

	public void Do(ChessBoard board, bool force) {
		board.MovePiece(moveFrom.FloorToInt(), moveTo.FloorToInt(), force);
		board.EnablePassant(skippedPosition.FloorToInt(), (skippedPosition + Vector2.up * (int)pawnTeam).FloorToInt());
	}

	public void Undo(ChessBoard board, bool force) {
		board.MovePiece(moveTo.FloorToInt(), moveFrom.FloorToInt(), force);
		board.DisablePassant();
	}
}
