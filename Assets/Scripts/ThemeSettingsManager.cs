using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ThemeSettingsState {
	LIGHT, DARK
}

public class ThemeSettingsManager : Singleton<ThemeSettingsManager> {
	[Header("References")]
	[SerializeField] private ThemeSettingsStateDictionary themeSettingsStateDictionary;
	[Header("Properties")]
	[SerializeField] private ThemeSettingsState _themeSettingsState;

	/// <summary>
	///		The currently active theme settings state
	/// </summary>
	public ThemeSettingsState ThemeSettingsState {
		get => _themeSettingsState;
		set {
			// Do nothing if the theme is being set to the same state as it was before
			if (_themeSettingsState == value) {
				return;
			}

			_themeSettingsState = value;

			UpdateAllThemeElements( );
		}
	}

	/// <summary>
	///		The currently active theme settings
	/// </summary>
	public ThemeSettings ActiveThemeSettings => themeSettingsStateDictionary[ThemeSettingsState];

	/// <summary>
	///		Find all game objects in the scene that inherit from the IThemeElement interface and update their theme colors
	/// </summary>
	private void UpdateAllThemeElements ( ) {
		List<IThemeElement> themeElements = FindObjectsOfType<MonoBehaviour>( ).OfType<IThemeElement>( ).ToList( );
		foreach (IThemeElement themeElement in themeElements) {
			themeElement.UpdateThemeElements( );
		}
	}

	private void OnValidate ( ) {
		UpdateAllThemeElements( );
	}

	protected override void Awake ( ) {
		base.Awake( );
		OnValidate( );
	}
}
