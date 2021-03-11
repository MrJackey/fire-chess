using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Grid))]
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

	[Header("VFX")]
	[SerializeField] private ParticleSystem selectedParticleSystem;
	[SerializeField] private ParticleSystem possibleMoveParticleSystem;
	[SerializeField] private Color forbiddenMoveColor;
	[SerializeField] private Color moveColor;
	[SerializeField] private float moveDuration = 0.75f;
	[SerializeField] private float jumpHeight = 2f;

	private Grid grid;
	private (int width, int height) boardSize;

	private ChessPiece[,] board;
	private ChessPiece selectedPiece;
	private Passant passant;
	private Stack<ChessPiece> forceDestroyedPieces;

	private King whiteKing;
	private King blackKing;

	private ParticleSystem[] possibleMoveParticleSystems;

#region Initialization

	private void Awake() {
		boardSize = (8, 8);
		board = new ChessPiece[boardSize.width, boardSize.height];
		passant = new Passant();
		forceDestroyedPieces = new Stack<ChessPiece>();

		grid = GetComponent<Grid>();
		selectedParticleSystem.Stop();

		GenerateBoard();

		possibleMoveParticleSystems = new ParticleSystem[27];
		for (int i = 0; i < 27; i++) {
			possibleMoveParticleSystems[i] = Instantiate(possibleMoveParticleSystem);
			possibleMoveParticleSystems[i].transform.parent = transform;
			possibleMoveParticleSystems[i].Stop();
		}
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
		Vector3 worldPosition = BoardToLocal(gridPosition);
		ChessPiece piece = Instantiate(prefab, worldPosition, prefab.transform.rotation);

		piece.transform.parent = transform;
		piece.Position = gridPosition;

		board[gridPosition.x, gridPosition.y] = piece;
		return piece;
	}
	
	public void GeneratePiece(PieceType piece, Team team, Vector2Int gridPosition, bool force) {
		if (force) {
			board[gridPosition.x, gridPosition.y] = forceDestroyedPieces.Pop();
		}
		else {
			GeneratePiece(GetPrefab(piece, team), gridPosition);
		}
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

	private bool IsPositionOnBoard(Vector2 position) {
		return !(position.x < 0) && !(position.x >= boardSize.width) &&
		       !(position.y < 0) && !(position.y >= boardSize.height);
	}

	private Vector2Int WorldToBoard(Vector3 worldPosition) {
		Vector3Int boardPosition = grid.WorldToCell(worldPosition);
		return (Vector2Int)boardPosition;
	}

	private Vector3 BoardToLocal(Vector2Int boardPosition) {
		return grid.GetCellCenterLocal((Vector3Int) boardPosition);
	}

#endregion

#region InputFlow
	public void HandlePlayerInput(Vector3 worldClick) {
		Vector2Int boardClick = WorldToBoard(worldClick);
		if (!IsPositionOnBoard(boardClick)) return;
		if (!replay.IsLive) return;
		// if (!MatchManager.IsMyTurn) return;

		ChessPiece clickedPiece = board[boardClick.x, boardClick.y];
		if (clickedPiece != null && clickedPiece.Team == MatchManager.MyTeam) {
			if (clickedPiece == selectedPiece) {
				DeselectPiece();
			}
			else {
				DeselectPiece();
				SelectPiece(clickedPiece);
			}

			return;
		}

		if (selectedPiece == null) return;

		MoveType typeOfMove =
			selectedPiece.TryMoveTo(boardClick, clickedPiece == null, out List<Vector2Int> path);

		if (typeOfMove == MoveType.None || IsPathObstructed(path)) return;

		if (TryExecuteMove(typeOfMove, selectedPiece, boardClick, true)) {
			// Prevent checking your own king
			King myKing = MatchManager.MyTeam == Team.White ? whiteKing : blackKing;
			if (IsChecked(myKing)) {
				replay.RevertLatestCommand();
				NotificationManager.Instance.AddNotification("That move would leave your king in check");
				return;
			}

			// Is the opponent's king checked?
			King opposingKing = MatchManager.MyTeam == Team.White ? blackKing : whiteKing;
			if (IsChecked(opposingKing, out ChessPiece checker, out List<Vector2Int> capturePath)) {
				if (IsCheckMate(opposingKing, checker, capturePath)) {
					NotificationManager.Instance.AddNotification("!!!CHECKMATE!!!");
				}
				else {
					NotificationManager.Instance.AddNotification("CHECK!!!");
				}
			}

			replay.Save();
			DeselectPiece();
		}
	}

	private bool TryExecuteMove(MoveType typeOfMove, ChessPiece movingPiece, Vector2Int boardClick, bool force) {
		ChessPiece clickedPiece = board[boardClick.x, boardClick.y];
		switch (typeOfMove) {
			case MoveType.Move:
				bool hasCaptured = false;
				CaptureCommand captureCommand = null;
				if (clickedPiece != null) {
					captureCommand = new CaptureCommand(movingPiece.Position, boardClick, boardClick, clickedPiece);
					replay.AddCommand(captureCommand, force);
					hasCaptured = true;
				}

				if (movingPiece is Pawn pawn && boardClick.y == (pawn.Team == Team.White ? boardSize.height - 1 : 0)) {
					PromotionCommand promotionCommand = new PromotionCommand(PieceType.Queen, movingPiece.Position, boardClick, pawn.Team);
					if (hasCaptured) {
						replay.RevertLatestCommand();
						replay.AddCommand(new CaptureAndPromoteCommand(captureCommand, promotionCommand), force);
					}
					else {
						replay.AddCommand(promotionCommand, force);
					}
					break;
				}

				if (!hasCaptured) {
					replay.AddCommand(new MoveCommand(movingPiece.Position, boardClick), force);
				}

				break;
			case MoveType.Castling:
				if (!TryCastling(movingPiece, boardClick, force)) {
					return false;
				}

				break;
			case MoveType.EnPassant:
				if (!TryEnPassant(movingPiece, boardClick, force)) {
					return false;
				}

				break;
			case MoveType.DoubleStep:
				if (clickedPiece != null) {
					return false;
				}

				replay.AddCommand(new DoubleStepCommand(movingPiece.Position, boardClick, movingPiece.Team), force);

				break;
			case MoveType.None:
				return false;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return true;
	}

#endregion

#region Logic

	private bool IsPathObstructed(IEnumerable<Vector2Int> path) {
		return path.Any(coord => board[coord.x, coord.y] != null);
	}

	private bool IsChecked(King king) {
		return IsChecked(king, out ChessPiece _, out List<Vector2Int> _);
	}

	private bool IsChecked(King king, out ChessPiece checker, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		checker = null;

		// check all directions
		for (float i = 0; i < Mathf.PI * 2; i += Mathf.PI / 4) {
			Vector2Int direction = new Vector2(Mathf.Cos(i), Mathf.Sin(i)).RoundToInt();
			ChessPiece closestPiece = GetClosestPiece(king.Position, direction);
			if (closestPiece == null || closestPiece.Team == king.Team) {
				continue;
			}

			if (closestPiece.TryMoveTo(king.Position, false, out List<Vector2Int> piecePath) ==
			    MoveType.None) {
				continue;
			}

			if (!IsPathObstructed(piecePath)) {
				path = piecePath;
				checker = closestPiece;
				return true;
			}
		}

		// check all possible knights
		IEnumerable opposingKnights = board.Enumerate().Where(x => x is Knight && x.Team != king.Team);
		foreach (Knight knight in opposingKnights) {
			if (knight == null) continue;

			if (knight.TryMoveTo(king.Position, false) == MoveType.None) {
				continue;
			}

			checker = knight;
			return true;
		}

		return false;
	}

	private bool IsCheckMate(King king, ChessPiece checker, List<Vector2Int> capturePath) {
		List<Vector2Int> kingMoves = GetPossibleMoves(king);

		foreach (Vector2Int possibleKingPosition in kingMoves) {
			MoveType moveType = king.TryMoveTo(possibleKingPosition, board[possibleKingPosition.x, possibleKingPosition.y] == null);
			if (!DoesMoveCheck(moveType, king, king, possibleKingPosition)) {
				return false;
			}
		}

		List<ChessPiece> kingAllies = board.Enumerate().Where(x => x != null && x.Team == king.Team && !(x is King)).ToList();

		foreach (ChessPiece piece in kingAllies) {
			MoveType moveType = piece.TryMoveTo(checker.Position, false, out List<Vector2Int> piecePath);
			if (moveType != MoveType.None && !IsPathObstructed(piecePath)) {
				if (!DoesMoveCheck(moveType, piece, king, checker.Position)) {
					return false;
				}
			}

			foreach (Vector2Int location in capturePath) {
				moveType = piece.TryMoveTo(location, true, out List<Vector2Int> piecePath2);
				if (moveType == MoveType.None) continue;
				if (IsPathObstructed(piecePath2)) continue;
				if (!TryExecuteMove(moveType, piece, location, true)) continue;

				if (!IsChecked(king)) {
					replay.RevertLatestCommand();
					return false;
				}
				replay.RevertLatestCommand();
			}
		}

		return true;
	}

	private bool DoesMoveCheck(MoveType move, ChessPiece mover, King king, Vector2Int newPosition) {
		if (!TryExecuteMove(move, mover, newPosition, true)) return true;

		if (!IsChecked(king)) {
			replay.RevertLatestCommand();
			return false;
		}

		replay.RevertLatestCommand();
		return true;
	}

	private ChessPiece GetClosestPiece(Vector2Int position, Vector2Int direction) {
		return GetClosestPiece(position, direction, out List<Vector2Int> _);
	}

	private ChessPiece GetClosestPiece(Vector2Int position, Vector2Int direction, out List<Vector2Int> path) {
		path = new List<Vector2Int>();
		Vector2Int nextPosition = position + direction;
		while (IsPositionOnBoard(nextPosition)) {
			if (board[nextPosition.x, nextPosition.y] != null) {
				return board[nextPosition.x, nextPosition.y];
			}

			path.Add(nextPosition);
			nextPosition += direction;
		}

		return null;
	}

	private List<Vector2Int> GetPossibleMoves(ChessPiece piece) {
		List<Vector2Int> moves = new List<Vector2Int>();

		if (piece is Knight knight) {
			foreach (Vector2Int knightMove in knight.PossibleMoves) {
				Vector2Int nextPosition = knight.Position + knightMove;
				if (!IsPositionOnBoard(nextPosition)) continue;

				ChessPiece otherPiece = board[nextPosition.x, nextPosition.y];
				if (otherPiece != null && otherPiece.Team == knight.Team) continue;

				moves.Add(nextPosition);
			}
		}
		else {
			const float smallNumber = 0.000001f;

			for (float i = 0; i < Mathf.PI * 2 - smallNumber; i += Mathf.PI / 4) {
				Vector2Int direction = new Vector2(Mathf.Cos(i), Mathf.Sin(i)).RoundToInt();
				Vector2Int nextPosition = piece.Position + direction;

				while (IsPositionOnBoard(nextPosition)) {
					ChessPiece otherPiece = board[nextPosition.x, nextPosition.y];
					if (otherPiece != null && otherPiece.Team == piece.Team) break;

					MoveType move = piece.TryMoveTo(nextPosition, otherPiece == null, out List<Vector2Int> path);
					if (move == MoveType.None) break;
					if (move == MoveType.EnPassant) {
						if (passant.Pawn == null) break;
						if (passant.Position != nextPosition || passant.Pawn.Team == piece.Team) break;
					}
					if (IsPathObstructed(path)) break;

					moves.Add(nextPosition);
					nextPosition += direction;
				}
			}
		}

		return moves;
	}

#endregion

#region Actions

	private void SelectPiece(ChessPiece piece) {
		selectedPiece = piece;
		selectedParticleSystem.transform.position = BoardToLocal(piece.Position);
		selectedParticleSystem.Play();

		List<Vector2Int> possibleMoves = GetPossibleMoves(piece);
		King myKing = MatchManager.MyTeam == Team.White ? whiteKing : blackKing;
		for (int i = 0; i < possibleMoves.Count; i++) {
			MoveType moveType = piece.TryMoveTo(possibleMoves[i],
				board[possibleMoves[i].x, possibleMoves[i].y] == null);

			ParticleSystem.MainModule module = possibleMoveParticleSystems[i].main;
			Color indicationColor = DoesMoveCheck(moveType, piece, myKing, possibleMoves[i])
				? forbiddenMoveColor
				: moveColor;
			module.startColor = indicationColor;

			possibleMoveParticleSystems[i].transform.position = BoardToLocal(possibleMoves[i]);
			possibleMoveParticleSystems[i].Play();
		}
	}

	public void DeselectPiece() {
		selectedPiece = null;
		selectedParticleSystem.Stop();

		foreach (ParticleSystem moveParticleSystem in possibleMoveParticleSystems) {
			moveParticleSystem.Stop();
		}
	}

	private bool TryCastling(ChessPiece movingKing, Vector2Int boardClick, bool force) {
		int castlingDirection = Math.Sign(boardClick.x - movingKing.Position.x);
		ChessPiece cornerPiece = board[
			castlingDirection > 0 ? boardSize.width - 1 : 0,
			movingKing.Team == Team.White ? 0 : boardSize.height - 1
		];

		if (cornerPiece == null || IsPathObstructed(movingKing.GetLinearPathTo(cornerPiece.Position))) return false;

		if (cornerPiece is Rook rook && !rook.HasMoved) {
			replay.AddCommand(new CastlingCommand(movingKing.Position, boardClick, rook.Position,  boardClick - Vector2Int.right * castlingDirection), force);
			return true;
		}

		return false;
	}

	private bool TryEnPassant(ChessPiece movingPawn, Vector2Int boardClick, bool force) {
		if (passant.Pawn != null && passant.Pawn.Team == movingPawn.Team) return false;
		if (passant.Pawn == null || passant.Position != boardClick ) return false;

		replay.AddCommand(new CaptureCommand(movingPawn.Position, boardClick, passant.Pawn.Position, passant.Pawn), force);

		return true;
	}

	public void MovePiece(Vector2Int from, Vector2Int to, bool force) {
		ChessPiece piece = board[from.x, from.y];
		Vector3 newPosition = BoardToLocal(to);


		board[piece.Position.x, piece.Position.y] = null;
		board[to.x, to.y] = piece;

		if (force) {
			piece.gameObject.transform.position = newPosition;
			piece.Position = to;
		}
		else {
			piece.gameObject.transform.DOKill();
			piece.transform.DOLocalJump(newPosition, jumpHeight, 1, moveDuration);
			piece.MoveTo(to);
		}
	}

	public void DestroyPiece(Vector2Int boardPosition, bool force) {
		ChessPiece piece = board[boardPosition.x, boardPosition.y];
		GameObject go = piece.gameObject;
		if (force) {
			forceDestroyedPieces.Push(piece);
		}
		else {
			go.transform.DOKill();
			Destroy(go);
		}

		board[boardPosition.x, boardPosition.y] = null;
	}

	public void EnablePassant(Vector2Int skippedPosition, Vector2Int pawnPosition) {
		passant.Position = skippedPosition;
		passant.Pawn = board[pawnPosition.x, pawnPosition.y] as Pawn;
	}

	public void DisablePassant() {
		passant.Pawn = null;
		passant.Position.Set(-1, -1);
	}

#endregion

}
