using UnityEngine;

public static class Vector2Extensions {
	public static Vector2Int AsInt(this Vector2 v) {
		return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
	}
}
