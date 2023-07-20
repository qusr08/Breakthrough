using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum GameState {
	GAME, PAUSED, GAMEOVER
}

public class GameManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ParticleManager _particleManager;
	[SerializeField] private BoardTextManager boardTextManager;
	[SerializeField] private GameSettingsScriptableObject _gameSettings;
	[Header("Point Values")]
	[SerializeField, Min(0)] private int _pointsPerDestroyedBlock = 6;
	[SerializeField, Min(0)] private int _pointsPerDroppedBlock = 12;
	[SerializeField, Min(0)] private int _pointsPerBreakthrough = 600;
	[SerializeField, Min(0)] private int _pointsPerDestroyedMino = 60;
	[SerializeField, Min(0)] private int _pointsPerFastDrop = 2;
	[Header("Properties")]
	[SerializeField, Min(0f)] private int _boardPoints;
	[SerializeField, Min(0f)] private int _totalPoints;
	[SerializeField, Min(0f)] private float _percentCleared;
	[SerializeField, Min(0)] private int _breakthroughs;
	[Space]
	[SerializeField] private PlayerControlledBlockGroup _activeMino;
	[SerializeField] private GameState _gameState;
	[Space]
	[SerializeField, Min(0f)] private float defaultMinoFallTime;
	[SerializeField, Min(0f)] private float _fastMinoFallTime;
	[SerializeField, Min(0f)] private float _minoMoveTime;
	[SerializeField, Min(0f)] private float _minoRotateTime;
	[SerializeField, Min(0f)] private float _minoPlaceTime;
	[SerializeField, Min(0f)] private float _blockScale;
	[SerializeField, Range(-1, 1)] private int _rotateDirection;
	[SerializeField, Min(0f)] private float _boardAnimationSpeed;
	[SerializeField, Min(1)] private int boomBlockGuarantee;

	private int _boomBlockDrought;

	#region Properties
	public ParticleManager ParticleManager => _particleManager;
	public GameSettingsScriptableObject GameSettings => _gameSettings;

	public int BoardPoints { get => _boardPoints; set => boardTextManager.BoardPointsBoardText.Value = _boardPoints = value; }
	public int TotalPoints { get => _totalPoints; set => boardTextManager.TotalPointsBoardText.Value = _totalPoints = value; }
	public float PercentCleared { get => _percentCleared; set => boardTextManager.PercentageClearBoardText.Value = _percentCleared = value; }
	public int Breakthroughs { get => _breakthroughs; set => boardTextManager.BreakthroughsBoardText.Value = _breakthroughs = value; }

	public PlayerControlledBlockGroup ActiveMino { get => _activeMino; set => _activeMino = value; }
	public GameState GameState {
		get => _gameState;
		set {
			_gameState = value;

			Debug.Log("Set Game State: " + value.ToString( ));

			switch (value) {
				case GameState.GAME:
					break;
				case GameState.PAUSED:
					break;
				case GameState.GAMEOVER:
					break;
			}
		}
	}

	public int PointsPerDestroyedBlock => _pointsPerDestroyedBlock;
	public int PointsPerDroppedBlock => _pointsPerDroppedBlock;
	public int PointsPerBreakthrough => _pointsPerBreakthrough;
	public int PointsPerDestroyedMino => _pointsPerDestroyedMino;
	public int PointsPerFastDrop => _pointsPerFastDrop;

	public float MinoFallTime => defaultMinoFallTime / GameSettings.MinoSpeedMultiplier;
	public float FastMinoFallTime => _fastMinoFallTime;
	public float MinoMoveTime => _minoMoveTime;
	public float FastMinoMoveTime => MinoMoveTime / 2f;
	public float MinoRotateTime => _minoRotateTime;
	public float FastMinoRotateTime => MinoRotateTime / 2f;
	public float MinoPlaceTime => _minoPlaceTime;
	public int RotateDirection => _rotateDirection;
	public float BlockScale => _blockScale;
	public float BoardAnimationSpeed => _boardAnimationSpeed;
	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }
	public float BoomBlockSpawnChance => ((float) BoomBlockDrought / boomBlockGuarantee) * (1 - GameSettings.BoomBlockChance) + GameSettings.BoomBlockChance;
	#endregion

	public void IncrementDifficulty ( ) {

	}
}
