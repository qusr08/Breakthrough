using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
	GAME, PAUSE, GAMEOVER
}

public class GameManager : MonoBehaviour {
	[SerializeField, Range(0f, 1f), Tooltip("The percentage of a Mino's weighted percentage value that is redistributed to the rest of the percentages. The higher this number, the less likely it is to have multiple Minos appear in a row.")] private float minoWeightPercentage;
	[SerializeField, Min(0), Tooltip("The number of Minos that can spawn without a boom block before one is guaranteed to spawn with a boom block.")] private int boomBlockGuarantee;
	[SerializeField, Min(0f), Tooltip("The default time that it takes Minos to fall on the board.")] private float defaultMinoFallTime;
	[SerializeField, Min(0f), Tooltip("The minimum time that it takes Minos to fall on the board.")] private float _minMinoFallTime;
	[SerializeField, Min(0f), Tooltip("The time it takes for Minos to move left and right.")] private float _minoMoveTime;
	[SerializeField, Min(0f), Tooltip("The time it takes for Minos to rotate.")] private float _minoRotateTime;
	[SerializeField, Min(0f), Tooltip("The time it takes for Minos to be placed on the board when they hit stationary blocks.")] private float _minoPlaceTime;
	[SerializeField, Min(0f), Tooltip("The default time that it takes for the hazard board area to fall.")] private float defaultHazardFallTime;
	[SerializeField, Min(0f), Tooltip("The minimum time that it takes for the hazard board area to fall.")] private float minHazardFallTime;
	[SerializeField, Range(-1, 1), Tooltip("The direction that Minos rotate. -1 is counter-clockwise, and 1 is clockwise.")] private int _rotateDirection;
	[SerializeField, Min(0), Tooltip("The current board points that the player has scored.")] private int _boardPoints;
	[SerializeField, Min(0), Tooltip("The current total points that the player has scored.")] private int _totalPoints;
	[SerializeField, Min(0f), Tooltip("The percentage of the board that is cleared during gameplay.")] private float _percentageCleared;
	[SerializeField, Min(0), Tooltip("The number of Breakthroughs that the player has gotten.")] private int _breakthroughs;
	[SerializeField, Tooltip("The current active Mino that the player is controlling on the board.")] private BlockGroup _activeMino;
	[SerializeField, Tooltip("A reference to the game board.")] private Board _board;
	[SerializeField, Tooltip("The current game state of the game.")] private GameState _gameState;
	[SerializeField, Range(0f, 1f), Tooltip("The current difficulty scaling value. The lower this value is, the slower the game scales in difficulty.")] private float difficultyValue;

	private int _boomBlockDrought;
	private float _minoFallTime;
	private float _hazardFallTime;
	private float _wallStrength;
	private int _wallHeight;
	private float boomBlockChance;

	#region Properties
	/// <summary>
	///		A reference to the board
	/// </summary>
	public Board Board { get => _board; private set => _board = value; }

	/// <summary>
	///		The number of board points the player currently has
	/// </summary>
	public int BoardPoints { get => _boardPoints; set => _boardPoints = value; }

	/// <summary>
	///		The number of total points the player currently has
	/// </summary>
	public int TotalPoints { get => _totalPoints; set => _totalPoints = value; }

	/// <summary>
	///		The percentage of the board that is currently empty
	/// </summary>
	public float PercentageCleared { get => _percentageCleared; set => _percentageCleared = value; }

	/// <summary>
	///		The number of breakthroughs the player currently has
	/// </summary>
	public int Breakthroughs { get => _breakthroughs; set => _breakthroughs = value; }

	/// <summary>
	///		The time that it takes for the hazard board area to fall one block
	/// </summary>
	public float HazardFallTime { get => _hazardFallTime; private set => _hazardFallTime = value; }

	/// <summary>
	///		The strength of the wall blocks when they are generated at the start of a board
	/// </summary>
	public float WallStrength { get => _wallStrength; private set => _wallStrength = value; }

	/// <summary>
	///		The height of the wall when it is generated at the start of a board
	/// </summary>
	public int WallHeight { get => _wallHeight; private set => _wallHeight = value; }

	/// <summary>
	///		The number of seconds it takes for a mino to fall down one block
	/// </summary>
	public float MinoFallTime { get => _minoFallTime; private set => _minoFallTime = value; }

	/// <summary>
	///		The minimum time it takes for the mino to fall down one block. As MinoFallTime scales, this acts as a limit to make sure minos do not fall too fast. This value is also used for the speed at which block groups fall when they are being updated
	/// </summary>
	public float MinMinoFallTime => _minMinoFallTime;

	/// <summary>
	///		The number of seconds it takes for a mino to move left or right by one block
	/// </summary>
	public float MinoMoveTime => _minoMoveTime;

	/// <summary>
	///		When the player holds down the input buttons to move a mino left or right, the mino will move faster than normal and uses this value rather than MinoMoveTime
	/// </summary>
	public float FastMinoMoveTime => MinoMoveTime / 2f;

	/// <summary>
	///		The number of seconds it takes for a mino to rotate left or right by 90 degrees
	/// </summary>
	public float MinoRotateTime => _minoRotateTime;

	/// <summary>
	///		When the player holds down the input buttons to rotate a mino left or right, the mino will rotate faster than normal and use this value rather than MinoRotateTime
	/// </summary>
	public float FastMinoRotateTime => MinoRotateTime / 2f;

	/// <summary>
	///		The number of seconds it takes a mino to be placed once it lands on the wall
	/// </summary>
	public float MinoPlaceTime => _minoPlaceTime;

	/// <summary>
	///		The current active mino on the board that is being controlled by player input
	/// </summary>
	public BlockGroup ActiveMino { get => _activeMino; set => _activeMino = value; }

	/// <summary>
	///		The percentage of a Mino's weighted percentage value that is redistributed to the rest of the percentages. The higher this number, the less likely it is to have multiple Minos appear in a row
	/// </summary>
	public float MinoWeightPercentage => minoWeightPercentage;

	/// <summary>
	///		The direction that minos will rotate
	/// </summary>
	public int RotateDirection => _rotateDirection;

	/// <summary>
	///		The number of minos that have spawned in a row without a boom block on them
	/// </summary>
	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }

	/// <summary>
	///		The chance that a boom block will spawn on the current mino
	/// </summary>
	public float BoomBlockSpawnChance => ((float) BoomBlockDrought / boomBlockGuarantee) * (1 - boomBlockChance) + boomBlockChance;
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		Board = FindObjectOfType<Board>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion

	/// <summary>
	///		Update all variables that effect game difficulty based on the amount of breakthroughs that have been achieved
	/// </summary>
	public void UpdateDifficulty ( ) {
		// Calculate the mino fall time for the specified settings
		float minoScaleFactor = (defaultMinoFallTime / GameSettingsManager.MinoSpeedMultiplier) - MinMinoFallTime;
		float minoSlope = -GameSettingsManager.MinoSpeedMultiplier * difficultyValue * Breakthroughs;
		MinoFallTime = (minoScaleFactor * Mathf.Exp(minoSlope)) + MinMinoFallTime;

		// Calculate the hazard fall time for the specified settings
		float hazardScaleFactor = (defaultHazardFallTime / GameSettingsManager.HazardSpeedMultiplier) - minHazardFallTime;
		float hazardSlope = -GameSettingsManager.HazardSpeedMultiplier * difficultyValue * Breakthroughs;
		HazardFallTime = (hazardScaleFactor * Mathf.Exp(hazardSlope)) + minHazardFallTime;

		// Calculate the wall strength for the specified settings
		// * This assumes that the wall health values are between 0 and 3
		float strengthSlope = -GameSettingsManager.WallStrengthMultiplier * difficultyValue * Breakthroughs;
		WallStrength = (-2f * Mathf.Exp(strengthSlope)) + 3f;

		// Calculate the wall height for the specified settings
		float availableBoardArea = GameSettingsManager.BoardHeight - Board.BreakthroughBoardArea.Height - Board.HazardBoardArea.Height;
		float heightMax = availableBoardArea / 2f;
		float heightMin = availableBoardArea / 8f;
		float heightScaleFactor = heightMin - heightMax;
		float heightSlope = -difficultyValue * Breakthroughs;
		WallHeight = Mathf.RoundToInt((heightScaleFactor * Mathf.Exp(heightSlope)) + heightMax);

		// Calculate the boom block spawn chance for the specified settings
		float chanceSlope = -difficultyValue * Breakthroughs;
		float chanceScaleFactor = GameSettingsManager.BoomBlockChance - 1;
		boomBlockChance = (chanceScaleFactor * Mathf.Exp(chanceSlope)) + 1;
	}
}