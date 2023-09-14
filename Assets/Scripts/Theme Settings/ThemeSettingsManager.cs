using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ThemeSettingsState {
	LIGHT, DARK
}

public class ThemeSettingsManager : Singleton<ThemeSettingsManager> {
	[SerializeField] private ThemeSettingsState _themeSettingsState;
	[SerializeField] private ThemeSettingsStateDictionary themeSettingsStateDictionary;

	#region Properties
	public ThemeSettingsState ThemeSettingsState => _themeSettingsState;
	public ThemeSettings ActiveThemeSettings => themeSettingsStateDictionary[ThemeSettingsState];

	public Color BackgroundColor => ActiveThemeSettings.BackgroundColor;
	public Color DetailColor => ActiveThemeSettings.DetailColor;
	public Color TextColor => ActiveThemeSettings.TextColor;
	public Color GlowColor => ActiveThemeSettings.GlowColor;
	public Color HazardColor => ActiveThemeSettings.HazardColor;
	public Color BreakthroughColor => ActiveThemeSettings.BreakthroughColor;
	public List<Color> BackgroundDetailColors => ActiveThemeSettings.BackgroundDetailColors;
	public MinoBlockColorDictionary MinoBlockColors => ActiveThemeSettings.MinoBlockColors;
	public WallBlockColorDictionary WallBlockColors => ActiveThemeSettings.WallBlockColors;
	#endregion

	#region Unity Functions

	#endregion
}
