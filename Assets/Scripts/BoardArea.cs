using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardArea : MonoBehaviour {
	[Header("Scene GameObjects")]
	[SerializeField] private Board board;
	[Header("Components")]
	[SerializeField] private Transform lineTransform;
	[SerializeField] private Transform tintedAreaTransform;
	[SerializeField] private Sprite boardFillCircleBottom;
	[SerializeField] private Sprite boardFillCircleTop;
	[Header("Properties")]
	[SerializeField, Range(0f, 1f), Tooltip("The thickness of the line that defines the board area.")] private float lineThickness = 0.25f;
	[SerializeField, Range(0f, 1f), Tooltip("How transparent the board area is.")] private float areaOpacity = 0.1f;
	[SerializeField, Tooltip("The color of the board area.")] public Color Color;
	[SerializeField, Tooltip("Whether or not the board area is above or below the line.")] public bool IsAreaAbove;
	[SerializeField, Min(1), Tooltip("The height of the board area.")] public int Height;

	public delegate void OnMinoEnterDelegate ( );
	public OnMinoEnterDelegate OnMinoEnter = ( ) => { };

	public delegate void OnMinoLeaveDelegate ( );
	public OnMinoEnterDelegate OnMinoLeave = ( ) => { };

	public delegate void OnMinoLandDelegate ( );
	public OnMinoLandDelegate OnMinoLand = ( ) => { };

	public delegate void OnBlockEnterDelegate ( );
	public OnBlockEnterDelegate OnBlockEnter = ( ) => { };

	// Whether or not the mino was previously inside the area
	// This is here to make sure the delegate methods are only called when the mino changes states, as in when it first enters or leaves
	private bool wasMinoInArea;

	// Whether or not the active Mino is within the area
	public bool IsMinoInArea {
		get {
			// If there happens to be no Mino on the board, there will not be a Mino in the area
			if (board.ActiveMino == null) {
				return false;
			}

			// Check the position of the Mino relative to the board area line
			if (IsAreaAbove && (board.ActiveMino.BoundsMaxY > transform.position.y)) {
				return true;
			}
			if (!IsAreaAbove && (board.ActiveMino.BoundsMinY < transform.position.y)) {
				return true;
			}

			return false;
		}
	}

	// Whether or not the active mino has landed inside the area
	public bool IsMinoLandedInArea {
		get {
			// If the mino is not in the area, then it definitely did not land in the area
			if (!IsMinoInArea) {
				return false;
			}

			// Return if the mino has landed or not
			return board.ActiveMino.HasLanded;
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

		// Get gameobjects within the scene
		board = FindObjectOfType<Board>( );
		lineTransform = transform.Find("Line");
		tintedAreaTransform = transform.Find("Tinted Area");

		// Do not do any of the below calculations if the board was not found
		if (board == null) {
			return;
		}

		// Set the position of the board area line based on the height specified
		transform.position = new Vector3(-0.5f + board.Width / 2.0f, -0.5f + Height, 0);

		// Set the position and size of the line
		lineTransform.position = transform.position;
		lineTransform.localScale = new Vector3(board.Width, lineThickness, 1);
		lineTransform.GetComponent<SpriteRenderer>( ).color = Color;

		// Set the position and size of the tinted area
		tintedAreaTransform.position = transform.position + Vector3.up * (IsAreaAbove ? (board.Height - Height) / 2.0f : Height / -2.0f);
		tintedAreaTransform.GetComponent<SpriteRenderer>( ).sprite = (IsAreaAbove ? boardFillCircleTop : boardFillCircleBottom);
		tintedAreaTransform.GetComponent<SpriteRenderer>( ).size = new Vector2(board.Width, (IsAreaAbove ? board.Height - Height : Height));
		tintedAreaTransform.GetComponent<SpriteRenderer>( ).color = new Color(Color.r, Color.g, Color.b, areaOpacity);
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}

    /// <summary>
    /// Update all of the delegate methods and check for scenarios where they would be triggered
    /// </summary>
    public void UpdateDelegates ( ) {
		// If the mino is inside the board area, call some delegate methods
		if (IsMinoInArea) {
			// If the mino was not previously inside this board area, then it has just entered the board area
			if (!wasMinoInArea) {
				wasMinoInArea = true;

				OnMinoEnter( );
			}
		} else {
			// If the mino is no longer inside the board area but previously was, then it has left the board area
			if (wasMinoInArea) {
				wasMinoInArea = false;

				OnMinoLeave( );
			}
		}

		// If the mino lands inside this board area, call some delegate methods
		if (IsMinoLandedInArea) {
			OnMinoLand( );
		}
	}
}
