using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class GameSettings : ScriptableObject {
	private const int BOARD_WIDTH_RANGE = 16;
	private const int BOARD_HEIGHT_RANGE = 16;
	private const int MINO_SPEED_MULTIPLIER_RANGE = 12;
	private const int HAZARD_SPEED_MULTIPLIER_RANGE = 12;
	private const int WALL_MULTIPLIER_RANGE = 8;
	private const int BOOM_BLOCK_CHANCE_RANGE = 10;
	private const int GAME_LEVEL_RANGE = 11;
	private const int ALLOWED_MINOS_RANGE = 511;

	private const string CHAR_LIST = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const int CODE_LENGTH = 7;

	[SerializeField, Range(0, BOARD_WIDTH_RANGE - 1), Tooltip("9 - 23, steps of 1")] private int boardWidthValue;
	[SerializeField, Range(0, BOARD_HEIGHT_RANGE - 1), Tooltip("21 - 35, steps of 1")] private int boardHeightValue;
	[SerializeField, Range(0, MINO_SPEED_MULTIPLIER_RANGE - 1), Tooltip("0.25 - 3.0, steps of 0.25")] private int minoSpeedMultiplierValue;
	[SerializeField, Range(0, HAZARD_SPEED_MULTIPLIER_RANGE - 1), Tooltip("0.25 - 3.0, steps of 0.25")] private int hazardSpeedMultiplierValue;
	[SerializeField, Range(0, WALL_MULTIPLIER_RANGE - 1), Tooltip("0.25 - 2.0, steps of 0.25")] private int wallMultiplierValue;
	[SerializeField, Range(0, BOOM_BLOCK_CHANCE_RANGE - 1), Tooltip("0.1 - 1.0, steps of 0.1")] private int boomBlockChanceValue;
	[SerializeField, Range(0, GAME_LEVEL_RANGE - 1), Tooltip("0 - 10, steps of 1")] private int gameLevelValue;
	[SerializeField, Range(0, ALLOWED_MINOS_RANGE - 1), Tooltip("Binary representation of the allowed minos")] private int allowedMinosValue;
	[Space]
	[SerializeField] private string _gameSettingsCode;
	[SerializeField] private long gameSettingsLong;

	#region Properties
	public int BoardWidth => boardWidthValue + 9;
	public int BoardHeight => boardHeightValue + 21;
	public float MinoSpeedMultiplier => (minoSpeedMultiplierValue * 0.25f) + 0.25f;
	public float HazardSpeedMultiplier => (hazardSpeedMultiplierValue * 0.25f) + 0.25f;
	public float WallMultiplier => (wallMultiplierValue * 0.25f) + 0.25f;
	public float BoomBlockChance => (boomBlockChanceValue * 0.1f) + 0.1f;
	public int GameLevel => gameLevelValue;
	public int AllowedMinos => allowedMinosValue + 1;

	public string GameSettingsCode {
		get {
			// This formula gets a unique integer for the inputted game settings
			gameSettingsLong =
				(long) ALLOWED_MINOS_RANGE * (GAME_LEVEL_RANGE * (BOOM_BLOCK_CHANCE_RANGE * (WALL_MULTIPLIER_RANGE *
				(HAZARD_SPEED_MULTIPLIER_RANGE * (MINO_SPEED_MULTIPLIER_RANGE * (BOARD_HEIGHT_RANGE * (boardWidthValue) +
				boardHeightValue) + minoSpeedMultiplierValue) + hazardSpeedMultiplierValue) + wallMultiplierValue) +
				boomBlockChanceValue) + gameLevelValue) + allowedMinosValue;

			// Add 0s to the front of the code so they are all the same length
			_gameSettingsCode = Encode(gameSettingsLong).PadLeft(CODE_LENGTH, '0');

			return _gameSettingsCode;
		}

		set {
			_gameSettingsCode = value;
			long settingsLong = gameSettingsLong = Decode(value);

			// Decrypt the integer to find each of the values that created it
			allowedMinosValue = (int) (settingsLong % ALLOWED_MINOS_RANGE);
			settingsLong /= ALLOWED_MINOS_RANGE;
			gameLevelValue = (int) (settingsLong % GAME_LEVEL_RANGE);
			settingsLong /= GAME_LEVEL_RANGE;
			boomBlockChanceValue = (int) (settingsLong % BOOM_BLOCK_CHANCE_RANGE);
			settingsLong /= BOOM_BLOCK_CHANCE_RANGE;
			wallMultiplierValue = (int) (settingsLong % WALL_MULTIPLIER_RANGE);
			settingsLong /= WALL_MULTIPLIER_RANGE;
			hazardSpeedMultiplierValue = (int) (settingsLong % HAZARD_SPEED_MULTIPLIER_RANGE);
			settingsLong /= HAZARD_SPEED_MULTIPLIER_RANGE;
			minoSpeedMultiplierValue = (int) (settingsLong % MINO_SPEED_MULTIPLIER_RANGE);
			settingsLong /= MINO_SPEED_MULTIPLIER_RANGE;
			boardHeightValue = (int) (settingsLong % BOARD_HEIGHT_RANGE);
			settingsLong /= BOARD_HEIGHT_RANGE;
			boardWidthValue = (int) (settingsLong % BOARD_WIDTH_RANGE);
		}
	}
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		GameSettingsCode = GameSettingsCode;
	}
	#endregion

	// https://www.stum.de/2008/10/20/base36-encoderdecoder-in-c/
	private string Encode (long input) {
		if (input < 0) {
			return "";
		}

		char[ ] clistarr = CHAR_LIST.ToCharArray( );
		var result = new Stack<char>( );

		while (input != 0) {
			result.Push(clistarr[input % 36]);
			input /= 36;
		}

		return new string(result.ToArray( ));
	}

	private long Decode (string input) {
		var reversed = input.ToUpper( ).Reverse( );
		long result = 0;
		int pos = 0;

		foreach (char c in reversed) {
			result += CHAR_LIST.IndexOf(c) * (long) Math.Pow(36, pos);
			pos++;
		}

		return result;
	}
}
