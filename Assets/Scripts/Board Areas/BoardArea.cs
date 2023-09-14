using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BoardArea : MonoBehaviour {
	[SerializeField, Min(0), Tooltip("The default height of the board area.")] private int _defaultHeight;
	[SerializeField, Tooltip("Whether or not the board area comes from the top or bottom of the board. This orientation effects the height value as well.")] private bool _isFlipped;
	[SerializeField, Tooltip("A reference to this board area's sizer script.")] protected BoardAreaSizer boardAreaSizer;
	[SerializeField, Tooltip("A reference to the game manager.")] protected GameManager gameManager;

	private int _height;

	#region Properties
	public int DefaultHeight => _defaultHeight;
	public int Height {
		get => _height;
		set {
			LeanTween.value(gameObject, boardAreaSizer.Recalculate, _height, value, gameManager.FastMinoMoveTime);
			_height = value;
		}
	}
	public bool IsFlipped => _isFlipped;
	#endregion

	#region Unity Functions
	protected virtual void OnValidate ( ) {
		boardAreaSizer = GetComponent<BoardAreaSizer>( );
		gameManager = FindObjectOfType<GameManager>( );

		_height = DefaultHeight;
		boardAreaSizer.Recalculate(_height);
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}
	#endregion

	public abstract void OnHeightChange ( );
	public abstract void OnBreakthrough ( );
	public abstract void OnBlockGroupMerge ( );
}
