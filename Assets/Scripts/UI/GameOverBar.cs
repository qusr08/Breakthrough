using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameOverBar : MonoBehaviour {
	[Header("Scene Objects")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Transform fillTransform;
	[SerializeField] private SpriteRenderer fillSpriteRenderer;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The spacing between this bar and the board.")] private float uiPadding;
	[SerializeField, Range(0f, 1f), Tooltip("How close the bar is to filling up all the way.")] private float progress;
	[SerializeField, Min(0f), Tooltip("The minimum height that the fill bar should be.")] private float minFillHeight;

	private float toHeight;
	private float toHeightVelocity;
	private float toFillHeight;
	private float toFillHeightVelocity;

	private Vector3 toPosition;
	private Vector3 toPositionVelocity;
	private Vector3 toFillPosition;
	private Vector3 toFillPositionVelocity;

	private bool calledOnValidate;

	public float Progress {
		get {
			return progress;
		}

		set {
			progress = value;
			progress = Mathf.Clamp01(progress);

			// Set the size of the fill bar
			toFillHeight = Mathf.Max(minFillHeight, progress * toHeight);
			toFillPosition = new Vector3(0.0f, -(toHeight - toFillHeight) / 2f, 0.0f);
		}
	}
	private float Height {
		get => spriteRenderer.size.y;
		set => spriteRenderer.size = new Vector2(spriteRenderer.size.x, value);
	}
	private float FillHeight {
		get => fillSpriteRenderer.size.y;
		set => fillSpriteRenderer.size = new Vector2(fillSpriteRenderer.size.x, value);
	}

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

		// Update the widths of the bars
		spriteRenderer.size = new Vector2(board.BorderThickness, 0.0f);
		fillSpriteRenderer.size = new Vector2(board.BorderThickness, 0.0f);

		RecalculateHeight( );

		// Update the size of the game over bar
		Progress = progress;

		// Fully update the animations so they are visible
		Height = toHeight;
		FillHeight = toFillHeight;
		transform.localPosition = toPosition;
		fillTransform.localPosition = toFillPosition;

		calledOnValidate = true;
	}

	private void Awake ( ) {
		calledOnValidate = false;

#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private void Update ( ) {
		if (!calledOnValidate) {
			return;
		}

		// Smoothly transition the height of the bar and the height of the fill inside
		Height = Mathf.SmoothDamp(Height, toHeight, ref toHeightVelocity, gameManager.BlockAnimationSpeed);
		FillHeight = Mathf.SmoothDamp(FillHeight, toFillHeight, ref toFillHeightVelocity, gameManager.BlockAnimationSpeed);
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, toPosition, ref toPositionVelocity, gameManager.BlockAnimationSpeed);
		fillTransform.localPosition = Vector3.SmoothDamp(fillTransform.localPosition, toFillPosition, ref toFillPositionVelocity, gameManager.BlockAnimationSpeed);

		// If the game over bar has reached its maximum height, reset the progress and decrease the game over area height
		if (toFillHeight - FillHeight < Utils.CLOSE_ENOUGH && 1f - Progress < Utils.CLOSE_ENOUGH) {
			board.GameOverBoardArea.ToCurrentHeight--;
			Progress = 0f;
		}
	}

	/// <summary>
	/// Increase the progress made in the bar
	/// </summary>
	public void IncrementProgress ( ) {
		Progress += 2f / Height;
	}

	/// <summary>
	/// Recalculate the height of the bar
	/// </summary>
	public void RecalculateHeight ( ) {
		// Set the size of the bar
		toHeight = board.GameOverBoardArea.ToCurrentHeight - board.BreakthroughBoardArea.ToCurrentHeight;
		toPosition = new Vector3(-(board.Width / 2f) - board.BorderThickness - uiPadding, (board.GameOverBoardArea.ToCurrentHeight + board.BreakthroughBoardArea.ToCurrentHeight) / 2f - (board.Height / 2f), 0f);
	}
}
