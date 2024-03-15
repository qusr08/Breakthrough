using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSettingsState {
	DEFAULT
}

public class GameSettingsManager : Singleton<GameSettingsManager> {
	[SerializeField] private GameSettingsState _gameSettingsState;
	[SerializeField] private GameSettingsStateDictionary gameSettingsStateDictionary;

	/// <summary>
	///		The currently active game settings state
	/// </summary>
	public GameSettingsState GameSettingsState => _gameSettingsState;

	/// <summary>
	///		The currently active game settings
	/// </summary>
	public GameSettings ActiveGameSettings => gameSettingsStateDictionary[GameSettingsState];
}