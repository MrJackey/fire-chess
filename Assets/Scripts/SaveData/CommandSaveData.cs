using System;
using UnityEngine;

public enum CommandType {
	Move,
	Remove,
	Castling,
	Promotion,
	DoubleStep,
	EnPassant,
	CaptureAndPromote,
}

[Serializable]
public class CommandSaveData {
	public CommandType commandType;
	public string data;

	public CommandSaveData(ICommand command) {
		this.data = JsonUtility.ToJson(command);
		SetCommandType(command);
	}

	private void SetCommandType(ICommand command) {
		commandType = command switch {
			MoveCommand _ => CommandType.Move,
			CaptureCommand _ => CommandType.Remove,
			CastlingCommand _ => CommandType.Castling,
			PromotionCommand _ => CommandType.Promotion,
			DoubleStepCommand _ => CommandType.DoubleStep,
			EnPassantCommand _ => CommandType.EnPassant,
			CaptureAndPromoteCommand _ => CommandType.CaptureAndPromote,
			_ => commandType
		};
	}

	public ICommand Deserialized() {
		return commandType switch {
			CommandType.Move => JsonUtility.FromJson<MoveCommand>(data),
			CommandType.Remove => JsonUtility.FromJson<CaptureCommand>(data),
			CommandType.Castling => JsonUtility.FromJson<CastlingCommand>(data),
			CommandType.Promotion => JsonUtility.FromJson<PromotionCommand>(data),
			CommandType.DoubleStep => JsonUtility.FromJson<DoubleStepCommand>(data),
			CommandType.EnPassant => JsonUtility.FromJson<EnPassantCommand>(data),
			CommandType.CaptureAndPromote => JsonUtility.FromJson<CaptureAndPromoteCommand>(data),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
