using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece {
	public override MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		if (!IsDiagonalPath(target)) return MoveType.None;

		path = GetLinearPathTo(target);
		return MoveType.Move;
	}
}
