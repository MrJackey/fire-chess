using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece {
	private bool hasMoved;
	public bool HasMoved => hasMoved;

	public override MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		if (!IsDiagonalPath(target) && !IsStraightPath(target)) return MoveType.None;

		int xDifference = target.x - Position.x;
		if (HasMoved || Mathf.Abs(xDifference) != 2) {
			path = GetLinearPathTo(target);
			return path.Count < 1 ? MoveType.Move : MoveType.None;
		}

		int yDifference = target.y - Position.y;

		return yDifference == 0 ? MoveType.Castling : MoveType.None;
	}

	public override void MoveTo(Vector2Int target) {
		base.MoveTo(target);
		hasMoved = true;
	}
}
