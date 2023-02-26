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
		float width = (dimensions.x * pixelSize) + ((dimensions.x - 1) * spacing);
		float height = (dimensions.y * pixelSize) + ((dimensions.y - 1) * spacing);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

		// Set the position of the button
		float positionX = (pixelSize + spacing) * (coordinates.x + (dimensions.x * 0.5f - 0.5f));
		float positionY = (pixelSize + spacing) * (coordinates.y - (dimensions.y * 0.5f - 0.5f));
		rectTransform.localPosition = new Vector2(positionX, positionY);
	}

	private void Awake ( ) {
#if UNITY_EDITOR
		OnValidate( );
#else
		_OnValidate( );
#endif
	}
}
