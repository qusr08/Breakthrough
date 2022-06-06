using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
	// https://forum.unity.com/threads/quaternion-smoothdamp.793533/
	public static Vector3 SmoothDampEuler (Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime) {
		return new Vector3(
			Mathf.SmoothDampAngle(current.x, target.x, ref currentVelocity.x, smoothTime),
			Mathf.SmoothDampAngle(current.y, target.y, ref currentVelocity.y, smoothTime),
			Mathf.SmoothDampAngle(current.z, target.z, ref currentVelocity.z, smoothTime)
		);
	}

	public static Vector3 RotatePositionAroundPivot (Vector3 position, Vector3 pivot, float degAngle) {
		return (Quaternion.Euler(0, 0, degAngle) * (position - pivot) + pivot);
	}

	public static bool CompareVectors (Vector3 vector1, Vector3 vector2) {
		return ((vector1 - vector2).magnitude < Constants.CLOSE_ENOUGH);
	}

	public static bool CompareAngleVectors (Vector3 vector1, Vector3 vector2) {
		vector1 = Vect3Round(Vect3Abs(vector1));
		vector2 = Vect3Round(Vect3Abs(vector2));

		return (
			vector1.x % 180 == vector2.x % 180 &&
			vector1.y % 180 == vector2.y % 180 &&
			vector1.z % 180 == vector2.z % 180
		);
	}

	public static Vector3 Vect3Abs (Vector3 vector) {
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}

	public static Vector3Int Vect3Round (Vector3 vector) {
		return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public static bool IsEven (int value) {
		return (value % 2 == 0);
	}

	public static List<Vector3> GetCardinalPositions (Vector3 position) {
		return new List<Vector3>( ) {
			position + Vector3.right,
			position + Vector3.down,
			position + Vector3.left,
			position + Vector3.up
		};
	}

	public static float[ , ] RandomPerlinNoiseGrid (int width, int height, float step, float scale) {
		float[ , ] noise = new float[width, height];
		float seed = Random.Range(-100f, 100f);

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				float x = seed + (i * step);
				float y = seed + (j * step);

				noise[i, j] = Mathf.PerlinNoise(x, y) * scale;
			}
		}

		return noise;
	}
}
