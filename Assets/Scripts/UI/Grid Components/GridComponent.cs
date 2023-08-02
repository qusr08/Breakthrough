using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
	[SerializeField] protected ThemeManager themeManager;
	[SerializeField] protected RectTransform rectTransform;
	[SerializeField] protected RectTransform backgroundRectTransform;
	[SerializeField] protected Image backgroundImage;
	[Space]
	[SerializeField] protected Vector2Int gridPosition;
	[SerializeField] protected Vector2Int gridDimensions;

	protected int backgroundColorIndex = -1;

	#region Unity Functions
	protected virtual void OnValidate ( ) {
		themeManager = FindObjectOfType<ThemeManager>( );
		rectTransform = GetComponent<RectTransform>( );

		// Set the size and position of the component
		float x = (gridPosition.x * Constants.UI_GRID_SIZE) + ((Constants.UI_GRID_SIZE / 2f) * gridDimensions.x);
		float y = (gridPosition.y * Constants.UI_GRID_SIZE) + ((Constants.UI_GRID_SIZE / 2f) * gridDimensions.y);
		rectTransform.anchoredPosition = new Vector3(x, y, 0f);
		rectTransform.sizeDelta = gridDimensions * Constants.UI_GRID_SIZE;
		rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;

		// Scale the background tile down based on the grid dimensions of the component
		float gap = Constants.UI_GRID_SIZE * (1 - Constants.BLOCK_SCALE);
		float xGridSize = Constants.UI_GRID_SIZE * gridDimensions.x;
		float yGridSize = Constants.UI_GRID_SIZE * gridDimensions.y;
		float xScale = Mathf.Max(0f, (xGridSize - gap) / xGridSize);
		float yScale = Mathf.Max(0f, (yGridSize - gap) / yGridSize);
		backgroundRectTransform.localScale = new Vector3(xScale, yScale, 1f);
	}

	protected virtual void Awake ( ) {
		OnValidate( );

		Color toColor = themeManager.GetRandomButtonColor(ref backgroundColorIndex);
		LeanTween.color(backgroundRectTransform, toColor, 0f);
	}
	#endregion

	public virtual void OnPointerEnter (PointerEventData eventData) {
		Color toColor = themeManager.GetRandomButtonColor(ref backgroundColorIndex);
		LeanTween.color(backgroundRectTransform, toColor, Constants.UI_FADE_TIME);
	}

	public virtual void OnPointerExit (PointerEventData eventData) {

	}

	public virtual void OnPointerClick (PointerEventData eventData) {

	}
}
