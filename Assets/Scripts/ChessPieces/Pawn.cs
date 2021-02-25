using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece {
	private bool hasMoved;

	public override MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		int xDifference = target.x - Position.x;
		int yDifference = target.y - Position.y;

		if (Mathf.Abs(xDifference) > 1) return MoveType.None;

		bool isNormalMove = yDifference == (int)Team;

		// Attacking
		if (Mathf.Abs(xDifference) == 1) {
			if (!isVacant) {
				return isNormalMove ? MoveType.Move : MoveType.None;
			}
			return isNormalMove ? MoveType.EnPassant : MoveType.None;
		}

		if (!isVacant) return MoveType.None;

		// Movement
		if (hasMoved || Mathf.Abs(yDifference) == 1) {
			return isNormalMove ? MoveType.Move : MoveType.None;
		}

		return yDifference == (int)Team * 2 ? MoveType.DoubleStep : MoveType.None;
	}

	public override void MoveTo(Vector2Int target) {
		base.MoveTo(target);
		hasMoved = true;
	}
}
