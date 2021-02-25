public interface ICommand {
	bool DoStep { get; }
	void Do(ChessBoard board);
	void Undo(ChessBoard board);
}
