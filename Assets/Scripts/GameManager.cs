using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PointsEffectType {
	NONE, TEXT, PARTICLE
}

public class GameManager : MonoBehaviour {
	[Header("Prefabs")]
	[SerializeField] private GameObject boardUIParticlePrefab;
	[Header("Scene GameObjects")]
	[SerializeField] public BoardUILabel TotalPointsUILabel;
	[SerializeField] public BoardUILabel BoardPointsUILabel;
	[SerializeField] public BoardUILabel BreakthroughsUILabel;
	[Header("Properties")]
	[SerializeField] private int _boardPoints;
	[SerializeField] private int _totalPoints;
	[SerializeField] private int _breakthroughs;
	[Space]
	[SerializeField] public int PointsPerDestroyedBlock = 6;
	[SerializeField] public int PointsPerDroppedBlock = 12;
	[SerializeField] public int PointsPerBreakthrough = 600;
	[SerializeField] public int PointsPerDestroyedMino = 60;
	// [SerializeField] public int PointsPerFastDrop = 2;

	public int BoardPoints {
		get {
			return _boardPoints;
		}

		set {
			_boardPoints = value;

			BoardPointsUILabel.Value.text = _boardPoints.ToString( );
		}
	}
	public int TotalPoints {
		get {
			return _totalPoints;
		}

		set {
			_totalPoints = value;

			TotalPointsUILabel.Value.text = _totalPoints.ToString( );
		}
	}
	public int Breakthroughs {
		get {
			return _breakthroughs;
		}

		set {
			_breakthroughs = value;

			BreakthroughsUILabel.Value.text = _breakthroughs.ToString( );
		}
	}

	public void AddBoardPoints (int points, PointsEffectType pointsEffectType = PointsEffectType.NONE, Block pointsEffectBlock = null) {
		BoardPoints += points;

		BoardPointsUILabel.TriggerTextAnimation( );

		// If a block has not been specified, do not do any effects because there is not starting position
		if (pointsEffectBlock == null) {
			return;
		}

		switch (pointsEffectType) {
			case PointsEffectType.NONE:
				break;
			case PointsEffectType.TEXT:
				break;
			case PointsEffectType.PARTICLE:
				break;
		}

		// BoardUIParticle boardUIParticle = Instantiate(boardUIParticlePrefab, pointsEffectBlock.Position, Quaternion.identity).GetComponent<BoardUIParticle>( );
		// boardUIParticle.ToPosition = BoardPointsUILabel.transform.position;
		// boardUIParticle.FromPosition = pointsEffectBlock.Position;
		// boardUIParticle.SetSprite(pointsEffectBlock.Sprite);
		// boardUIParticle.IsInitialized = true;
	}
}