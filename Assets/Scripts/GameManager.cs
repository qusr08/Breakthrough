using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState {
	TITLE, GAME, PAUSE, BREAKTHROUGH, GAME_OVER
}

public class GameManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private Board board;
	[SerializeField] private AudioManager audioManager;
	[Space]
	[SerializeField] private GameOverBar gameOverBar;
	[SerializeField] private BreakthroughLevelBar breakthroughLevelbar;
	[SerializeField] private TextMeshProUGUI totalPointsText;
	[SerializeField] private TextMeshProUGUI boardPointsText;
	[SerializeField] private TextMeshProUGUI percentageClearedText;
	[Space]
	[SerializeField] private AnimatedText breakthroughText;
	[SerializeField] private AnimatedText gameOverText;
	[SerializeField] private AnimatedText pointsText;
	[Space]
	[SerializeField] private BoardArea _hazardBoardArea;
	[SerializeField] private BoardArea _breakthroughBoardArea;
	[Header("Properties")]
	[SerializeField] private GameState _gameState;
	[Header("Points Values")]
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each destroyed block.")] public int PointsPerDestroyedBlock = 6;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each block dropped into the breakthrough area.")] public int PointsPerDroppedBlock = 12;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each breakthrough.")] public int PointsPerBreakthrough = 600;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each fully destroyed mino.")] public int PointsPerDestroyedMino = 60;
	[SerializeField, Min(0f), Tooltip("The points that the player gets for each time they fast drop the mino.")] public int PointsPerFastDrop = 2;
	[Header("Game Values")]
	[SerializeField, Min(0), Tooltip("The number of boards that have been generated.")] private int boardsGenerated = 0;
	[Space]
	[SerializeField, Tooltip("The height of the board wall.")] private int wallHeight = 0;
	[SerializeField, Min(0f), Tooltip("The minimum height of the board wall.")] private int wallHeightMinimum = 2;
	[SerializeField, Min(0f), Tooltip("The maximum height of the board wall.")] private int wallHeightMaximum = 15;
	[SerializeField, Tooltip("The minimum health that new wall blocks can be generated with.")] private float wallBlockHealthMinimum = 0f;
	[SerializeField, Tooltip("The maximum health that new wall blocks can be generated with.")] private float wallBlockHealthMaximum = 0f;
	[Space]
	[SerializeField, Tooltip("The rate at which the game over bar increments, in blocks per second.")] private float gameOverBarSpeed = 0.2f;
	[Space]
	[SerializeField, Range(0f, 1f), Tooltip("The base spawn chance for boom blocks.")] private float boomBlockSpawnChance = 0.4f;
	[SerializeField, Min(0), Tooltip("How many Minos it will take to guarantee that the player gets a boom block on their Mino.")] private int boomBlockGuarantee = 5;
	[Space]
	[SerializeField, Min(0f), Tooltip("The speed that mino block groups fall.")] private float minoFallTime = 1f;
	[SerializeField, Min(0f), Tooltip("The sensitivity of the player movement.")] private float minoMoveTime = 0.15f;
	[SerializeField, Min(0f), Tooltip("The sensitivity of the player rotation.")] private float minoRotateTime = 0.25f;
	[SerializeField, Range(-1, 1), Tooltip("The direction that the player controlled block group should be rotated. -1 is clockwise, 1 is counter-clockwise")] private int minoRotateDirection = -1;
	[SerializeField, Min(0f), Tooltip("How long a block group that is controlled by the player needs to be stationary in order for it to be placed.")] private float minoPlaceTime = 0.75f;
	[Space]
	[SerializeField, Range(0f, 1f), Tooltip("How much blocks should be scaled down when they are created. This gives a gap between each of them and gives a better idea of where each grid space is.")] private float blockScale = 0.95f;
	[SerializeField, Range(0f, 1f), Tooltip("How fast block group transforms should be animated.")] private float animationSpeed = 0.04f;

	private int _boomBlockDrought = 0;

	#region Properties

	public BoardArea BreakthroughBoardArea { get => _breakthroughBoardArea; }
	public BoardArea HazardBoardArea { get => _hazardBoardArea; }

	public float MinoFallTime => minoFallTime;
	public float MinoMoveTime => minoMoveTime;
	public float MinoRotateTime => minoRotateTime;
	public int MinoRotateDirection => minoRotateDirection;
	public float MinoPlaceTime => minoPlaceTime;

	public float BlockScale => blockScale;
	public float AnimationSpeed => animationSpeed;

	public int BoomBlockDrought { get => _boomBlockDrought; set => _boomBlockDrought = value; }

	public float CurrentBoomBlockSpawnPercentage { get => ((float) _boomBlockDrought / boomBlockGuarantee) * (1 - boomBlockSpawnChance) + boomBlockSpawnChance; }
	public float FallTimeAccelerated { get => minoFallTime / 20f; }
	public float MoveTimeAccelerated { get => minoMoveTime / 2f; }

	public int BoardPoints {
		get => int.Parse(boardPointsText.text);
		set => boardPointsText.text = value.ToString( );
	}

	public int TotalPoints {
		get => int.Parse(totalPointsText.text);
		set => totalPointsText.text = value.ToString( );
	}

	public float PercentageCleared {
		get {
			int stringLength = percentageClearedText.text.Length;
			return float.Parse(percentageClearedText.text.Substring(0, stringLength - 1));
		}
		set {
			percentageClearedText.text = $"{(value * 100):0.##}%";
		}
	}

	public GameState GameState {
		get {
			return _gameState;
		}
		set {
			_gameState = value;

			switch (_gameState) {
				case GameState.TITLE:
					break;
				case GameState.GAME:
					break;
				case GameState.PAUSE:
					break;
				case GameState.BREAKTHROUGH:
					StartCoroutine(BreakthroughSequence( ));
					break;
				case GameState.GAME_OVER:
					break;
			}
		}
	}

	#endregion

	private void Awake ( ) {
		// Set board area delegate methods
		BreakthroughBoardArea.OnDestroyMino += ( ) => GameState = GameState.BREAKTHROUGH;
		BreakthroughBoardArea.OnUpdateBlockGroups += ( ) => {
			// Clear all blocks inside the brekathrough board area
			for (int y = 0; y < BreakthroughBoardArea.CurrentHeight; y++) {
				for (int x = 0; x < board.Width; x++) {
					// If there is a block at the position, then remove it
					if (board.RemoveBlockFromBoard(new Vector3(x, y), true)) {
						BoardPoints += PointsPerDroppedBlock;
					}
				}
			}
		};
		HazardBoardArea.OnHeightChange += IsGameOver;
		HazardBoardArea.OnUpdateBlockGroups += IsGameOver;
	}

	private void Start ( ) {
		StartCoroutine(GenerateBoardSequence( ));
	}

	/// <summary>
	/// Update certain gameplay UI sizes
	/// </summary>
	public void UpdateGameplayUI ( ) {
		gameOverBar.RecalculateHeight( );
		breakthroughLevelbar.RecalculateHeight( );
	}

	/// <summary>
	/// Check to see if the player has gotten a game over. This method is added to the game over board area delegates
	/// </summary>
	private void IsGameOver ( ) {
		// If the state is already in game over, do not try and set it again
		// This can happen if the game over board area changes in height after the block groups are updated
		if (GameState == GameState.GAME_OVER) {
			return;
		}

		if (board.GetPercentageClearRectangle(0, Mathf.RoundToInt(HazardBoardArea.ToCurrentHeight), board.Width, 1) < 1f) {
			GameState = GameState.GAME_OVER;
		}
	}

	#region Sequences
	private IEnumerator BreakthroughSequence ( ) {
		audioManager.PlaySoundEffect(SoundEffectClipType.WIN);

		// Breakthrough text appears on screen
		breakthroughText.ShowText(board.transform.position, true);

		// Screen shakes a little bit

		// Repell background squares

		// Break all blocks off of the board
		// These blocks should leave behind a "shadow" of where they were to make it more obvious how much of the board was cleared
		yield return StartCoroutine(ClearBoardSequence(0.25f, true));

		// Wait for a bit
		yield return new WaitForSeconds(1f);

		// Move breakthrough text upwards
		breakthroughText.MoveText(board.transform.position + (Vector3.up * board.Height / 4f));

		// Update points
		BoardPoints += PointsPerBreakthrough * breakthroughLevelbar.Level;
		int totalPointsGained = (int) (BoardPoints * (PercentageCleared / 100f));

		// Have percentage cleared text appear
		pointsText.SetText($"{BoardPoints} pts x {PercentageCleared:0.##}% clear \n\n+{totalPointsGained} pts");
		pointsText.ShowText(board.transform.position, false);

		// Wait for a bit
		yield return new WaitForSeconds(3f);

		// Actually clear the board now
		yield return StartCoroutine(ClearBoardSequence(0.1f));

		// Hide text
		breakthroughText.HideText( );
		pointsText.HideText( );

		// Reset variables
		TotalPoints += totalPointsGained;
		BoardPoints = 0;
		PercentageCleared = 0f;
		HazardBoardArea.ToCurrentHeight = HazardBoardArea.DefaultHeight;
		gameOverBar.Progress = 0f;

		// Generate a new wall
		yield return StartCoroutine(GenerateBoardSequence( ));
	}

	private IEnumerator ClearBoardSequence (float speed, bool convertToShadows = false) {
		// This sequence will destroy each row of the board one by one
		// Only keep looping if there is a block in the row
		bool hasBlockInRow = true;
		float y = HazardBoardArea.CurrentHeight;

		while (hasBlockInRow) {
			hasBlockInRow = false;

			// Loop through the entire row at a certain y value
			for (int x = 0; x < board.Width; x++) {
				// If there is a block at the position, then remove it
				if (board.RemoveBlockFromBoard(new Vector3(x, y), true, convertToShadows)) {
					hasBlockInRow = true;
				}
			}

			y++;

			// Wait a little bit before destroying the next row
			yield return new WaitForSeconds(speed);
		}
	}

	private IEnumerator GenerateBoardSequence ( ) {
		// Update varibles for difficulty scaling
		wallHeight = Mathf.RoundToInt(Mathf.Clamp(0.96f * (Mathf.Sin(0.58f * boardsGenerated) + (0.68f * boardsGenerated)) + wallHeightMinimum, wallHeightMinimum, wallHeightMaximum));
		wallBlockHealthMinimum = Mathf.Clamp(0.123f * boardsGenerated + 1f, 1, 3);
		wallBlockHealthMaximum = Mathf.Clamp(0.063f * boardsGenerated, 0, 2);
		minoFallTime = Mathf.Clamp(0.032f * boardsGenerated + 1f, 1f / 20f, 1f);
		gameOverBarSpeed = Mathf.Clamp(0.06f * boardsGenerated + 2f, 2f, 4.5f); // CHANGE THIS

		// Update the breakthrough level indicator
		breakthroughLevelbar.Level = (boardsGenerated / 6) + 1;
		breakthroughLevelbar.Progress = boardsGenerated % 6;

		// Increase the difficulty of the game after calculations
		boardsGenerated++;

		board.ResetBoard( );

		float[ , ] wallHealthValues = Utils.GenerateRandomNoiseGrid(board.Width, wallHeight, wallBlockHealthMinimum, wallBlockHealthMaximum);
		for (int j = 0; j < wallHeight; j++) {
			for (int i = 0; i < board.Width; i++) {
				// Make sure the perlin noise value can be converted to a wall block
				int healthValue = Mathf.RoundToInt(wallHealthValues[i, j]);

				// There is also a 50 percent chance that the health will just increase
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
					// healthValue++;
					healthValue += UnityEngine.Random.Range(-1, 2);
				}

				// Make sure the bottom of the wall is always solid
				// This prevents any straight paths being generated
				if (healthValue == 0 && j == 0) {
					healthValue = 1;
				}

				// Make sure the value stays within the range of the wall health
				healthValue = Mathf.Clamp(healthValue, 0, 3);

				// If the noise value is greater than 0, a wall block will spawn
				// If it is less than or equal to 0, there will be a gap in the wall at that point
				if (healthValue > 0) {
					board.CreateBlock(new Vector3(i, j + BreakthroughBoardArea.DefaultHeight), healthValue);
				}
			}

			audioManager.PlaySoundEffect(SoundEffectClipType.BUILD_WALL);

			yield return new WaitForSeconds(0.25f);
		}

		// Update the state to start placing minos
		// BoardUpdateState = BoardUpdateState.PLACING_MINO;
		board.BoardState = BoardState.UPDATING_BLOCK_GROUPS;
		GameState = GameState.GAME;
	}

	#endregion
}