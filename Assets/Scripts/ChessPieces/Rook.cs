using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece {
	private bool hasMoved;
	public bool HasMoved => hasMoved;

	public override MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		if (!IsStraightPath(target)) return MoveType.None;

		path = GetLinearPathTo(target);
		return MoveType.Move;
	}

	public override void MoveTo(Vector2Int target) {
		base.MoveTo(target);
		hasMoved = true;
	}
}
