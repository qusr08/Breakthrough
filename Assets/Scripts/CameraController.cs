using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private Camera _camera;

	#region Properties
	public Camera Camera => _camera;
	public float SizeScaleFactor => _camera.orthographicSize / Constants.CAM_DEFLT_VALUE;
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		themeManager = FindObjectOfType<ThemeManager>( );
		_camera = GetComponent<Camera>( );
	}

	private void Awake ( ) {
		OnValidate( );

		Camera.backgroundColor = themeManager.ActiveTheme.BackgroundColor;
	}
	#endregion
}
