using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private Camera cam;

	#region Unity Functions
	private void OnValidate ( ) {
		themeManager = FindObjectOfType<ThemeManager>( );
		cam = GetComponent<Camera>( );
	}

	private void Awake ( ) {
		OnValidate( );

		cam.backgroundColor = themeManager.ActiveTheme.BackgroundColor;
	}
	#endregion
}
