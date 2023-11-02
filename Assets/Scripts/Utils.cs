using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils {
	/// <summary>
	///		Get a random enum value
	/// </summary>
	/// <typeparam name="T">The type of the enum</typeparam>
	/// <returns>
	///		<strong>T</strong> which is the randomly chosen value
	/// </returns>
	public static T GetRandomEnumValue<T> ( ) {
		List<T> values = Enum.GetValues(typeof(T)).Cast<T>( ).ToList( );
		return values[UnityEngine.Random.Range(0, values.Count)];
	}

	/// <summary>
	///		Round all elements of a 2D vector to the nearest integer
	/// </summary>
	/// <param name="vector">The 2D vector to round</param>
	/// <returns>
	///		<strong>Vector2Int</strong> that is the rounded 2D vector 
	/// </returns>
	public static Vector2Int Vect2Round (Vector2 vector) {
		return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	/// <summary>
	///		Round all elements of a 3D vector to the nearest integer
	/// </summary>
	/// <param name="vector">The 3D vector to round</param>
	/// <returns>
	///		<strong>Vector3Int</strong> that is the rounded 3D vector
	/// </returns>
	public static Vector3Int Vect3Round (Vector3 vector) {
		return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	/// <summary>
	///		Get the specified color with a specified alpha value applied to it
	/// </summary>
	/// <param name="color">The color to alter</param>
	/// <param name="alpha">The alpha value of the outputted color</param>
	/// <returns>
	///		<strong>Color</strong> that is the specified color with the modified alpha value
	/// </returns>
	public static Color ColorWithAlpha (Color color, float alpha) {
		color.a = alpha;
		return color;
	}

	/// <summary>
	///		Generate a random grid of noise
	/// </summary>
	/// <param name="width">The width of the grid</param>
	/// <param name="height">The height of the grid</param>
	/// <param name="minRange">The minimum value of the randomly generated noise values</param>
	/// <param name="maxRange">The maximum value of the randomly generated noise values</param>
	/// <returns>
	///		<strong>2D Float Array</strong> that contains all of the generated noise values
	/// </returns>
	public static float[ , ] GenerateRandomNoiseGrid (int width, int height, float minRange, float maxRange) {
		float[ , ] noise = new float[width, height];

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				noise[i, j] = UnityEngine.Random.Range(minRange, maxRange);
			}
		}

		return noise;
	}

	/// <summary>
	///		Get all cardinal positions to the specified position
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>
	///		<strong>Vector2Int List</strong> that contains all four of the cardinal positions
	/// </returns>
	public static List<Vector2Int> GetCardinalPositions (Vector2Int position) {
		return new List<Vector2Int>( ) {
			position + Vector2Int.up,
			position + Vector2Int.right,
			position + Vector2Int.down,
			position + Vector2Int.left
		};
	}

	/// <summary>
	///		Rotate a 2D position around a pivot point by a certain number of degrees
	/// </summary>
	/// <param name="position">The position to rotate around the pivot point</param>
	/// <param name="pivot">The pivot point to rotate around</param>
	/// <param name="angle">The angle in degrees to rotate the position around the pivot point by</param>
	/// <returns>
	///		<strong>Vector2Int</strong> that is the 2D position after it has been rotated by the specified number of degrees around the pivot point
	///	</returns>
	public static Vector2Int RotatePositionAroundPivot2D (Vector2 position, Vector2 pivot, float angle) {
		Vector3 distance = position - pivot;
		Vector3 rotatedPosition = Quaternion.Euler(0, 0, angle) * distance + (Vector3) pivot;
		return (Vector2Int) Vect3Round(rotatedPosition);
	}
}
