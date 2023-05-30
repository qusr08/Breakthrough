using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
	GAME, PAUSED, GAMEOVER
}

public class GameManager : MonoBehaviour {
	public static int BOARD_WIDTH = 16;
	public static int BOARD_HEIGHT = 28;
	public static float HAZARD_TIME = 25f;
	public static float MINO_SPEED = 1f;
	public static float BOOM_BLOCK_INITIAL_CHANCE = 0.4f;

	[Header("Components")]
	[SerializeField] private ParticleManager _particleManager;
	[SerializeField] private BoardTextManager boardTextManager;
	[Header("Point Values")]
	[SerializeField, Min(0)] private int _pointsPerDestroyedBlock = 6;
	[SerializeField, Min(0)] private int _pointsPerDroppedBlock = 12;
	[SerializeField, Min(0)] private int _pointsPerBreakthrough = 600;
	[SerializeField, Min(0)] private int _pointsPerDestroyedMino = 60;
	[SerializeField, Min(0)] private int _pointsPerFastDrop = 2;
	[Header("Game Settings")]
	[SerializeField] private Vector2 wallHealthRange;
	[SerializeField] private int wallHeight;
	[SerializeField] private float hazardTime;
	[Header("Properties")]
	[SerializeField, Min(0f)] private int _boardPoints;
	[SerializeField, Min(0f)] private int _totalPoints;
	[SerializeField, Min(0f)] private float _percentCleared;
	[SerializeField, Min(0)] private int boardsGenerated;
	[SerializeField] private PlayerControlledBlockGroup _activeMino;
	[SerializeField] private GameState _gameState;
	[Space]
	[SerializeField, Min(0f)] private float _fallTime;
	[SerializeField, Min(0f)] private float _moveTime;
	[SerializeField, Min(0f)] private float _rotateTime;
	[SerializeField, Min(0f)] private float _placeTime;
	[SerializeField, Min(0f)] private float _blockScale;
	[SerializeField, Range(-1, 1)] private int _rotateDirection;
	[SerializeField, Min(0f)] private float _boomBlockAnimationSpeed;
	[SerializeField, Min(0f)] private float _blockGroupAnimationSpeed;
	[SerializeField, Min(1)] private int boomBlockGuarantee;

	private int _boomBlockDrought;

	#region Properties
	public int BoardPoints { get => _boardPoints; set => boardTextManager.BoardPointsBoardText.Value = _boardPoints = value; }
	public int TotalPoints { get => _totalPoints; set => boardTextManager.TotalPointsBoardText.Value = _totalPoints = value; }
	public float PercentCleared { get => _percentCleared; set => boardTextManager.PercentageClearBoardText.Value = _percentCleared = value; }
	public PlayerControlledBlockGroup ActiveMino { get => _activeMino; set => _activeMino = value; }

	public GameState GameState {
		get => _gameState;
		set {
			_gameState = value;

			Debug.Log("Set Game State: " + value.ToString( ));

			switch (_gameState) {
				case GameState.GAME:
					break;
				case GameState.PAUSED:
					break;
				case GameState.GAMEOVER:
					break;
			}
		}
	}

	public ParticleManager ParticleManager => _particleManager;

	public int PointsPerDestroyedBlock => _pointsPerDestroyedBlock;
	public int PointsPerDroppedBlock => _pointsPerDroppedBlock;
	public int PointsPerBreakthrough => _pointsPerBreakthrough;
	public int PointsPerDestroyedMino => _pointsPerDestroyedMino;
	public int PointsPerFastDrop => _pointsPerFastDrop;

	public Vector2 WallHealthRange { get => wallHealthRange; private set => wallHealthRange = value; }
	public int WallHeight { get => wallHeight; private set => wallHeight = value; }
	public float HazardTime { get => hazardTime; set => hazardTime = value; }

	public float FallTime { get => _fallTime; private set => _fallTime = value; }
	public float FallTimeAccelerated => FallTime / 20f;
	public float MoveTime => _moveTime;
	public float MoveTimeAccelerated => MoveTime / 2f;
	public float RotateTime => _rotateTime;
	public float RotateTimeAccelerated => RotateTime / 2f;
	public int RotateDirection => _rotateDirection;
	public float PlaceTime => _placeTime;
	public float BlockScale => _blockScale;
	public float BoomBlockAnimationSpeed => _boomBlockAnimationSpeed;
	public float BlockGroupAnimationSpeed => _blockGroupAnimationSpeed;
	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }
	public float BoomBlockSpawnChance => ((float) BoomBlockDrought / boomBlockGuarantee) * (1 - BOOM_BLOCK_INITIAL_CHANCE) + BOOM_BLOCK_INITIAL_CHANCE;
	#endregion
}
