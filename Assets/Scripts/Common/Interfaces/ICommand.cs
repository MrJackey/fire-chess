public interface ICommand {
	void Do(ChessBoard board);
	void Undo(ChessBoard board);
}
