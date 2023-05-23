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
	[SerializeField, Min(0)] private int defaultHeight;
	[SerializeField] protected Color lineColor;
	[SerializeField] protected Color areaColor;
	[SerializeField, Range(0, 0.5f)] protected float lineThickness;
	[SerializeField] private bool _isAreaAbove;

	private int _height;
	private float fromHeight;
	private float fromHeightVelocity;

	#region Properties
	public int Height {
		get => _height;
		set {
			_height = value;
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
		gameManager = FindObjectOfType<GameManager>( );

		// Set sprites and colors of for the board area
		lineSpriteRenderer.color = lineColor;
		areaSpriteRenderer.color = areaColor;
		areaSpriteRenderer.sprite = (IsAreaAbove ? fillCircleTop : fillCircleBottom);

		// Update the height
		Height = defaultHeight;
		fromHeight = defaultHeight;
		Recalculate( );
	}

	protected void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

	protected void Update ( ) {
		fromHeight = Mathf.SmoothDamp(fromHeight, Height, ref fromHeightVelocity, gameManager.BlockGroupAnimationSpeed);

		if (fromHeight != Height) {
			Recalculate( );
		}
	}
	#endregion

	public abstract void OnDestroyActiveMino ( );
	public abstract void OnUpdateBlockGroups ( );
	public abstract void OnHeightChange ( );

	private void Recalculate ( ) {
		// Set the position of the board area
		transform.position = new Vector3(-0.5f + (board.Width / 2.0f), -0.5f + fromHeight, 0);

		// Set the position and scale of the board area line
		lineTransform.position = transform.position;
		lineTransform.localScale = new Vector3(board.Width, lineThickness, 1);

		// Set the position and size of the board area indicator
		areaTransform.position = transform.position + Vector3.up * (IsAreaAbove ? (board.Height - fromHeight) / 2.0f : -fromHeight / 2.0f);
		areaSpriteRenderer.size = new Vector2(board.Width, IsAreaAbove ? board.Height - fromHeight : fromHeight);
	}
}