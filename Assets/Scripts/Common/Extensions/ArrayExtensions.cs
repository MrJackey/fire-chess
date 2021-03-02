using System.Collections.Generic;

public static class ArrayExtensions {
	public static IEnumerable<T> Enumerate<T>(this T[,] array) {
		for (int i = 0; i < array.GetLength(0); i++) {
			for (int j = 0; j < array.GetLength(1); j++) {
				yield return array[i, j];
			}
		}
	}
}
