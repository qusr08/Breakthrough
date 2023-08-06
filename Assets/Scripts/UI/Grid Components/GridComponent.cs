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
	[SerializeField] protected Vector2Int _gridPosition;
	[SerializeField] protected Vector2Int _gridDimensions;

	protected int backgroundColorIndex = -1;

	#region Properties
	public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }
	public Vector2Int GridDimensions { get => _gridDimensions; set => _gridDimensions = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		themeManager = FindObjectOfType<ThemeManager>( );
		rectTransform = GetComponent<RectTransform>( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );

		backgroundImage.color = themeManager.GetRandomButtonColor(ref backgroundColorIndex);
	}
	#endregion

	public void RecalculateUI ( ) {
		// Set the size and position of the component
		float x = (GridPosition.x * Constants.UI_GRID_SIZE) + ((Constants.UI_GRID_SIZE / 2f) * GridDimensions.x);
		float y = (GridPosition.y * Constants.UI_GRID_SIZE) + ((Constants.UI_GRID_SIZE / 2f) * GridDimensions.y);
		rectTransform.anchorMin = rectTransform.anchorMax = Vector2.up;
		rectTransform.anchoredPosition = new Vector3(x, -y, 0f);
		rectTransform.sizeDelta = GridDimensions * Constants.UI_GRID_SIZE;

		// Scale the background tile down based on the grid dimensions of the component
		float gap = Constants.UI_GRID_SIZE * (1 - Constants.BLOCK_SCALE);
		float xGridSize = Constants.UI_GRID_SIZE * GridDimensions.x;
		float yGridSize = Constants.UI_GRID_SIZE * GridDimensions.y;
		float xScale = Mathf.Max(0f, (xGridSize - gap) / xGridSize);
		float yScale = Mathf.Max(0f, (yGridSize - gap) / yGridSize);
		backgroundRectTransform.localScale = new Vector3(xScale, yScale, 1f);
	}

	protected void FadeBackgroundColor (Color toColor, float fadeTime) {
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, toColor, fadeTime);
	}

	public virtual void OnPointerEnter (PointerEventData eventData) {
		Color toColor = themeManager.GetRandomButtonColor(ref backgroundColorIndex);
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, toColor, Constants.UI_FADE_TIME);
	}

	public virtual void OnPointerExit (PointerEventData eventData) {

	}

	public virtual void OnPointerClick (PointerEventData eventData) {

	}
}
