using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThemeState {
	LIGHT, DARK
}

public class ThemeManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private ThemeStateDictionary themeSettingsList;
	[Header("Properties")]
	[SerializeField] private ThemeState _themeState;

	private static ThemeManager _instance;

	#region Properties
	public static ThemeManager Instance { get => _instance; private set => _instance = value; }
	public ThemeState ThemeState { get => _themeState; set => _themeState = value; }
	public ThemeSettings ActiveTheme => themeSettingsList[ThemeState];
	#endregion

	#region Unity Functions
	private void Awake ( ) {
		// Make sure only one instance of the theme manager is in the game at one time
		if (Instance != null && Instance != this) {
			Destroy(this);
		} else {
			Instance = this;
		}

		// Have the theme manager persist between scenes
		DontDestroyOnLoad(gameObject);
	}
	#endregion

	public Color GetRandomButtonColor ( ) {
		int _ = -1;
		return GetRandomButtonColor(ref _);
	}

	public Color GetRandomButtonColor (ref int excludedIndex) {
		int buttonColorCount = ActiveTheme.ButtonColors.Count;
		excludedIndex = Utils.GetRandomArrayIndexExcluded(buttonColorCount, excludedIndex);
		return ActiveTheme.ButtonColors[excludedIndex];
	}

	public Color GetRandomBackgroundDetailColor ( ) {
		int backgroundDetailColorCount = ActiveTheme.BackgroundDetailColors.Count;
		return ActiveTheme.BackgroundDetailColors[Random.Range(0, backgroundDetailColorCount)];
	}

	public Color GetRandomMinoColor ( ) {
		int minoColorCount = ActiveTheme.BlockColors.Count;
		return ActiveTheme.BlockColors[(BlockColor) Random.Range(0, minoColorCount)];
	}
}
