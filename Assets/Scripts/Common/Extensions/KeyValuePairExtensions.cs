﻿using System.Collections.Generic;

public static class KeyValuePairExtensions {
	public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 key, out T2 value) {
		key = pair.Key;
		value = pair.Value;
	}
}
