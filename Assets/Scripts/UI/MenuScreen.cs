using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuScreen : MonoBehaviour {
	[SerializeField] private MenuManager menuManager;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private int _menuLevel;
	[SerializeField] private List<GridComponent> _gridComponents;
	[SerializeField] private Vector2 _dimensionModifiers;

	private Vector2Int gridDimensions = Vector2Int.zero;

	#region Properties
	public int MenuLevel => _menuLevel;
	public Vector2 DimensionModifiers => _dimensionModifiers;
	public List<GridComponent> GridComponents { get => _gridComponents; private set => _gridComponents = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		menuManager = FindObjectOfType<MenuManager>( );
		rectTransform = GetComponent<RectTransform>( );
		GridComponents = GetComponentsInChildren<GridComponent>( ).ToList( );

		RecalculateUI( );
	}

	private void Awake ( ) {
		OnValidate( );

		GenerateGridBoxComponents( );
	}
	#endregion

	private void RecalculateUI ( ) {
		// Calculate the size of the rect transform
		rectTransform.sizeDelta = Constants.SCRN_RES * DimensionModifiers;
		gridDimensions = Utils.Vect2Round(rectTransform.sizeDelta / Constants.UI_GRID_SIZE);

		// Calculate the anchored position of the rect transform
		float anchoredX = rectTransform.sizeDelta.x / 2f;
		float anchoredY = (-rectTransform.sizeDelta.y / 2f) + (-MenuLevel * rectTransform.rect.height);
		// rectTransform.anchorMin = rectTransform.anchorMax = Vector2.up;
		rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
	}

	private void GenerateGridBoxComponents ( ) {
		// Generate the grid of blocks in the background based on perlin noise
		for (int x = 0; x < gridDimensions.x; x++) {
			for (int y = 0; y < gridDimensions.y; y++) {
				// If the noise value is not equal to 0, then generate a grid box component
				if (menuManager.GridBoxNoise[x, y + (gridDimensions.y * MenuLevel)] >= 0.25) {
					menuManager.CreateGridBoxComponent(this, new Vector2Int(x, y));
				}
			}
		}
	}
}
