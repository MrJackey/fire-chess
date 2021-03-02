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

	public bool DoStep => false;

	public void Do(ChessBoard board) {
		board.MovePiece(from.FloorToInt(), to.FloorToInt());
	}

	public void Undo(ChessBoard board) {
		board.MovePiece(to.FloorToInt(), from.FloorToInt());
	}
}
