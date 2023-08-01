using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridComponent : MonoBehaviour, IPointerEnterHandler {
	[Header("Components")]
	[SerializeField] private ThemeManager themeManager;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private RectTransform backgroundRectTransform;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private TextMeshProUGUI textMeshPro;
	[SerializeField] private Image image;
	[Header("Properties")]
	[SerializeField] private Vector2Int gridPosition;
	[SerializeField] private Vector2Int gridDimensions;
	[Space]
	[SerializeField] private bool _showBackground;
	[SerializeField] private bool _showText;
	[SerializeField] private bool _showImage;

	private int backgroundColorIndex = -1;

	#region Properties
	public bool ShowBackground {
		get => _showBackground;
		private set {
			_showBackground = value;
			backgroundRectTransform.gameObject.SetActive(_showBackground);
		}
	}
	public bool ShowText {
		get => _showText;
		private set {
			_showText = value;
			textMeshPro.gameObject.SetActive(_showText);
		}
	}
	public bool ShowImage {
		get => _showImage;
		private set {
			_showImage = value;
			image.gameObject.SetActive(_showImage);
		}
	}
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
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

		// Set image color
		image.color = new Color(1f, 1f, 1f, Constants.BLOCK_ICON_ALPHA);

		// Update showing variables
		ShowBackground = ShowBackground;
		ShowText = ShowText;
		ShowImage = ShowImage;
	}

	private void Awake ( ) {
		OnValidate( );

		textMeshPro.color = themeManager.ActiveTheme.TextColor;
		FadeToNewColor(backgroundRectTransform, 0f);
	}
	#endregion

	private void FadeToNewColor (RectTransform rectTransform, float seconds) {
		// Get a new color to fade to
		Color toColor = themeManager.GetRandomButtonColor(ref backgroundColorIndex);

		// Tween to the new color
		LeanTween.color(rectTransform, toColor, seconds);
	}

	public void OnPointerEnter (PointerEventData eventData) {
		FadeToNewColor(backgroundRectTransform, Constants.UI_FADE_TIME);
	}
}
