using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HazardBar : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Board board;
	[Space]
	[SerializeField] private Transform backgroundTransform;
	[SerializeField] private Transform fillTransform;
	[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
	[SerializeField] private SpriteRenderer fillSpriteRenderer;
	[SerializeField] private SpriteRenderer glowSpriteRenderer;
	[Header("Properties")]
	[SerializeField, Range(0f, 1f)] private float _progress;
	[SerializeField, Range(0f, 1f)] private float thickness;

	private float toProgress;
	private float toProgressVelocity;

	#region Properties
	public float Progress {
		get => _progress;
		set {
			_progress = Mathf.Clamp01(value);

			// Set the position and size of the fill bar
			fillTransform.localPosition = new Vector3(0f, Mathf.Max(-backgroundSpriteRenderer.size.y * (1 - _progress) / 2f, (-backgroundSpriteRenderer.size.y * 0.5f) + 0.5f));
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

		gameManager = FindObjectOfType<GameManager>( );
		board = FindObjectOfType<Board>( );

		// Set background position
		transform.localPosition = new Vector3(-(board.Width / 2) - board.BorderThickness - board.BoardPadding, 0f);
		backgroundTransform.localScale = new Vector3(thickness, thickness, 1);
		backgroundSpriteRenderer.size = new Vector2(1, board.Height * (4f / 3f));

		// Set glow size
		glowSpriteRenderer.size = new Vector2(1, board.Height) + (Vector2.one * (board.GlowThickness * 2));

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

	private void Update ( ) {
		if (board.BoardState == BoardState.GAMEOVER || board.BoardState == BoardState.BREAKTHROUGH) {
			return;
		}

		// Increase the progress of the bar based on the allotted time
		// Progress += Time.deltaTime / gameManager.HazardTime;
		toProgress += Time.deltaTime / 10f;
		Progress = Mathf.SmoothDamp(Progress, toProgress, ref toProgressVelocity, gameManager.BlockGroupAnimationSpeed);

		// If the progress of the bar reaches the top, then drop the hazard area down
		if (Progress == 1f) {
			board.HazardBoardArea.Height--;
			board.HazardBoardArea.OnHeightChange( );

			toProgress = 0f;
		}
	}
}
