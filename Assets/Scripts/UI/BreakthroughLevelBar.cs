using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BreakthroughLevelBar : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[SerializeField] private Transform currentLevelTransform;
	[SerializeField] private TextMeshPro currentLevelText;
	[SerializeField] private SpriteRenderer currentLevelSpriteRenderer;
	[SerializeField] private Transform nextLevelTransform;
	[SerializeField] private TextMeshPro nextLevelText;
	[SerializeField] private SpriteRenderer nextLevelSpriteRenderer;
	[SerializeField] private List<SpriteRenderer> dotSpriteRenderers;
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The spacing between this bar and the board.")] private float uiPadding;
	[SerializeField, Min(0f), Tooltip("The spacing between each of the blocks that make up this bar.")] private float elementPadding;
	[SerializeField, Tooltip("The color that the elements appear when they are not selected.")] private Color unselectedColor;
	[SerializeField, Tooltip("The color that the elements appear when they are selected.")] private Color selectedColor;
	[SerializeField, Range(0f, 2f), Tooltip("The unselected size of a dot.")] private float unselectedSize;
	[SerializeField, Range(0f, 2f), Tooltip("The selected size of a dot.")] private float selectedSize;
	[SerializeField, Tooltip("The current progress of the player to reaching the next level.")] private int progress;
	[SerializeField, Min(0), Tooltip("The current level that is being displayed.")] private int level;

	private float[ ] toDotSizes = new float[6];
	private float[ ] toDotSizesVelocities = new float[6];

	private bool calledOnValidate;

	public int Progress {
		get {
			return progress;
		}
		set {
			progress = value;

			// Set what the dot elements should resize to and change their color to
			for (int i = 0; i < dotSpriteRenderers.Count; i++) {
				bool isSelected = progress >= dotSpriteRenderers.Count - i;
				dotSpriteRenderers[i].color = (isSelected ? selectedColor : unselectedColor);
				toDotSizes[i] = isSelected ? selectedSize : unselectedSize;
			}
		}
	}

	public int Level {
		get {
			return level;
		}
		set {
			level = value;

			// Update the level text variables
			currentLevelText.text = level.ToString( );
			nextLevelText.text = (level + 1).ToString( );
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

		RecalculateHeight( );

		// Calculate the sizes of the dots based on the progress
		for (int i = 0; i < dotSpriteRenderers.Count; i++) {
			toDotSizes[i] = unselectedSize;
			toDotSizesVelocities[i] = 0f;
		}

		Progress = progress;

		// Immediately set the size of the dots in the editor
		/*for (int i = 0; i < dotSpriteRenderers.Count; i++) {
			float size = toDotSizes[i];
			dotSpriteRenderers[i].size = new Vector2(size, size);
		}*/

		// Set the color of the elements
		currentLevelSpriteRenderer.color = selectedColor;
		nextLevelSpriteRenderer.color = unselectedColor;

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

		// Smoothly transition between states of the elements
		for (int i = 0; i < dotSpriteRenderers.Count; i++) {
			float currentSize = dotSpriteRenderers[i].size.x;
			float smoothedValue = Mathf.SmoothDamp(currentSize, toDotSizes[i], ref toDotSizesVelocities[i], gameManager.AnimationSpeed);
			dotSpriteRenderers[i].size = new Vector2(smoothedValue, smoothedValue);
		}
	}

	/// <summary>
	/// Calculate the position of a specific element
	/// </summary>
	/// <param name="elementIndex">The index of the element. This is essentially the order of the elements as they stack vertically.</param>
	/// <returns>The position to set the element to</returns>
	private Vector3 GetElementPosition (int elementIndex) {
		int offsetIndex = 8 - elementIndex;
		return new Vector3(transform.position.x, offsetIndex + (offsetIndex * elementPadding) + gameManager.BreakthroughBoardArea.CurrentHeight, 0f);
	}

	/// <summary>
	/// Recalculate the y position of all the elements
	/// </summary>
	public void RecalculateHeight ( ) {
		// Set the positions of all the elements
		transform.position = new Vector3(board.transform.position.x + (board.Width / 2f) + board.BorderThickness + uiPadding, (8 + (7 * elementPadding)) / 2f + gameManager.BreakthroughBoardArea.CurrentHeight, 0f);
		currentLevelTransform.position = GetElementPosition(8);
		for (int i = 0; i < dotSpriteRenderers.Count; i++) {
			dotSpriteRenderers[i].transform.position = GetElementPosition(i + 2);
		}
		nextLevelTransform.position = GetElementPosition(1);
	}
}
