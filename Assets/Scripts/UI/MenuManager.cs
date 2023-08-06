using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {
	[SerializeField] private GameObject gridBoxComponentPrefab;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private MenuScreen titleScreen;
	[SerializeField] private MenuScreen menuScreen;
	[SerializeField] private MenuScreen playScreen;
	[SerializeField, Min(1)] private int menuLevels;

	private int _activeMenuLevel = 0;
	private Vector2Int _gridDimensions;
	private MenuScreen prevMenuScreen;
	private MenuScreen _activeMenuScreen;
	private float[ , ] _gridBoxNoise;

	#region Properties
	public Vector2Int GridDimensions { get => _gridDimensions; set => _gridDimensions = value; }
	public float[ , ] GridBoxNoise { get => _gridBoxNoise; private set => _gridBoxNoise = value; }

	public int ActiveMenuLevel {
		get => _activeMenuLevel;
		set {
			_activeMenuLevel = value;

			Vector2 toAnchoredPosition = new Vector2(0, _activeMenuLevel * rectTransform.rect.height);
			LeanTween.value(gameObject, (Vector2 vector) => rectTransform.anchoredPosition = vector, rectTransform.anchoredPosition, toAnchoredPosition, Constants.UI_MENU_TRANS_TIME)
				.setOnComplete(( ) => {
					if (prevMenuScreen != null) {
						prevMenuScreen.gameObject.SetActive(false);
					}
				});
		}
	}
	public MenuScreen ActiveMenuScreen {
		get => _activeMenuScreen;
		set {
			prevMenuScreen = _activeMenuScreen;

			_activeMenuScreen = value;

			_activeMenuScreen.gameObject.SetActive(true);
			ActiveMenuLevel = _activeMenuScreen.MenuLevel;
		}
	}
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		rectTransform = GetComponent<RectTransform>( );
	}

	private void Awake ( ) {
		OnValidate( );

		ActiveMenuScreen = titleScreen;
	}

	private IEnumerator Start ( ) {
		yield return new WaitForEndOfFrame( );

		GridDimensions = Utils.Vect2Round(rectTransform.rect.size / Constants.UI_GRID_SIZE);
		GridBoxNoise = Utils.GenerateRandomNoiseGrid(GridDimensions.x, GridDimensions.y * menuLevels, 0, 3);
	}
	#endregion

	public void CreateGridBoxComponent (MenuScreen menuScreen, Vector2Int gridPosition) {
		BoxGridComponent boxGridComponent = Instantiate(gridBoxComponentPrefab, menuScreen.transform).GetComponent<BoxGridComponent>( );
		boxGridComponent.GridPosition = gridPosition;
		boxGridComponent.GridDimensions = Vector2Int.one;
		boxGridComponent.RecalculateUI( );
	}
}
