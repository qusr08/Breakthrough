using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BoardArea : MonoBehaviour {
	[Header("Components - Board Area")]
	[SerializeField] protected Board board;
	[Space]
	[SerializeField] protected Transform lineTransform;
	[SerializeField] protected Transform areaTransform;
	[SerializeField] protected SpriteRenderer lineSpriteRenderer;
	[SerializeField] protected SpriteRenderer areaSpriteRenderer;
	[SerializeField] protected Sprite fillCircleBottom;
	[SerializeField] protected Sprite fillCircleTop;
	[Header("Properties - Board Area")]
	[SerializeField, Min(0)] private int _height;
	[SerializeField] protected Color color;
	[SerializeField, Range(0, 1)] protected float areaOpacity;
	[SerializeField, Range(0, 0.5f)] protected float lineThickness;
	[SerializeField] private bool _isAreaAbove;

	private int defaultHeight;

	#region Properties
	public int Height {
		get => _height;
		set {
			_height = value;

			// Set the position of the board area
			transform.position = new Vector3(-0.5f + (board.Width / 2.0f), -0.5f + _height, 0);

			// Set the position and scale of the board area line
			lineTransform.position = transform.position;
			lineTransform.localScale = new Vector3(board.Width, lineThickness, 1);

			// Set the position and size of the board area indicator
			areaTransform.position = transform.position + Vector3.up * (IsAreaAbove ? (board.Height - _height) / 2.0f : -_height / 2.0f);
			areaSpriteRenderer.size = new Vector2(board.Width, IsAreaAbove ? board.Height - _height : _height);
		}
	}
	public bool IsAreaAbove => _isAreaAbove;
	#endregion

	#region Unity
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

		board = FindObjectOfType<Board>( );

		// Set sprites and colors of for the board area
		lineSpriteRenderer.color = color;
		areaSpriteRenderer.sprite = (IsAreaAbove ? fillCircleTop : fillCircleBottom);
		areaSpriteRenderer.color = new Color(color.r, color.g, color.b, areaOpacity);

		// Update the height
		Height = Height;
	}

	protected void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif

		defaultHeight = Height;
	}
	#endregion

	public abstract void OnDestroyMino ( );
	public abstract void OnUpdateBlockGroups ( );
	public abstract void OnHeightChange ( );

	public void ResetHeight ( ) {
		Height = defaultHeight;
	}
}