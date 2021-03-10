using System;
using UnityEngine;

[Serializable]
public class MoveCommand : ICommand {
	[SerializeField] private Vector2 from;
	[SerializeField] private Vector2 to;

	public MoveCommand(Vector2 from, Vector2 to) {
		this.from = from;
		this.to = to;
	}

	public void Do(ChessBoard board, bool force) {
		board.MovePiece(from.FloorToInt(), to.FloorToInt(), force);
	}

	public void Undo(ChessBoard board, bool force) {
		board.MovePiece(to.FloorToInt(), from.FloorToInt(), force);
	}
}
