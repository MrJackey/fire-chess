using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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

	#region Initialization

	private void Awake() {
		boardSize = (8, 8);
		board = new ChessPiece[boardSize.width, boardSize.height];

		tilemap = GetComponent<Tilemap>();
		selectedParticleSystem.Stop();
		passant = new Passant();

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
		GeneratePiece(whiteKingPrefab, new Vector2Int(4, 0));
		GeneratePiece(blackKingPrefab, new Vector2Int(4, 7));
	}

	private void GeneratePiece(ChessPiece prefab, Vector2Int gridPosition) {
		Vector3 worldPosition = tilemap.GetCellCenterWorld((Vector3Int)gridPosition);
		ChessPiece piece = Instantiate(prefab, worldPosition, Quaternion.identity);

		piece.transform.parent = transform;
		piece.Position = gridPosition;

		board[gridPosition.x, gridPosition.y] = piece;
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

	private bool PositionIsOnBoard(Vector2 worldPosition) {
		return !(worldPosition.x < 0) && !(worldPosition.x > boardSize.width) &&
		       !(worldPosition.y < 0) && !(worldPosition.y > boardSize.height);
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
		if (!PositionIsOnBoard(worldClick)) return;
		if (!replay.IsLive) return;
		// if (!MatchManager.IsMyTurn) return;

		Vector2Int boardClick = WorldToBoard(worldClick);

		ChessPiece clickedPiece = board[boardClick.x, boardClick.y];
		if (clickedPiece != null && clickedPiece.Team == MatchManager.Team) {
			SelectPiece(clickedPiece);
			return;
		}

		if (selectedPiece == null) return;

		MoveType typeOfMove = selectedPiece.TryMoveTo(boardClick, clickedPiece == null, out List<Vector2Int> path);

		if (typeOfMove == MoveType.None || IsPathObstructed(path)) return;

		if (passant.Pawn != null && replay.Commands.LastOrDefault(x => x is DoubleStepCommand) != null) {
			DisablePassant();
		}

		if (ExecuteMove(typeOfMove, clickedPiece, boardClick)) {
			replay.Save();
			DeselectPiece();
		}
	}

	private bool ExecuteMove(MoveType typeOfMove, ChessPiece clickedPiece, Vector2Int boardClick) {
		switch (typeOfMove) {
			case MoveType.Move:
				if (clickedPiece != null) {
					replay.AddCommand(new RemoveCommand(boardClick, clickedPiece));
				}

				if (selectedPiece is Pawn pawn && boardClick.y == (pawn.Team == Team.White ? boardSize.height - 1 : 0)) {
					replay.AddCommand(new PromotionCommand(PieceType.Queen, selectedPiece.Position, boardClick, pawn.Team));
					break;
				}

				replay.AddCommand(new MoveCommand(selectedPiece.Position, boardClick));

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

				MoveCommand pawnMove = new MoveCommand(selectedPiece.Position, boardClick);
				replay.AddCommand(new DoubleStepCommand(pawnMove, selectedPiece.Team, boardClick - Vector2Int.up * (int) selectedPiece.Team));

				break;
			case MoveType.None:
				return false;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return true;
	}

	private bool IsPathObstructed(IEnumerable<Vector2Int> path) {
		return path.Any(coord => board[coord.x, coord.y] != null);
	}

	#region Actions

	private void SelectPiece(ChessPiece piece) {
		// Debug.Log($"[Board] Selecting piece: {piece}");
		selectedPiece = piece;
		selectedParticleSystem.transform.position = BoardToWorld(piece.Position);
		selectedParticleSystem.Play();
	}

	private void DeselectPiece() {
		// Debug.Log($"[Board] Deselecting piece: {selectedPiece}");
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
			MoveCommand kingMove = new MoveCommand(selectedPiece.Position, boardClick);
			MoveCommand rookMove = new MoveCommand(rook.Position, boardClick - Vector2Int.right * castlingDirection);
			replay.AddCommand(new CastlingCommand(kingMove, rookMove));
			return true;
		}

		return false;
	}

	private bool TryEnPassant(Vector2Int boardClick) {
		if (passant.Pawn == null || passant.Position != boardClick) return false;

		RemoveCommand pawnRemove = new RemoveCommand(passant.Pawn.Position, passant.Pawn);
		MoveCommand pawnMove = new MoveCommand(selectedPiece.Position, boardClick);
		replay.AddCommand(new EnPassantCommand(pawnMove, pawnRemove));

		return true;
	}

	public void MovePiece(Vector2Int from, Vector2Int to) {
		// Debug.Log($"[Board] Moving piece from: {from} to: {to}");
		ChessPiece piece = board[from.x, from.y];
		piece.transform.position = BoardToWorld(to);

		board[piece.Position.x, piece.Position.y] = null;
		board[to.x, to.y] = piece;
		piece.MoveTo(to);
	}

	public void DestroyPiece(Vector2Int boardPosition) {
		// Debug.Log($"[Board] Destroying piece at: {boardPosition}");
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
