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

	public static Color BackgroundColor => Instance.ActiveThemeSettings.BackgroundColor;
	public static Color DetailColor => Instance.ActiveThemeSettings.DetailColor;
	public static Color TextColor => Instance.ActiveThemeSettings.TextColor;
	public static Color GlowColor => Instance.ActiveThemeSettings.GlowColor;
	public static Color HazardColor => Instance.ActiveThemeSettings.HazardColor;
	public static Color BreakthroughColor => Instance.ActiveThemeSettings.BreakthroughColor;
	public static List<Color> BackgroundDetailColors => Instance.ActiveThemeSettings.BackgroundDetailColors;
	public static MinoBlockColorDictionary MinoBlockColors => Instance.ActiveThemeSettings.MinoBlockColors;
	public static WallBlockColorDictionary WallBlockColors => Instance.ActiveThemeSettings.WallBlockColors;
	#endregion

	#region Unity Functions

	#endregion
}
