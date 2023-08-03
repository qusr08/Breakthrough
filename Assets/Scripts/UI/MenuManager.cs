using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuType {
	TITLE, MENU, PLAY
}

public class MenuManager : MonoBehaviour {
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private MenuScreen titleScreen;
	[SerializeField] private MenuScreen menuScreen;
	[SerializeField] private MenuScreen playScreen;

	private int _activeMenuLevel = 0;
	private MenuScreen prevMenuScreen;
	private MenuScreen _activeMenuScreen;

	#region Properties
	private Vector2 AnchoredPosition { get => rectTransform.anchoredPosition; set => rectTransform.anchoredPosition = value; }

	public int ActiveMenuLevel {
		get => _activeMenuLevel;
		set {
			_activeMenuLevel = value;

			Vector2 toAnchoredPosition = new Vector2(0, _activeMenuLevel * rectTransform.rect.height);
			LeanTween.value(gameObject, (Vector2 vector) => AnchoredPosition = vector, AnchoredPosition, toAnchoredPosition, Constants.UI_MENU_TRANS_TIME)
				.setOnComplete(() => {
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
	#endregion
}
