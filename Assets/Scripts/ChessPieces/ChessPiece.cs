using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: Make POCO - Only need sprites on board via a visualiser??
public abstract class ChessPiece : MonoBehaviour {
	[SerializeField] private Team team = Team.White;
	public Team Team => team;

	private Vector2Int position;
	public Vector2Int Position {
		get => position;
		set => position = value;
	}

	public PieceType Type =>
		this switch {
			Pawn _ => PieceType.Pawn,
			Rook _ => PieceType.Rook,
			Bishop _ => PieceType.Bishop,
			Knight _ => PieceType.Knight,
			Queen _ => PieceType.Queen,
			King _ => PieceType.King,
			_ => throw new ArgumentOutOfRangeException()
	};

	public abstract MoveType TryMoveTo(Vector2Int target, bool isVacant, out List<Vector2Int> path);

	public virtual void MoveTo(Vector2Int newPosition) {
		position = newPosition;
	}

	// Checks if the target path follows a '+' shape
	protected bool IsStraightPath(Vector2Int target) {
		Vector2Int diff = GetPositionDifference(target);
		return diff.x == 0 && diff.y != 0 ||
		       diff.y == 0 && diff.x != 0;
	}

	// Checks if the target path follows a 'x' shape
	protected bool IsDiagonalPath(Vector2Int target) {
		Vector2Int diff = GetPositionDifference(target);
		return Mathf.Abs(diff.x) == Mathf.Abs(diff.y);
	}

	protected Vector2Int GetPositionDifference(Vector2Int target) {
		return target - position;
	}

	public List<Vector2Int> GetLinearPathTo(Vector2Int target) {
		if (!IsStraightPath(target) && !IsDiagonalPath(target)) throw new ArgumentOutOfRangeException(nameof(target), "The path to the target is not linear");

		List<Vector2Int> path = new List<Vector2Int>();
		Vector2Int pathPosition = Position;
		Vector2Int pathDifference = GetPositionDifference(target);
		Vector2Int pathDirection = new Vector2Int(Math.Sign(pathDifference.x),Math.Sign(pathDifference.y));

		while (pathPosition != target - pathDirection) {
			pathPosition += pathDirection;
			pathDifference -= pathDirection;
			path.Add(pathPosition);
		}

		return path;
	}
}
