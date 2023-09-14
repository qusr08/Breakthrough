using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MinoData {
	public static MinoBlockDataDictionary MinoBlockData = new MinoBlockDataDictionary( ) {
		{ MinoType.X, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(1.5f, 0.5f),
			new Vector2(0.5f, 1.5f)
		}},
		{ MinoType.C, new List<Vector2>() {
			new Vector2(0.5f, -0.5f),
			new Vector2(1.5f, -0.5f),
			new Vector2(1.5f, 0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(-1.5f, -0.5f),
			new Vector2(-1.5f, 0.5f)
		}},
		{ MinoType.D, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(1.5f, 0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(-1.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(-0.5f, -0.5f)
		}},
		{ MinoType.F, new List<Vector2>() {
			new Vector2(-0.5f, 0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(1.5f, 0.5f),
			new Vector2(-0.5f, -1.5f)
		}},
		{ MinoType.O, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(0.5f, 1.5f),
			new Vector2(-0.5f, 1.5f)
		}},
		{ MinoType.Z_L, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(1.5f, -0.5f),
			new Vector2(-1.5f, 0.5f)
		}},
		{ MinoType.Z_R, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(1.5f, 0.5f),
			new Vector2(-1.5f, -0.5f)
		}},
		{ MinoType.S_L, new List<Vector2>() {
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f),
			new Vector2(0.5f, 1.5f),
			new Vector2(0.5f, -1.5f),
			new Vector2(-0.5f, 1.5f),
			new Vector2(1.5f, -1.5f),
		}},
		{ MinoType.S_R, new List<Vector2>() {
			new Vector2(-0.5f, 0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 1.5f),
			new Vector2(-0.5f, -1.5f),
			new Vector2(0.5f, 1.5f),
			new Vector2(-1.5f, -1.5f)
		}},
	};
}
