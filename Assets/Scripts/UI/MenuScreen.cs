using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuScreen : MonoBehaviour {
	[SerializeField] private MenuManager menuManager;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private int _menuLevel;
	[SerializeField] private Vector2Int _gridDimensions;
	[SerializeField] private List<GridComponent> _gridComponents;

	#region Properties
	public int MenuLevel => _menuLevel;
	public Vector2Int GridDimensions { get => _gridDimensions; private set => _gridDimensions = value; }
	public List<GridComponent> GridComponents { get => _gridComponents; private set => _gridComponents = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		menuManager = FindObjectOfType<MenuManager>( );
		rectTransform = GetComponent<RectTransform>( );

		GridComponents = GetComponentsInChildren<GridComponent>( ).ToList( );

		rectTransform.anchorMin = rectTransform.anchorMax = Vector2.up;
	}

	private IEnumerator Start ( ) {
		// https://www.reddit.com/r/Unity3D/comments/3b0861/getting_the_size_of_a_stretched_recttransform/
		yield return new WaitForEndOfFrame( );
		yield return new WaitForEndOfFrame( );

		OnValidate( );

		// Clamp the grid dimensions of the menu screen
		int clampedGridDimensionsX = Mathf.Clamp(GridDimensions.x, 0, menuManager.GridDimensions.x);
		int clampedGridDimensionsY = Mathf.Clamp(GridDimensions.y, 0, menuManager.GridDimensions.y);
		GridDimensions = new Vector2Int(clampedGridDimensionsX, clampedGridDimensionsY);

		// Set the position and size of the rect transform
		rectTransform.sizeDelta = GridDimensions * Constants.UI_GRID_SIZE;
		float anchoredX = rectTransform.sizeDelta.x / 2f;
		float anchoredY = (-rectTransform.sizeDelta.y / 2f) + (-MenuLevel * rectTransform.rect.height);
		rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);

		// Generate the grid of blocks in the background based on perlin noise
		for (int x = 0; x < GridDimensions.x; x++) {
			for (int y = 0; y < GridDimensions.y; y++) {
				// If the noise value is not equal to 0, then generate a grid box component
				if (menuManager.GridBoxNoise[x, y + (GridDimensions.y * MenuLevel)] >= 0.25) {
					menuManager.CreateGridBoxComponent(this, new Vector2Int(x, y));
				}
			}
		}
	}
	#endregion
}
