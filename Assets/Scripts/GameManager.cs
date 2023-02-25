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
	[SerializeField] public int PointsPerDestroyedBlock = 6;
	[SerializeField] public int PointsPerDroppedBlock = 12;
	[SerializeField] public int PointsPerBreakthrough = 600;
	[SerializeField] public int PointsPerDestroyedMino = 60;
	[SerializeField] public int PointsPerFastDrop = 2;
	[SerializeField] private List<PointsEvent> pointsEvents;

	public int BoardPoints {
		get {
			return int.Parse(BoardPointsUILabel.Value.text);
		}

		set {
			BoardPointsUILabel.Value.text = value.ToString( );
		}
	}
	public int TotalPoints {
		get {
			return int.Parse(TotalPointsUILabel.Value.text);
		}

		set {
			TotalPointsUILabel.Value.text = value.ToString( );
		}
	}
	public int Breakthroughs {
		get {
			return int.Parse(BreakthroughsUILabel.Value.text);
		}

		set {
			BreakthroughsUILabel.Value.text = value.ToString();
		}
	}

	/*public void AddBoardPoints (int points, PointsEffectType pointsEffectType = PointsEffectType.NONE, Block pointsEffectBlock = null) {
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
	}*/
}