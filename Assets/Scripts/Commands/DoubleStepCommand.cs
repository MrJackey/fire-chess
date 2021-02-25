using System;
using UnityEngine;

[Serializable]
public class DoubleStepCommand : ICommand {
	[SerializeField] private MoveCommand pawnMove;
	[SerializeField] private Team pawnTeam;
	[SerializeField] private Vector2 skippedPosition;

	public bool DoStep => false;

	public DoubleStepCommand(MoveCommand pawnMove, Team pawnTeam, Vector2 skippedPosition) {
		this.pawnMove = pawnMove;
		this.pawnTeam = pawnTeam;
		this.skippedPosition = skippedPosition;
	}

	public void Do(ChessBoard board) {
		pawnMove.Do(board);
		board.EnablePassant(skippedPosition.AsInt(), (skippedPosition + Vector2.up * (int)pawnTeam).AsInt());
	}

	public void Undo(ChessBoard board) {
		pawnMove.Undo(board);
		board.DisablePassant();
	}
}
