using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utils : MonoBehaviour {
	public static T GetRandomEnumValue<T> ( ) {
		List<T> values = Enum.GetValues(typeof(T)).Cast<T>( ).ToList( );
		return values[UnityEngine.Random.Range(0, values.Count)];
	}

	public static Vector2Int Vect2Round (Vector2 vector) {
		return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	public static Color ColorWithAlpha (Color color, float alpha) {
		color.a = alpha;
		return color;
	}

	public static float[ , ] GenerateRandomNoiseGrid (int width, int height, float minRange, float maxRange) {
		float[ , ] noise = new float[width, height];

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				noise[i, j] = UnityEngine.Random.Range(minRange, maxRange);
			}
		}

		return noise;
	}

	public static List<Vector2Int> GetCardinalDirections (Vector2Int vector) {
		return new List<Vector2Int>( ) {
			vector + Vector2Int.up,
			vector + Vector2Int.right,
			vector + Vector2Int.down,
			vector + Vector2Int.left
		};
	}
}
