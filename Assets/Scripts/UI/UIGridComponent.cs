using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGridComponent : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private UIGridManager uiGridManager;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private RectTransform backgroundRectTransform;
	[Header("Properties")]
	[SerializeField] private Vector2Int gridPosition;
	[SerializeField] private Vector2Int gridDimensions;

	#region Unity Functions
	private void OnValidate ( ) {
		uiGridManager = FindObjectOfType<UIGridManager>( );
		rectTransform = GetComponent<RectTransform>( );

		// Set the size and position of the component
		float x = (gridPosition.x * uiGridManager.GridSize) + ((uiGridManager.GridSize / 2f) * gridDimensions.x);
		float y = (gridPosition.y * uiGridManager.GridSize) + ((uiGridManager.GridSize / 2f) * gridDimensions.y);
		rectTransform.anchoredPosition = new Vector3(x, y, 0f);
		rectTransform.sizeDelta = gridDimensions * uiGridManager.GridSize;
		rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;

		// Scale the background tile down based on the grid dimensions of the component
		float gap = uiGridManager.GridSize * (1 - Constants.BLK_SCL);
		float xGridSize = uiGridManager.GridSize * gridDimensions.x;
		float yGridSize = uiGridManager.GridSize * gridDimensions.y;
		float xScale = Mathf.Max(0f, (xGridSize - gap) / xGridSize);
		float yScale = Mathf.Max(0f, (yGridSize - gap) / yGridSize);
		backgroundRectTransform.localScale = new Vector3(xScale, yScale, 1f);
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion
}
