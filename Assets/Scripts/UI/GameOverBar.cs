using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameOverBar : MonoBehaviour {
    [Header("Scene Objects")]
    [SerializeField] private Board board;
    [SerializeField] private Transform fillTransform;
    [SerializeField] private SpriteRenderer fillSpriteRenderer;
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Properties")]
    [SerializeField, Range(0f, 1f), Tooltip("How close the bar is to filling up all the way.")] private float progress;
    [SerializeField, Min(0f), Tooltip("The minimum height that the fill bar should be.")] private float minFillHeight;

    public float Progress {
        get {
            return progress;
        }

        set {
            progress = value;

            // Set the size of the bar
            spriteRenderer.size = new Vector2(board.BorderThickness, board.GameOverBoardArea.Height - board.BreakthroughBoardArea.Height);
            transform.localPosition = new Vector3(-(board.Width / 2f) - 1.5f - board.UIPadding, (board.GameOverBoardArea.Height + board.BreakthroughBoardArea.Height) / 2f - (board.Height / 2f), 0f);

            // Set the size of the fill bar
            fillSpriteRenderer.size = new Vector2(board.BorderThickness, Mathf.Max(minFillHeight, progress * spriteRenderer.size.y));
            fillTransform.localPosition = new Vector3(0.0f, -(spriteRenderer.size.y - fillSpriteRenderer.size.y) / 2f, 0.0f);
        }
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

        spriteRenderer = GetComponent<SpriteRenderer>();

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
}
