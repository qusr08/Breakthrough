using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BoardArea : MonoBehaviour {
	[Header("Components - Board Area")]
	[SerializeField] protected Board board;
	[SerializeField] protected GameManager gameManager;
	[Space]
	[SerializeField] protected Transform lineTransform;
	[SerializeField] protected Transform areaTransform;
	[SerializeField] protected SpriteRenderer lineSpriteRenderer;
	[SerializeField] protected SpriteRenderer areaSpriteRenderer;
	[SerializeField] protected Sprite fillCircleBottom;
	[SerializeField] protected Sprite fillCircleTop;
	[Header("Properties - Board Area")]
	[SerializeField, Min(0)] protected int _defaultHeight;
	[SerializeField, Range(0, 0.5f)] protected float lineThickness;
	[SerializeField] protected bool _isAreaAbove;

	private int _height;
	private float fromHeight;
	private float fromHeightVelocity;

	#region Properties
	public int Height { get => _height; set => _height = value; }
	public int DefaultHeight => _defaultHeight;
	public bool IsAreaAbove => _isAreaAbove;
	#endregion

	#region Unity Functions
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

		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );

		// Set sprite for the board area
		areaSpriteRenderer.sprite = (IsAreaAbove ? fillCircleTop : fillCircleBottom);

		// Update the height
		ResetHeight( );

		OnChildValidate( );
	}

	protected abstract void OnChildValidate ( );

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	private void Update ( ) {
		fromHeight = Mathf.SmoothDamp(fromHeight, Height, ref fromHeightVelocity, gameManager.BoardAnimationSpeed);

		if (fromHeight != Height) {
			Recalculate( );
		}
	}
	#endregion

	public abstract void OnDestroyActiveMino ( );
	public abstract void OnMergeBlockGroups ( );
	public abstract void OnHeightChange ( );

	public void ResetHeight ( ) {
		Height = DefaultHeight;
		fromHeight = DefaultHeight;
		Recalculate( );
	}

	private void Recalculate ( ) {
		// Set the position of the board area
		transform.position = new Vector3(-0.5f + (gameManager.GameSettings.BoardWidth / 2.0f), -0.5f + (IsAreaAbove ? gameManager.GameSettings.BoardHeight - fromHeight : fromHeight), 0);

		// Set the position and scale of the board area line
		lineTransform.position = transform.position;
		lineTransform.localScale = new Vector3(gameManager.GameSettings.BoardWidth, lineThickness, 1);

		// Set the position and size of the board area indicator
		areaTransform.position = transform.position + Vector3.up * (fromHeight / (IsAreaAbove ? 2.0f : -2.0f));
		areaSpriteRenderer.size = new Vector2(gameManager.GameSettings.BoardWidth, fromHeight);
	}
}