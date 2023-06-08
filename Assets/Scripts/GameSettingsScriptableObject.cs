using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class GameSettingsScriptableObject : ScriptableObject {
	[SerializeField] private int _boardWidth;
	[SerializeField] private int _boardHeight;
	[SerializeField] private int _minoSpeedMultiplier;
	[SerializeField] private int _wallMultiplier;
	[SerializeField] private int _boomBlockChance;
	[SerializeField] private int _gameLevel;
	[SerializeField] private int _allowedMinos;

	#region Properties
	public int BoardWidth => _boardWidth;
	public int BoardHeight => _boardHeight;
	public float MinoSpeedMultiplier => _minoSpeedMultiplier;
	public float WallMultiplier => _wallMultiplier;
	public float BoomBlockChance => _boomBlockChance;
	public int GameLevel => _gameLevel;
	public int AllowedMinos => _allowedMinos;

	public string GameSettingsCode => $"{BoardWidth:X}{BoardHeight:X}{MinoSpeedMultiplier:X}{WallMultiplier:X}{BoomBlockChance:X}{GameLevel:X}{AllowedMinos:X}";
	#endregion

	public void LoadGameSettingsFromString(string gameSettingsCode) {

	}
}
