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

	public int BoardWidth => ActiveGameSettings.BoardWidth;
	public int BoardHeight => ActiveGameSettings.BoardHeight;
	public float MinoSpeedMultiplier => ActiveGameSettings.MinoSpeedMultiplier;
	public float HazardSpeedMultiplier => ActiveGameSettings.HazardSpeedMultiplier;
	public float WallStrengthMultiplier => ActiveGameSettings.WallStrengthMultiplier;
	public float BoomBlockChance => ActiveGameSettings.BoomBlockChance;
	public int GameLevel => ActiveGameSettings.GameLevel;
	public int AllowedMinos => ActiveGameSettings.AllowedMinos;
	#endregion

	#region Unity Functions

	#endregion
}
