using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GridComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
	[SerializeField] protected ThemeManager themeManager;
	[SerializeField] protected CameraManager cameraController;
	[SerializeField] protected RectTransform rectTransform;
	[SerializeField] protected RectTransform backgroundRectTransform;
	[SerializeField] protected Image backgroundImage;
	[Space]
	[SerializeField] protected Vector2Int _gridPosition;
	[SerializeField] protected Vector2Int _gridDimensions;

	protected delegate Color OnHoverColorFunction ( );
	protected delegate Color OnIdleColorFunction ( );

	protected OnHoverColorFunction onHoverColorFunction;
	protected OnIdleColorFunction onIdleColorFunction;

	private bool isHovered;
	private LTDescr backgroundColorLTID = null;

	#region Properties
	public Vector2Int GridPosition { get => _gridPosition; set => _gridPosition = value; }
	public Vector2Int GridDimensions { get => _gridDimensions; set => _gridDimensions = value; }
	protected Vector2 MouseWorldPosition => cameraController.Camera.ScreenToWorldPoint(Mouse.current.position.ReadValue( ));
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		cameraController = FindObjectOfType<CameraManager>( );
		themeManager = FindObjectOfType<ThemeManager>( );
		rectTransform = GetComponent<RectTransform>( );

		RecalculateUI( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );

		onHoverColorFunction = ( ) => themeManager.GetRandomMinoColor( );
		onIdleColorFunction = ( ) => themeManager.GetRandomButtonColor( );

		backgroundImage.color = onIdleColorFunction( );
		isHovered = false;
	}

	protected virtual void Update ( ) {
		// If the mouse position is close to this grid component, fade the colors of the background
		float mouseDistance = Utils.DistanceSquared(MouseWorldPosition, transform.position);
		if (mouseDistance <= GameManager.UI_GRID_SIZE) {
			if (!isHovered) {
				FadeBackgroundColor(onHoverColorFunction( ), Constants.UI_FADE_TIME);
				isHovered = true;
			}
		} else {
			if (isHovered) {
				FadeBackgroundColor(onIdleColorFunction( ), Constants.UI_FADE_TIME * 3);
				isHovered = false;
			}
		}
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

	public virtual void OnPointerEnter (PointerEventData eventData) { }

	public virtual void OnPointerExit (PointerEventData eventData) { }

	public virtual void OnPointerClick (PointerEventData eventData) { }

	protected void FadeBackgroundColor (Color toColor, float fadeTime) {
		// If there is currently a background tween happening, cancel it
		if (backgroundColorLTID != null) {
			LeanTween.cancel(backgroundImage.gameObject, backgroundColorLTID.id);
		}

		backgroundColorLTID = LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, toColor, fadeTime)
			.setOnComplete(( ) => backgroundColorLTID = null);
	}
}
