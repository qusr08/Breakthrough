using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIButton : MonoBehaviour {
	[Header("Properties")]
	[SerializeField, Min(0f), Tooltip("The pixel dimensions (width and height) of the button.")] private float pixelSize;
	[SerializeField, Tooltip("The dimensions of the button in grid sections.")] private Vector2Int dimensions;
	[SerializeField, Min(0f), Tooltip("The spacing between buttons.")] private float spacing;
	[SerializeField, Tooltip("The coordinates of the button around the center of its parent object. This factors in the spacing between buttons so it is better than setting the transform position directly.")] private Vector2Int coordinates;
	[Header("Components")]
	[SerializeField] private RectTransform rectTransform;

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

		// Set the dimensions of the button
		float width = pixelSize * dimensions.x;
		float height = pixelSize * dimensions.y;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

		// Set the position of the button
		Vector2 center = new Vector2();
		rectTransform.localPosition = new Vector2(coordinates.x * (width + spacing), coordinates.y * (height + spacing));
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}
