using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState {
	GAME, PAUSED, GAMEOVER
}

public class GameManager : MonoBehaviour {
	public const int UI_GRID_SIZE = 120;

	[Header("Components")]
	[SerializeField] private BoardTextManager boardTextManager;
	[SerializeField] private GameSettings _gameSettings;
	[SerializeField] private Board board;
	[SerializeField] private CameraManager cameraManager;
	[SerializeField] private HazardBar hazardBar;
	[SerializeField] private TextMeshProUGUI totalPointsUIText;
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
	[SerializeField, Min(0f)] private float _minMinoFallTime;
	[SerializeField, Min(0f)] private float _minoMoveTime;
	[SerializeField, Min(0f)] private float _minoRotateTime;
	[SerializeField, Min(0f)] private float _minoPlaceTime;
	[Space]
	[SerializeField, Min(0f)] private float defaultHazardFallTime;
	[SerializeField, Min(0f)] private float minHazardFallTime;
	[Space]
	[SerializeField, Range(-1, 1)] private int _rotateDirection;
	[SerializeField, Min(0f)] private float _boardAnimationSpeed;
	[Header("Temporary")]
	[SerializeField] private GameObject pausedText;

	private int _boomBlockDrought;
	private float _minoFallTime;
	private float _hazardFallTime;
	private float _wallStrength;
	private int _wallHeight;
	private float _boomBlockChance;

	#region Properties
	public GameSettings GameSettings => _gameSettings;

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
					pausedText.SetActive(false);
					break;
				case GameState.PAUSED:
					pausedText.SetActive(true);
					break;
				case GameState.GAMEOVER:
					break;
			}
		}
	}

	public float MinoFallTime { get => _minoFallTime; private set => _minoFallTime = value; }
	public float MinMinoFallTime => _minMinoFallTime;

	public float HazardFallTime { get => _hazardFallTime; private set => _hazardFallTime = value; }

	public float WallStrength { get => _wallStrength; private set => _wallStrength = value; }
	public int WallHeight { get => _wallHeight; private set => _wallHeight = value; }

	public float BoomBlockChance { get => _boomBlockChance; set => _boomBlockChance = value; }

	public float MinoMoveTime => _minoMoveTime;
	public float FastMinoMoveTime => MinoMoveTime / 2f;
	public float MinoRotateTime => _minoRotateTime;
	public float FastMinoRotateTime => MinoRotateTime / 2f;
	public float MinoPlaceTime => _minoPlaceTime;

	public int RotateDirection => _rotateDirection;
	public float BoardAnimationSpeed => Mathf.Min(_boardAnimationSpeed, MinMinoFallTime);
	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }
	public float BoomBlockSpawnChance => ((float) BoomBlockDrought / Constants.BOOM_BLOCK_GUAR) * (1 - BoomBlockChance) + BoomBlockChance;
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		boardTextManager = FindObjectOfType<BoardTextManager>( );
		cameraManager = FindObjectOfType<CameraManager>( );
	}

	private void Awake ( ) {
		OnValidate( );

		GameState = GameState.GAME;
	}
	#endregion

	public void UpdateDifficulty ( ) {
		// Calculate the mino fall time for the specified settings
		float minoScaleFactor = (defaultMinoFallTime / GameSettings.MinoSpeedMultiplier) - MinMinoFallTime;
		float minoSlope = -GameSettings.MinoSpeedMultiplier * Constants.DIFF_VALUE * Breakthroughs;
		MinoFallTime = (minoScaleFactor * Mathf.Exp(minoSlope)) + MinMinoFallTime;

		// Calculate the hazard fall time for the specified settings
		float hazardScaleFactor = (defaultHazardFallTime / GameSettings.HazardSpeedMultiplier) - minHazardFallTime;
		float hazardSlope = -GameSettings.HazardSpeedMultiplier * Constants.DIFF_VALUE * Breakthroughs;
		HazardFallTime = (hazardScaleFactor * Mathf.Exp(hazardSlope)) + minHazardFallTime;

		// Calculate the wall strength for the specified settings
		// * This assumes that the wall health values are between 0 and 3
		float strengthSlope = -GameSettings.WallStrengthMultiplier * Constants.DIFF_VALUE * Breakthroughs;
		WallStrength = (-2f * Mathf.Exp(strengthSlope)) + 3f;

		// Calculate the wall height for the specified settings
		float availableBoardArea = GameSettings.BoardHeight - board.BreakthroughBoardArea.Height - board.HazardBoardArea.Height;
		float heightMax = availableBoardArea / 2f;
		float heightMin = availableBoardArea / 8f;
		float heightScaleFactor = heightMin - heightMax;
		float heightSlope = -Constants.DIFF_VALUE * Breakthroughs;
		WallHeight = Mathf.RoundToInt((heightScaleFactor * Mathf.Exp(heightSlope)) + heightMax);

		// Calculate the boom block spawn chance for the specified settings
		float chanceSlope = -Constants.DIFF_VALUE * Breakthroughs;
		float chanceScaleFactor = GameSettings.BoomBlockChance - 1;
		BoomBlockChance = (chanceScaleFactor * Mathf.Exp(chanceSlope)) + 1;
	}

	public void OnPause (InputValue inputValue) {
		if (GameState == GameState.PAUSED) {
			GameState = GameState.GAME;
		} else {
			GameState = GameState.PAUSED;
		}
	}
}
