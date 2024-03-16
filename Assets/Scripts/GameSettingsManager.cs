using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameSettingsState {
	DEFAULT, TEST
}

public class GameSettingsManager : Singleton<GameSettingsManager> {
	[Header("References")]
	[SerializeField] private GameSettingsStateDictionary gameSettingsStateDictionary;
	[Header("Properties")]
	[SerializeField] private GameSettingsState _gameSettingsState;

	/// <summary>
	///		The currently active game settings state
	/// </summary>
	public GameSettingsState GameSettingsState {
		get => _gameSettingsState;
		set {
			// Do nothing if the theme is being set to the same state as it was before
			if (_gameSettingsState == value) {
				return;
			}

			_gameSettingsState = value;

			UpdateAllGameElements( );
		}
	}

	/// <summary>
	///		The currently active game settings
	/// </summary>
	public GameSettings ActiveGameSettings => gameSettingsStateDictionary[GameSettingsState];

	private void OnValidate ( ) {
		UpdateAllGameElements( );
	}

	protected override void Awake ( ) {
		base.Awake( );
		OnValidate( );
	}

	/// <summary>
	///		Update all necessary game elements when 
	/// </summary>
	private void UpdateAllGameElements ( ) {
		BoardManager.Instance.OnValidate( );
	}
}