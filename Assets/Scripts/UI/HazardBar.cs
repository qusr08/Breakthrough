using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HazardBar : MonoBehaviour {
	[Header("Components - Hazard Bar")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private CameraController cameraController;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] private Transform backgroundTransform;
	[SerializeField] private Transform fillTransform;
	[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
	[SerializeField] private SpriteRenderer fillSpriteRenderer;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[Header("Properties - Hazard Bar")]
	[SerializeField, Range(0f, 1f)] private float _progress;

	private float toProgress;
	private float toProgressVelocity;

	#region Properties
	public float Progress {
		get => _progress;
		set {
			_progress = Mathf.Clamp01(value);

			// Set the position and size of the fill bar
			float fillBarBottom = (-backgroundSpriteRenderer.size.y * 0.5f) + 0.5f;
			float fillBarCenter = -backgroundSpriteRenderer.size.y * (1 - _progress) / 2f;
			fillTransform.localPosition = new Vector3(0f, Mathf.Max(fillBarCenter, fillBarBottom));
			fillSpriteRenderer.size = new Vector2(1, Mathf.Max(1, _progress * backgroundSpriteRenderer.size.y));
		}
	}
	#endregion

#if UNITY_EDITOR
	private void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	private void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		cameraController = FindObjectOfType<CameraController>( );
		themeManager = FindObjectOfType<ThemeManager>( );
		gameManager = FindObjectOfType<GameManager>( );
		board = FindObjectOfType<Board>( );

		// Calculate the position of the hazard bar
		float x = -(gameManager.GameSettings.BoardWidth / 2f) - (board.BorderThickness * 1.5f) - board.BoardPadding;
		float y = 0f;

		// Set background position
		transform.position = board.transform.position + new Vector3(x, y);
		backgroundTransform.localScale = new Vector3(board.BorderThickness, board.BorderThickness, 1);
		backgroundSpriteRenderer.size = new Vector2(1, gameManager.GameSettings.BoardHeight * (4f / 3f) / cameraController.SizeScaleFactor);
		backgroundSpriteRenderer.color = themeManager.GetRandomButtonColor( );

		// Set glow size and color
		glowSpriteRenderer.size = new Vector2(1, gameManager.GameSettings.BoardHeight) + (Vector2.one * (board.GlowThickness * 2));
		glowSpriteRenderer.color = themeManager.ActiveTheme.GlowColor;

		// Set fill color
		Color hazardColor = themeManager.ActiveTheme.HazardColor;
		fillSpriteRenderer.color = new Color(hazardColor.r, hazardColor.g, hazardColor.b, 1f);

		// Update the progress
		Progress = Progress;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	public void ResetProgress ( ) {
		Progress = toProgress = 0;
	}

	private void Update ( ) {
		if (gameManager.GameState != GameState.GAME || board.BoardState == BoardState.BREAKTHROUGH) {
			return;
		}

		// Increase the progress of the bar based on the allotted time
		toProgress += Time.deltaTime / gameManager.HazardFallTime;
		Progress = Mathf.SmoothDamp(Progress, toProgress, ref toProgressVelocity, gameManager.BoardAnimationSpeed);

		// If the progress of the bar reaches the top, then drop the hazard area down
		if (Progress == 1f) {
			board.HazardBoardArea.Height++;
			board.HazardBoardArea.OnHeightChange( );

			toProgress = 0f;
		}
	}
}
