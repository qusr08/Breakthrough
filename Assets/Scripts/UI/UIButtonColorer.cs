using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonColorer : MonoBehaviour {
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private Image image;
	[SerializeField, Tooltip("How long in seconds it takes for the image to fade to a new color.")] private float fadeTime;

	#region Unity Functions
	private void OnValidate ( ) {
		themeManager = FindObjectOfType<ThemeManager>( );
		image = GetComponent<Image>( );
	}

	private void Awake ( ) {
		OnValidate( );

		int buttomColorCount = themeManager.ActiveThemeSettings.ButtonColors.Count;
		image.color = themeManager.ActiveThemeSettings.ButtonColors[Random.Range(0, buttomColorCount)];
	}

	private void OnMouseEnter ( ) {

	}
	#endregion


}
