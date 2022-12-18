using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PointsEffectType {
	NONE, TEXT, PARTICLE
}

public class GameManager : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private BoardUILabel totalPointsUILabel;
	[SerializeField] private BoardUILabel boardPointsUILabel;
	[SerializeField] private BoardUILabel breakthroughsUILabel;
	[Header("Properties")]
	[SerializeField] private int _boardPoints;
	[SerializeField] private int _totalPoints;
	[SerializeField] private int _breakthroughs;
	[Space]
	[SerializeField] public int PointsPerDestroyedBlock = 6;
	[SerializeField] public int PointsPerDroppedBlock = 12;
	[SerializeField] public int PointsPerBreakthrough = 600;
	[SerializeField] public int PointsPerDestroyedMino = 60;
	[SerializeField] public int PointsPerFastDrop = 2;

	public int BoardPoints {
		get {
			return _boardPoints;
		}

		set {
			_boardPoints = value;

			boardPointsUILabel.Value.text = _boardPoints.ToString( );
		}
	}
	public int TotalPoints {
		get {
			return _totalPoints;
		}

		set {
			_totalPoints = value;

			totalPointsUILabel.Value.text = _totalPoints.ToString( );
		}
	}
	public int Breakthroughs {
		get {
			return _breakthroughs;
		}

		set {
			_breakthroughs = value;

			breakthroughsUILabel.Value.text = _breakthroughs.ToString( );
		}
	}

	public void AddBoardPoints (int points, PointsEffectType pointsEffectType = PointsEffectType.NONE, BlockColor blockColor = BlockColor.DARK_COAL) {
		BoardPoints += points;

		switch (pointsEffectType) {
			case PointsEffectType.NONE:
				break;
			case PointsEffectType.TEXT:
				break;
			case PointsEffectType.PARTICLE:
				break;
		}

		boardPointsUILabel.TriggerTextAnimation( );
	}
}