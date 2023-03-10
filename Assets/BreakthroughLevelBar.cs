using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BreakthroughLevelBar : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private Board board;
	[SerializeField] private Transform currentLevelTransform;
	[SerializeField] private TextMeshPro currentLevelText;
	[SerializeField] private SpriteRenderer currentLevelSpriteRenderer;
	[SerializeField] private Transform nextLevelTransform;
	[SerializeField] private TextMeshPro nextLevelText;
	[SerializeField] private SpriteRenderer nextLevelSpriteRenderer;
	[SerializeField] private Transform dotsTransform;
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The spacing between this bar and the board.")] private float uiPadding;
	[SerializeField, Min(0f), Tooltip("The spacing between each of the blocks that make up this bar.")] private float elementPadding;
	[SerializeField, Tooltip("The color that the elements appear when they are not selected.")] private Color unselectedColor;
	[SerializeField, Tooltip("The color that the elements appear when they are selected.")] private Color selectedColor;
	[SerializeField, Range(0, 5), Tooltip("The current progress of the player to reaching the next level.")] private int progress;

	public int Progress {
		get {
			return progress;
		}
		set {
			progress = value;

			// Set the colors of the dot elements
			for (int i = 0; i < dotsTransform.childCount; i++) {
				dotsTransform.GetChild(i).GetComponent<SpriteRenderer>( ).color = (progress >= dotsTransform.childCount - 1 - i ? selectedColor : unselectedColor);
			}
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

		// Set the positions of all the elements
		transform.position = new Vector3(board.transform.position.x + (board.Width / 2f) + board.BorderThickness + uiPadding, (8 + (7 * elementPadding)) / 2f + board.BreakthroughBoardArea.CurrentHeight, 0f);
		currentLevelTransform.position = GetElementPosition(8);
		for (int i = 0; i < dotsTransform.childCount; i++) {
			dotsTransform.GetChild(i).position = GetElementPosition(i + 2);
		}
		nextLevelTransform.position = GetElementPosition(1);

		// Set the color of the elements
		currentLevelTransform.GetComponent<SpriteRenderer>( ).color = selectedColor;
		nextLevelTransform.GetComponent<SpriteRenderer>( ).color = unselectedColor;

		Progress = progress;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private Vector3 GetElementPosition (int elementIndex) {
		int offsetIndex = 8 - elementIndex;
		return new Vector3(transform.position.x, offsetIndex + (offsetIndex * elementPadding) + board.BreakthroughBoardArea.CurrentHeight, 0f);
	}
}
