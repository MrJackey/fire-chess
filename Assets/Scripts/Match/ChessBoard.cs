using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class ChessBoard : MonoBehaviour {
	[Header("White Pieces")]
	[SerializeField] private Pawn whitePawnPrefab;
	[SerializeField] private Rook whiteRookPrefab;
	[SerializeField] private Knight whiteKnightPrefab;
	[SerializeField] private Bishop whiteBishopPrefab;
	[SerializeField] private Queen whiteQueenPrefab;
	[SerializeField] private King whiteKingPrefab;

	[Header("Black Pieces")]
	[SerializeField] private Pawn blackPawnPrefab;
	[SerializeField] private Rook blackRookPrefab;
	[SerializeField] private Knight blackKnightPrefab;
	[SerializeField] private Bishop blackBishopPrefab;
	[SerializeField] private Queen blackQueenPrefab;
	[SerializeField] private King blackKingPrefab;

	[Space]
	[SerializeField] private ReplaySystem replay;
	[SerializeField] private ParticleSystem selectedParticleSystem;

	private Tilemap tilemap;
	private (int width, int height) boardSize;

	private ChessPiece[,] board;
	private ChessPiece selectedPiece;
	private Passant passant;

	private King whiteKing;
	private King blackKing;

	#region Initialization

	private void Awake() {
		boardSize = (8, 8);
		board = new ChessPiece[boardSize.width, boardSize.height];
		passant = new Passant();

		tilemap = GetComponent<Tilemap>();
		selectedParticleSystem.Stop();

		GenerateBoard();
	}

	private void GenerateBoard() {
		// Pawns
		for (int i = 0; i < boardSize.width; i++) {
			GeneratePiece(whitePawnPrefab, new Vector2Int(i, 1));
			GeneratePiece(blackPawnPrefab, new Vector2Int(i, 6));
		}

		// Rooks
		GeneratePiece(whiteRookPrefab, new Vector2Int(0, 0));
		GeneratePiece(whiteRookPrefab, new Vector2Int(7, 0));
		GeneratePiece(blackRookPrefab, new Vector2Int(0, 7));
		GeneratePiece(blackRookPrefab, new Vector2Int(7, 7));

		// Knights
		GeneratePiece(whiteKnightPrefab, new Vector2Int(1, 0));
		GeneratePiece(whiteKnightPrefab, new Vector2Int(6, 0));
		GeneratePiece(blackKnightPrefab, new Vector2Int(1, 7));
		GeneratePiece(blackKnightPrefab, new Vector2Int(6, 7));

		// Bishops
		GeneratePiece(whiteBishopPrefab, new Vector2Int(2, 0));
		GeneratePiece(whiteBishopPrefab, new Vector2Int(5, 0));
		GeneratePiece(blackBishopPrefab, new Vector2Int(2, 7));
		GeneratePiece(blackBishopPrefab, new Vector2Int(5, 7));

		// Queens
		GeneratePiece(whiteQueenPrefab, new Vector2Int(3, 0));
		GeneratePiece(blackQueenPrefab, new Vector2Int(3, 7));

		// Kings
		whiteKing = (King)GeneratePiece(whiteKingPrefab, new Vector2Int(4, 0));
		blackKing = (King)GeneratePiece(blackKingPrefab, new Vector2Int(4, 7));
	}

	private ChessPiece GeneratePiece(ChessPiece prefab, Vector2Int gridPosition) {
		Vector3 worldPosition = tilemap.GetCellCenterWorld((Vector3Int)gridPosition);
		ChessPiece piece = Instantiate(prefab, worldPosition, Quaternion.identity);

		piece.transform.parent = transform;
		piece.Position = gridPosition;

		board[gridPosition.x, gridPosition.y] = piece;
		return piece;
	}
	
	public void GeneratePiece(PieceType piece, Team team, Vector2Int gridPosition) {
		GeneratePiece(GetPrefab(piece, team), gridPosition);
	}

	private ChessPiece GetPrefab(PieceType piece, Team team) {
		return piece switch {
			PieceType.Pawn => team == Team.White ? whitePawnPrefab : blackPawnPrefab,
			PieceType.Rook => team == Team.White ? whiteRookPrefab : blackRookPrefab,
			PieceType.Bishop => team == Team.White ? whiteBishopPrefab : blackBishopPrefab,
			PieceType.Knight => team == Team.White ? whiteKnightPrefab : blackKnightPrefab,
			PieceType.Queen => team == Team.White ? whiteQueenPrefab : blackQueenPrefab,
			PieceType.King => team == Team.White ? whiteKingPrefab : blackKingPrefab,
			_ => throw new ArgumentOutOfRangeException(),
		};
	}

	#endregion

	#region BoardHelpers

	private bool IsPositionIsOnBoard(Vector2 worldPosition) {
		return !(worldPosition.x < 0) && !(worldPosition.x >= boardSize.width) &&
		       !(worldPosition.y < 0) && !(worldPosition.y >= boardSize.height);
	}

	private Vector2Int WorldToBoard(Vector3 worldPosition) {
		Vector3Int boardPosition = tilemap.WorldToCell(worldPosition);
		return (Vector2Int)boardPosition;
	}

	private Vector3 BoardToWorld(Vector2Int boardPosition) {
		return tilemap.GetCellCenterWorld((Vector3Int)boardPosition);
	}

	#endregion

	public void HandlePlayerInput(Vector3 worldClick) {
		if (!IsPositionIsOnBoard(worldClick)) return;
		if (!replay.IsLive) return;
		// if (!MatchManager.IsMyTurn) return;

		Vector2Int boardClick = WorldToBoard(worldClick);

		ChessPiece clickedPiece = board[boardClick.x, boardClick.y];
		if (clickedPiece != null && clickedPiece.Team == MatchManager.MyTeam) {
			if (clickedPiece == selectedPiece) {
				DeselectPiece();
			}
			else {
				SelectPiece(clickedPiece);
			}

			return;
		}

		if (selectedPiece == null) return;

		MoveType typeOfMove =
			selectedPiece.TryMoveTo(boardClick, clickedPiece == null, out List<Vector2Int> path);

		if (typeOfMove == MoveType.None || IsPathObstructed(path)) return;

		if (passant.Pawn != null && !(replay.Commands[replay.Commands.Count - 1] is DoubleStepCommand)) {
			DisablePassant();
		}

		if (TryExecuteMove(typeOfMove, clickedPiece, boardClick)) {
			replay.Save();
			DeselectPiece();
		}
	}

	private bool TryExecuteMove(MoveType typeOfMove, ChessPiece clickedPiece, Vector2Int boardClick) {
		switch (typeOfMove) {
			case MoveType.Move:
				bool hasCaptured = false;
				CaptureCommand captureCommand = null;
				if (clickedPiece != null) {
					captureCommand = new CaptureCommand(selectedPiece.Position, boardClick, boardClick, clickedPiece);
					replay.AddCommand(captureCommand);
					hasCaptured = true;
				}

				if (selectedPiece is Pawn pawn && boardClick.y == (pawn.Team == Team.White ? boardSize.height - 1 : 0)) {
					PromotionCommand promotionCommand = new PromotionCommand(PieceType.Queen, selectedPiece.Position, boardClick, pawn.Team);
					if (hasCaptured) {
						replay.RevertNewCommands();
						replay.AddCommand(new CaptureAndPromoteCommand(captureCommand, promotionCommand));
					}
					else {
						replay.AddCommand(new PromotionCommand(PieceType.Queen, selectedPiece.Position, boardClick, pawn.Team));
					}
					break;
				}

				if (!hasCaptured) {
					replay.AddCommand(new MoveCommand(selectedPiece.Position, boardClick));
				}

				break;
			case MoveType.Castling:
				if (!TryCastling(boardClick)) {
					return false;
				}

				break;
			case MoveType.EnPassant:
				if (!TryEnPassant(boardClick)) {
					return false;
				}

				break;
			case MoveType.DoubleStep:
				if (clickedPiece != null) {
					return false;
				}

				replay.AddCommand(new DoubleStepCommand(selectedPiece.Position, boardClick, selectedPiece.Team));

				break;
			case MoveType.None:
				return false;
			default:
				throw new ArgumentOutOfRangeException();
		}

		// Prevent checking your own king
		King myKing = MatchManager.MyTeam == Team.White ? whiteKing : blackKing;
		if (IsChecked(myKing)) {
			replay.RevertNewCommands();
			NotificationManager.Instance.AddNotification("You are not allowed to check yourself");
			return false;
		}

		King opposingKing = MatchManager.MyTeam == Team.White ? blackKing : whiteKing;
		if (IsChecked(opposingKing)) {
			NotificationManager.Instance.AddNotification("CHECK!!!");
		}

		return true;
	}

	private bool IsPathObstructed(IEnumerable<Vector2Int> path) {
		return path.Any(coord => board[coord.x, coord.y] != null);
	}

	private bool IsChecked(King king) {
		// check all directions
		for (float i = 0; i < Mathf.PI * 2; i += Mathf.PI / 4) {
			Vector2Int direction = new Vector2(Mathf.Cos(i), Mathf.Sin(i)).CeilToInt();
			ChessPiece closestPiece = GetClosestPiece(king.Position, direction);
			if (closestPiece == null || closestPiece.Team == king.Team) {
				continue;
			}

			if (closestPiece.TryMoveTo(king.Position, false, out List<Vector2Int> path) ==
			    MoveType.None) {
				continue;
			}

			if (!IsPathObstructed(path)) {
				return true;
			}
		}

		// check all possible knights
		IEnumerable opposingKnights = board.Enumerate().Where(x => x is Knight && x.Team != king.Team);
		foreach (Knight knight in opposingKnights) {
			if (knight == null) continue;

			if (knight.TryMoveTo(king.Position, false, out List<Vector2Int> path) == MoveType.None) {
				continue;
			}

			if (!IsPathObstructed(path)) {
				return true;
			}
		}

		return false;
	}

	private ChessPiece GetClosestPiece(Vector2Int position, Vector2Int direction) {
		Vector2Int nextPosition = position + direction;
		while (IsPositionIsOnBoard(nextPosition)) {
			if (board[nextPosition.x, nextPosition.y] != null) {
				return board[nextPosition.x, nextPosition.y];
			}

			nextPosition += direction;
		}

		return null;
	}

	#region Actions

	private void SelectPiece(ChessPiece piece) {
		selectedPiece = piece;
		selectedParticleSystem.transform.position = BoardToWorld(piece.Position);
		selectedParticleSystem.Play();
	}

	public void DeselectPiece() {
		selectedPiece = null;
		selectedParticleSystem.Stop();
	}

	private bool TryCastling(Vector2Int boardClick) {
		int castlingDirection = Math.Sign(boardClick.x - selectedPiece.Position.x);
		ChessPiece cornerPiece = board[
			castlingDirection > 0 ? boardSize.width - 1 : 0,
			selectedPiece.Team == Team.White ? 0 : boardSize.height - 1
		];

		if (cornerPiece == null || IsPathObstructed(selectedPiece.GetLinearPathTo(cornerPiece.Position))) return false;

		if (cornerPiece is Rook rook && !rook.HasMoved) {
			replay.AddCommand(new CastlingCommand(selectedPiece.Position, boardClick, rook.Position,  boardClick - Vector2Int.right * castlingDirection));
			return true;
		}

		return false;
	}

	private bool TryEnPassant(Vector2Int boardClick) {
		if (passant.Pawn == null || passant.Position != boardClick) return false;

		replay.AddCommand(new CaptureCommand(selectedPiece.Position, boardClick, passant.Pawn.Position, passant.Pawn));

		return true;
	}

	public void MovePiece(Vector2Int from, Vector2Int to) {
		ChessPiece piece = board[from.x, from.y];
		piece.transform.position = BoardToWorld(to);

		board[piece.Position.x, piece.Position.y] = null;
		board[to.x, to.y] = piece;
		piece.MoveTo(to);
	}

	public void DestroyPiece(Vector2Int boardPosition) {
		Destroy(board[boardPosition.x, boardPosition.y].gameObject);
		board[boardPosition.x, boardPosition.y] = null;
	}

	public void EnablePassant(Vector2Int skippedPosition, Vector2Int pawnPosition) {
		passant.Position = skippedPosition;
		passant.Pawn = board[pawnPosition.x, pawnPosition.y] as Pawn;
	}

	public void DisablePassant() {
		passant.Pawn = null;
	}

	#endregion
}
