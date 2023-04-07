using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BoardArea : MonoBehaviour {
	[Header("Components - Board Area")]
	[SerializeField] protected Transform lineTransform;
	[SerializeField] protected Transform areaTransform;
	[SerializeField] protected SpriteRenderer lineSpriteRenderer;
	[SerializeField] protected SpriteRenderer areaSpriteRenderer;
	[SerializeField] protected Sprite fillCircleBottom;
	[SerializeField] protected Sprite fillCircleTop;
	[Header("Properties - Board Area")]
	[SerializeField] private int _height;
	[SerializeField] protected Color color;
	[SerializeField] protected float areaOpacity;
	[SerializeField] protected float lineThickness;
	[SerializeField] private bool _isAreaAbove;

	private int defaultHeight;

	#region Properties
	public int Height => _height;
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

		// Set sprites and colors of for the board area
		lineSpriteRenderer.color = color;
		areaSpriteRenderer.sprite = (IsAreaAbove ? fillCircleTop : fillCircleBottom);
		areaSpriteRenderer.color = new Color(color.r, color.g, color.b, areaOpacity);
	}

	protected void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
		#endregion

		defaultHeight = Height;
    }
}