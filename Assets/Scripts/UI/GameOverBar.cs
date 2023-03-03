using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameOverBar : MonoBehaviour {
    [Header("Scene Objects")]
    [SerializeField] private Board board;
    [SerializeField] private Transform fillTransform;
    [SerializeField] private SpriteRenderer fillSpriteRenderer;
    [SerializeField] private BoardArea gameOverArea;
    [SerializeField] private BoardArea breakthroughArea;
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Properties")]
    [SerializeField, Min(0f), Tooltip("The spacing between this bar and the board.")] private float uiPadding;
    [SerializeField, Range(0f, 1f), Tooltip("How close the bar is to filling up all the way.")] private float progress;
    [SerializeField, Min(0f), Tooltip("The minimum height that the fill bar should be.")] private float minFillHeight;

    private float toFillHeight;
    private float toFillHeightVelocity;
    private Vector3 toFillPosition;
    private Vector3 toFillPositionVelocity;

    public float Progress {
        get {
            return progress;
        }

        set {
            progress = value;
            progress = Mathf.Clamp01(progress);

            // Set the size of the bar
            Height = board.GameOverBoardArea.Height - board.BreakthroughBoardArea.Height;
            transform.localPosition = new Vector3(-(board.Width / 2f) - board.BorderThickness - uiPadding, (board.GameOverBoardArea.Height + board.BreakthroughBoardArea.Height) / 2f - (board.Height / 2f), 0f);

            // Set the size of the fill bar
            toFillHeight = Mathf.Max(minFillHeight, progress * spriteRenderer.size.y);
            toFillPosition = new Vector3(0.0f, -(spriteRenderer.size.y - toFillHeight) / 2f, 0.0f);
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

        // Update the size of the game over bar
        Progress = progress;
    }

    private void Awake ( ) {
#if UNITY_EDITOR
        OnValidate( );
#else
		_OnValidate( );
#endif
    }

    private void Update ( ) {
        FillHeight = Mathf.SmoothDamp(FillHeight, toFillHeight, ref toFillHeightVelocity, Mino.DAMP_SPEED);
        fillTransform.localPosition = Vector3.SmoothDamp(fillTransform.localPosition, toFillPosition, ref toFillPositionVelocity, Mino.DAMP_SPEED);
    }

    public void IncrementProgress ( ) {
        Progress += 1f / Height;
    }
}
