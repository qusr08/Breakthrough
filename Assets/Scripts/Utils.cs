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

	public static Vector2Int Vect2Round (Vector2 vector) {
		return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	public static List<Transform> GetImmediateChildren (Transform transform) {
		List<Transform> children = new List<Transform>( );

		foreach (Transform child in transform) {
			children.Add(child);
		}

		return children;
	}
}
