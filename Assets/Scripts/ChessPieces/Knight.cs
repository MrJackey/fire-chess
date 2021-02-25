using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece {
	public override MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path) {
		Vector2Int diff = GetPositionDifference(target);
		path = new List<Vector2Int>();

		return Mathf.Abs(diff.x) == 2 && Mathf.Abs(diff.y) == 1 ||
		       Mathf.Abs(diff.y) == 2 && Mathf.Abs(diff.x) == 1 ? MoveType.Move : MoveType.None;
	}
}
