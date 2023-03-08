using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardArea : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[Header("Components")]
	[SerializeField] private Transform lineTransform;
	[SerializeField] private Transform tintedAreaTransform;
	[SerializeField] private SpriteRenderer lineSpriteRenderer;
	[SerializeField] private SpriteRenderer tintedAreaSpriteRenderer;
	[SerializeField] private Sprite boardFillCircleBottom;
	[SerializeField] private Sprite boardFillCircleTop;
	[Header("Properties")]
	[SerializeField, Range(0f, 1f), Tooltip("The thickness of the line that defines the board area.")] private float lineThickness = 0.25f;
	[SerializeField, Range(0f, 1f), Tooltip("How transparent the board area is.")] private float areaOpacity = 0.1f;
	[SerializeField, Tooltip("The color of the board area.")] public Color Color;
	[SerializeField, Tooltip("Whether or not the board area is above or below the line.")] public bool IsAreaAbove;
	[SerializeField, Min(0), Tooltip("The default height of the board area.")] public int DefaultHeight;

	private float currentHeight;
	[HideInInspector] public float ToCurrentHeight;
	private float toCurrentHeightVelocity;

	public delegate void OnUpdateDelegate ( );
	public OnUpdateDelegate OnUpdate = ( ) => { };

	public delegate void OnDestroyMinoDelegate ( );
	public OnDestroyMinoDelegate OnDestroyMino = ( ) => { };

	public float CurrentHeight {
		get {
			return currentHeight;
		}

		set {
			currentHeight = value;

			// Do not update anything if the board is null
			if (board == null) {
				return;
			}

			// Set the position of the board area line based on the height specified
			transform.position = new Vector3(-0.5f + board.Width / 2.0f, -0.5f + currentHeight, 0);

			// Set the position and size of the line
			lineTransform.position = transform.position;
			lineTransform.localScale = new Vector3(board.Width, lineThickness, 1);

			// Set the position and size of the tinted area
			tintedAreaTransform.position = transform.position + Vector3.up * (IsAreaAbove ? (board.Height - currentHeight) / 2.0f : -currentHeight / 2.0f);
			tintedAreaSpriteRenderer.size = new Vector2(board.Width, (IsAreaAbove ? board.Height - currentHeight : currentHeight));

			// Update any board UI that may rely on the heights of this board area
			board.UpdateGameplayUI( );
		}
	}

#if UNITY_EDITOR
	protected void OnValidate ( ) => EditorApplication.delayCall += _OnValidate;
#endif
	protected void _OnValidate ( ) {
#if UNITY_EDITOR
		EditorApplication.delayCall -= _OnValidate;
		if (this == null) {
			return;
		}
#endif

		// Set sprites and colors of for the board area
		lineSpriteRenderer.color = Color;
		tintedAreaSpriteRenderer.sprite = (IsAreaAbove ? boardFillCircleTop : boardFillCircleBottom);
		tintedAreaSpriteRenderer.color = new Color(Color.r, Color.g, Color.b, areaOpacity);

		// Fully update the animations so they are visible
		CurrentHeight = DefaultHeight;
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private void Update ( ) {
		CurrentHeight = Mathf.SmoothDamp(CurrentHeight, ToCurrentHeight, ref toCurrentHeightVelocity, gameManager.BlockAnimationSpeed);
	}
}
