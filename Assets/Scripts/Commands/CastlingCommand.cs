using System;
using UnityEngine;

[Serializable]
public class CastlingCommand : ICommand {
	[SerializeField] private MoveCommand moveOne;
	[SerializeField] private MoveCommand moveTwo;

	public CastlingCommand(MoveCommand moveOne, MoveCommand moveTwo) {
		this.moveOne = moveOne;
		this.moveTwo = moveTwo;
	}

	public bool DoStep => false;

	public void Do(ChessBoard board) {
		moveOne.Do(board);
		moveTwo.Do(board);
	}

	public void Undo(ChessBoard board) {
		moveOne.Undo(board);
		moveTwo.Undo(board);
	}
}
