using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] public BoardUILabel TotalPointsUILabel;
	[SerializeField] public BoardUILabel BoardPointsUILabel;
	[SerializeField] public BoardUILabel PercentageClearedUILabel;
	[Header("Points Values")]
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each destroyed block.")] public int PointsPerDestroyedBlock = 6;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each block dropped into the breakthrough area.")] public int PointsPerDroppedBlock = 12;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each breakthrough.")] public int PointsPerBreakthrough = 600;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each fully destroyed mino.")] public int PointsPerDestroyedMino = 60;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each time they fast drop the mino.")] public int PointsPerFastDrop = 2;
	[Header("Game Values")]
	[SerializeField, Min(0f), Tooltip("The default speed that block groups fall.")] public float FallTime = 1f;
	[SerializeField, Min(0f), Tooltip("The sensitivity of the player movement.")] public float MoveTime = 0.15f;
	[SerializeField, Min(0f), Tooltip("The sensitivity of the player rotation.")] public float RotateTime = 0.25f;
	[SerializeField, Min(0f), Tooltip("How long a block group that is controlled by the player needs to be stationary in order for it to be placed.")] public float PlaceTime = 0.75f;
	[SerializeField, Range(-1, 1), Tooltip("The direction that the player controlled block group should be rotated. -1 is clockwise, 1 is counter-clockwise")] public int RotateDirection = -1;
	[SerializeField, Range(0f, 1f), Tooltip("How much blocks should be scaled down when they are created. This gives a gap between each of them and gives a better idea of where each grid space is.")] public float BlockScale = 0.95f;
	[SerializeField, Range(0f, 1f), Tooltip("How fast block group transforms should be animated.")] public float BlockAnimationSpeed = 0.04f;

	public float FallTimeAccelerated {
		get => FallTime / 20f;
	}
	public float MoveTimeAccelerated {
		get => MoveTime / 2f;
	}

	public int BoardPoints {
		get => int.Parse(BoardPointsUILabel.Value.text);
		set => BoardPointsUILabel.Value.text = value.ToString( );
	}
	public int TotalPoints {
		get => int.Parse(TotalPointsUILabel.Value.text);
		set => TotalPointsUILabel.Value.text = value.ToString( );
	}
	public float PercentageCleared {
		get {
			int stringLength = PercentageClearedUILabel.Value.text.Length;
			return float.Parse(PercentageClearedUILabel.Value.text.Substring(0, stringLength - 1));
		}
		set {
			PercentageClearedUILabel.Value.text = $"{(value * 100):0.##}%";
		}
	}
}