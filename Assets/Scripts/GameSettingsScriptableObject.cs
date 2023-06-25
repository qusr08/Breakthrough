using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Game Settings")]
public class GameSettingsScriptableObject : ScriptableObject {
	[SerializeField, Range(0, 15), Tooltip("9 - 23, steps of 1")] private int _boardWidth;
	[SerializeField, Range(0, 15), Tooltip("21 - 35, steps of 1")] private int _boardHeight;
	[SerializeField, Range(0, 11), Tooltip("0.25 - 3.0, steps of 0.25")] private int _minoSpeedMultiplier;
	[SerializeField, Range(0, 11), Tooltip("0.25 - 3.0, steps of 0.25")] private int _hazardSpeedMultiplier;
	[SerializeField, Range(0, 7), Tooltip("0.25 - 2.0, steps of 0.25")] private int _wallMultiplier;
	[SerializeField, Range(0, 9), Tooltip("0.1 - 1.0, steps of 0.1")] private int _boomBlockChance;
	[SerializeField, Range(0, 10), Tooltip("0 - 10, steps of 1")] private int _gameLevel;
	[SerializeField, Tooltip("Binary representation of the allowed minos")] private int _allowedMinos;

	#region Properties
	public int BoardWidth => _boardWidth + 9;
	public int BoardHeight => _boardHeight + 21;
	public float MinoSpeedMultiplier => (_minoSpeedMultiplier * 0.25f) + 0.25f;
	public float HazardSpeedMultiplier => (_hazardSpeedMultiplier * 0.25f) + 0.25f;
	public float WallMultiplier => (_wallMultiplier * 0.25f) + 0.25f;
	public float BoomBlockChance => (_boomBlockChance * 0.1f) + 0.1f;
	public int GameLevel => _gameLevel;
	public int AllowedMinos => _allowedMinos;

	public string GameSettingsCode => $"{BoardWidth:X}{BoardHeight:X}{MinoSpeedMultiplier:X}{HazardSpeedMultiplier:X}{WallMultiplier:X}{BoomBlockChance:X}{GameLevel:X}{AllowedMinos:X}";
	#endregion

	public void LoadGameSettingsFromString(string gameSettingsCode) {

	}
}
