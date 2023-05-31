using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Utils {
	public const float CLOSE_ENOUGH = 0.01f;

	/// <summary>
	/// SmoothDamp function for rotation vectors
	/// https://forum.unity.com/threads/quaternion-smoothdamp.793533/
	/// </summary>
	/// <param name="current">The current rotation vector</param>
	/// <param name="target">The target rotation vector</param>
	/// <param name="currentVelocity">The velocity of the function as it rotates</param>
	/// <param name="smoothTime">The time it takes to smooth between the rotations</param>
	/// <returns>The current rotation vector as it smoothly transitions to the target rotation </returns>
	public static Vector3 SmoothDampEuler (Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime) {
		return new Vector3(
			Mathf.SmoothDampAngle(current.x, target.x, ref currentVelocity.x, smoothTime),
			Mathf.SmoothDampAngle(current.y, target.y, ref currentVelocity.y, smoothTime),
			Mathf.SmoothDampAngle(current.z, target.z, ref currentVelocity.z, smoothTime)
		);
	}

	/// <summary>
	/// Rotate a position vector around a pivot
	/// </summary>
	/// <param name="position">The position vector to rotate</param>
	/// <param name="pivot">The position vector of the pivot to rotate around</param>
	/// <param name="degAngle">The angle in degrees to rotate the position vector</param>
	/// <returns>The rotated position vector</returns>
	public static Vector3 RotatePositionAroundPivot (Vector3 position, Vector3 pivot, float degAngle) {
		return (Quaternion.Euler(0, 0, degAngle) * (position - pivot) + pivot);
	}

	/// <summary>
	/// Rotate a position vector around a pivot
	/// </summary>
	/// <param name="position">The position vector to rotate</param>
	/// <param name="pivot">The position vector of the pivot to rotate around</param>
	/// <param name="degAngle">The angle in degrees to rotate the position vector</param>
	/// <returns>The rotated position vector</returns>
	public static Vector2 RotatePositionAroundPivot (Vector2 position, Vector2 pivot, float degAngle) {
		return (Quaternion.Euler(0, 0, degAngle) * (position - pivot) + (Vector3) pivot);
	}

	/// <summary>
	/// Check to see if two position vectors are close enough to each other
	/// THIS DOES NOT CHECK TO EXACTLY EQUAL, JUST TO MAKE SURE THE TWO VECTORS ARE CLOSE
	/// THIS IS USEFUL WHEN DEALING WITH CHECKING POSITIONS IN UNITY
	/// </summary>
	/// <param name="vector1">The first position vector</param>
	/// <param name="vector2">The second position vector</param>
	/// <returns>Whether the two position vectors are close enough to each other</returns>
	public static bool CompareVectors (Vector2 vector1, Vector2 vector2) => CompareVectors((Vector3) vector1, (Vector3) vector2);
	public static bool CompareVectors (Vector3 vector1, Vector3 vector2) {
		return ((vector1 - vector2).magnitude < CLOSE_ENOUGH);
	}

	/// <summary>
	/// Compare two rotation vectors to make sure they are equal
	/// </summary>
	/// <param name="vector1">The first rotation vector</param>
	/// <param name="vector2">The second rotation vector</param>
	/// <returns>Whether or not the two rotation vectors are equal</returns>
	public static bool CompareDegreeAngleVectors (Vector3 vector1, Vector3 vector2) {
		vector1 = Vect3Round(Vect3Abs(vector1));
		vector2 = Vect3Round(Vect3Abs(vector2));

		return (
			vector1.x % 180 == vector2.x % 180 &&
			vector1.y % 180 == vector2.y % 180 &&
			vector1.z % 180 == vector2.z % 180
		);
	}

	/// <summary>
	/// Get the absolute value of each element in a vector
	/// </summary>
	/// <param name="vector">The vector to take the absolute value of</param>
	/// <returns>The modified vector with all of its values greater than 0</returns>
	public static Vector3 Vect3Abs (Vector3 vector) {
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}

	/// <summary>
	/// Round each element in a vector to the nearest integer
	/// </summary>
	/// <param name="vector">The vector to round</param>
	/// <returns>The modified vector with each element an integer</returns>
	public static Vector3Int Vect3Round (Vector3 vector) {
		return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	/// <summary>
	/// Round each element in a vector to the nearest integer
	/// </summary>
	/// <param name="vector">The vector to round</param>
	/// <returns>The modified vector with each element an integer</returns>
	public static Vector2Int Vect2Round (Vector3 vector) {
		return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	/// <summary>
	/// Check to see if a value is even
	/// </summary>
	/// <param name="value">The value to check</param>
	/// <returns>Whether or not the value is even</returns>
	public static bool IsEven (int value) {
		return (value % 2 == 0);
	}

	/// <summary>
	/// Get all cardinal positions around a certain position vector
	/// </summary>
	/// <param name="position">The center position to check the cardinal directions of</param>
	/// <returns>A list of all the cardinal position vectors</returns>
	public static List<Vector2Int> GetCardinalPositions (Vector2Int position) {
		return new List<Vector2Int>( ) {
			position + Vector2Int.right,
			position + Vector2Int.down,
			position + Vector2Int.left,
			position + Vector2Int.up
		};
	}

	/// <summary>
	/// Generate a grid of perlin noise values
	/// </summary>
	/// <param name="width">The width of the grid</param>
	/// <param name="height">The height of the grid</param>
	/// <param name="roughness">How rough the "terrain" is of the generated perlin noise</param>
	/// <param name="scale">How much to scale the values generated</param>
	/// <param name="elevation">How high to increase the perlin noise values</param>
	/// <returns>A 2D array of values generated by the perlin noise</returns>
	public static float[ , ] GeneratePerlinNoiseGrid (int width, int height, float roughness, float scale = 1, float elevation = 0) {
		float[ , ] noise = new float[width, height];
		// Get a random value to offset the perlin noise by to make sure the values are random
		float seed = UnityEngine.Random.Range(-100000f, 100000f);

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				// Get the coordinates of the value in the grid
				float x = seed + (i * roughness);
				float y = seed + (j * roughness);

				// Save the scaled value of the perlin noise to an array
				noise[i, j] = (Mathf.PerlinNoise(x, y) * scale) + elevation;
			}
		}

		return noise;
	}

	/// <summary>
	/// Generate a random grid of values
	/// </summary>
	/// <param name="width">The width of the grid.</param>
	/// <param name="height">The height of the grid.</param>
	/// <param name="minRange">The minimum range of the values.</param>
	/// <param name="maxRange">The maximum range of the values.</param>
	/// <returns>Returns a float array that has all of the random values inside it.</returns>
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
	/// Find the size of an enum.
	/// </summary>
	/// <param name="enumType">The type of the enum to check</param>
	/// <returns>The size of the enum</returns>
	public static int GetEnumSize (Type enumType) {
		return Enum.GetNames(enumType).Length;
	}

	/// <summary>
	/// Parse a hex color string to a color object
	/// </summary>
	/// <param name="hex">The hex string to parse. Make sure to include the '#' in front of the code.</param>
	/// <returns>A color object of the parsed hex string. If there was an error parsing the string, the color white is returned.</returns>
	public static Color GetColorFromHex (string hex) {
		if (ColorUtility.TryParseHtmlString(hex, out Color color)) {
			return color;
		}

		return Color.white;
	}

	/// <summary>
	/// Get a random array element with an excluded index
	/// </summary>
	/// <typeparam name="T">The type of the values contained within the array</typeparam>
	/// <param name="array">The array to get the value of</param>
	/// <param name="excludedIndex">The index to exclude from the array</param>
	/// <returns>A random element from the array that is not at the excluded index</returns>
	public static T GetRandomArrayElementExcluded<T> (List<T> array, int excludedIndex) {
		return array[GetRandomArrayIndexExcluded(array, excludedIndex)];
	}

	/// <summary>
	/// Get a random array index with an excluded index
	/// </summary>
	/// <typeparam name="T">The type of the values contained within the array</typeparam>
	/// <param name="array">The array to get the index from</param>
	/// <param name="excludedIndex">The index to exclude</param>
	/// <returns>A random index that is not the excluded index</returns>
	public static int GetRandomArrayIndexExcluded<T> (List<T> array, int excludedIndex) {
		List<int> copy = new List<int>( );
		for (int i = 0; i < array.Count; i++) {
			if (i == excludedIndex) {
				continue;
			}

			copy.Add(i);
		}

		return UnityEngine.Random.Range(0, copy.Count);
	}
}
