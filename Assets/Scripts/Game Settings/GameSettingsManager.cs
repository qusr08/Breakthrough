using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSettingsState {
	DEFAULT
}

public class GameSettingsManager : Singleton<GameSettingsManager> {
	[SerializeField] private GameSettingsState _gameSettingsState;
	[SerializeField] private GameSettingsStateDictionary gameSettingsStateDictionary;

	#region Properties
	public GameSettingsState GameSettingsState => _gameSettingsState;
	public GameSettings ActiveGameSettings => gameSettingsStateDictionary[GameSettingsState];

	public static int BoardWidth => Instance.ActiveGameSettings.BoardWidth;
	public static int BoardHeight => Instance.ActiveGameSettings.BoardHeight;
	public static float MinoSpeedMultiplier => Instance.ActiveGameSettings.MinoSpeedMultiplier;
	public static float HazardSpeedMultiplier => Instance.ActiveGameSettings.HazardSpeedMultiplier;
	public static float WallStrengthMultiplier => Instance.ActiveGameSettings.WallStrengthMultiplier;
	public static float BoomBlockChance => Instance.ActiveGameSettings.BoomBlockChance;
	public static int GameLevel => Instance.ActiveGameSettings.GameLevel;
	public static int AllowedMinos => Instance.ActiveGameSettings.AllowedMinos;
	#endregion

	#region Unity Functions

	#endregion
}
