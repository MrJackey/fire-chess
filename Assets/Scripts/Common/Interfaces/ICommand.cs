public interface ICommand {
	void Do(ChessBoard board, bool force = false);
	void Undo(ChessBoard board, bool force = false);
}
