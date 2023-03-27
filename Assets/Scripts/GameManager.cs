using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
	GAME, GAME_BREAKTHROUGH, GAME_PAUSED, GAME_GAMEOVER
}

public enum PointsType {
	DESTROYED_BLOCK, DROPPED_BLOCK, BREAKTHROUGH, DESTROYED_MINO, FAST_DROP
}

public class GameManager : MonoBehaviour {
	[Header("Point Values")]
	[SerializeField, Min(0)] private int pointsPerDestroyedBlock = 6;
	[SerializeField, Min(0)] private int pointsPerDroppedBlock = 12;
	[SerializeField, Min(0)] private int pointsPerBreakthrough = 600;
	[SerializeField, Min(0)] private int pointsPerDestroyedMino = 60;
	[SerializeField, Min(0)] private int pointsPerFastDrop = 2;
	[Header("Properties")]
	[SerializeField] private GameState _gameState;
	[SerializeField, Min(0f)] private int _boardPoints;
	[SerializeField, Min(0f)] private int _totalPoints;
	[SerializeField, Min(0f)] private float _percentCleared;
	[SerializeField, Min(0)] private int boardsGenerated;
	[SerializeField] private PlayerControlledBlockGroup _activeMino;
	[Space]
	[SerializeField, Min(0f)] private float _fallTime;
	[SerializeField, Min(0f)] private float _fallTimeAccelerated;
	[SerializeField, Min(0f)] private float _moveTime;
	[SerializeField, Min(0f)] private float _moveTimeAccelerated;
	[SerializeField, Min(0f)] private float _rotateTime;
	[SerializeField, Min(0f)] private float _rotateTimeAccelerated;
	[SerializeField, Min(0f)] private float _placeTime;
	[SerializeField, Min(0f)] private float _blockScale;
	[SerializeField, Min(0f)] private float _animationSpeed;
	[SerializeField, Min(1)] private int boomBlockGuarantee;
	[SerializeField, Min(0f)] private float boomBlockInitialChance;
	[SerializeField, Min(0)] private int _boomBlockDrought;

	#region Properties
	public GameState GameState => _gameState;
	public int BoardPoints {
		get => _boardPoints;
		private set {
			_boardPoints = value;

			/// TODO: Update UI
		}
	}
	public int TotalPoints {
		get => _totalPoints;
		private set {
			_totalPoints = value;

			// TODO: Update UI
		}
	}
	public float PercentCleared {
		get => _percentCleared;
		private set {
			_percentCleared = value;

			// TODO: Update UI
		}
	}
	public PlayerControlledBlockGroup ActiveMino { get => _activeMino; set => _activeMino = value; }

	public float FallTime { get => _fallTime; private set => _fallTime = value; }
	public float FallTimeAccelerated => _fallTimeAccelerated;
	public float MoveTime => _moveTime;
	public float MoveTimeAccelerated => _moveTimeAccelerated;
	public float RotateTime => _rotateTime;
	public float RotateTimeAccelerated => _rotateTimeAccelerated;
	public float PlaceTime => _placeTime;
	public float BlockScale => _blockScale;
	public float AnimationSpeed => _animationSpeed;
	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }
	public float BoomBlockSpawnChance => ((float) BoomBlockDrought / boomBlockGuarantee) * (1 - boomBlockInitialChance) + boomBlockInitialChance;
	#endregion

	/// <summary>
	/// Add board points
	/// </summary>
	/// <param name="pointsType">The type of points to add to the board points</param>
	public void AddBoardPoints (PointsType pointsType) {
		switch (pointsType) {
			case PointsType.DESTROYED_BLOCK:
				BoardPoints += pointsPerDestroyedBlock;
				break;
			case PointsType.DROPPED_BLOCK:
				BoardPoints += pointsPerDroppedBlock;
				break;
			case PointsType.BREAKTHROUGH:
				BoardPoints += pointsPerBreakthrough;
				break;
			case PointsType.DESTROYED_MINO:
				BoardPoints += pointsPerDestroyedMino;
				break;
			case PointsType.FAST_DROP:
				BoardPoints += pointsPerFastDrop;
				break;
		}
	}
}
