using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
	[SerializeField] private GameObject gridBoxComponentPrefab;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private List<MenuScreen> menuScreens;

	private int menuLevelCount;
	private MenuScreen prevMenuScreen;

	private Vector2Int _gridDimensions = Vector2Int.zero;
	private MenuScreen _activeMenuScreen;
	private float[ , ] _gridBoxNoise;

	#region Properties
	public Vector2Int GridDimensions { get => _gridDimensions; set => _gridDimensions = value; }
	public float[ , ] GridBoxNoise { get => _gridBoxNoise; private set => _gridBoxNoise = value; }

	public MenuScreen ActiveMenuScreen {
		get => _activeMenuScreen;
		set {
			prevMenuScreen = _activeMenuScreen;

			_activeMenuScreen = value;
			_activeMenuScreen.gameObject.SetActive(true);

			Vector2 toAnchoredPosition = new Vector2(0, _activeMenuScreen.MenuLevel * rectTransform.rect.height);
			LeanTween.value(gameObject, (Vector2 vector) => rectTransform.anchoredPosition = vector, rectTransform.anchoredPosition, toAnchoredPosition, Constants.UI_MENU_TRANS_TIME)
				.setOnComplete(( ) => {
					if (prevMenuScreen != null) {
						prevMenuScreen.gameObject.SetActive(false);
					}
				});
		}
	}
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		rectTransform = GetComponent<RectTransform>( );

		RecalculateUI( );
	}

	private void Awake ( ) {
		OnValidate( );

		ActiveMenuScreen = menuScreens[0];
		
	}

	#endregion

	private void RecalculateUI ( ) {
		// Calculate the grid dimensions of the screen
		GridDimensions = Constants.SCREEN_RES / Constants.UI_GRID_SIZE;

		// Find the highest menu level of this menu
		foreach (MenuScreen menuScreen in menuScreens) {
			menuLevelCount = Mathf.Max(menuLevelCount, menuScreen.MenuLevel + 1);
		}

		// Generate a noise grid for the grid box components
		GridBoxNoise = Utils.GenerateRandomNoiseGrid(GridDimensions.x, GridDimensions.y * menuLevelCount, 0f, 1f);
	}

	public void CreateGridBoxComponent (MenuScreen menuScreen, Vector2Int gridPosition) {
		foreach (GridComponent gridComponent in menuScreen.GridComponents) {
			int minX = gridComponent.GridPosition.x;
			int maxX = gridComponent.GridPosition.x + gridComponent.GridDimensions.x;
			int minY = gridComponent.GridPosition.y;
			int maxY = gridComponent.GridPosition.y + gridComponent.GridDimensions.y;

			// Check to see if the new grid position of the to-be-created box grid component is overlapping another grid component
			bool isOverlappingX = (gridPosition.x >= minX && gridPosition.x < maxX);
			bool isOverlappingY = (gridPosition.y >= minY && gridPosition.y < maxY);

			// If there is an overlap, then return from this method and do not create a new box grid component
			if (isOverlappingX && isOverlappingY) {
				return;
			}
		}

		BoxGridComponent boxGridComponent = Instantiate(gridBoxComponentPrefab, menuScreen.transform).GetComponent<BoxGridComponent>( );
		boxGridComponent.GridPosition = gridPosition;
		boxGridComponent.GridDimensions = Vector2Int.one;
		boxGridComponent.transform.SetAsFirstSibling( );
		boxGridComponent.RecalculateUI( );
	}
}
