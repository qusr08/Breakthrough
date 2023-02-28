using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardUILabel : MonoBehaviour {
	[Header("Components")]
	[SerializeField] public TextMeshProUGUI Label;
	[SerializeField] public TextMeshProUGUI Value;

	private void OnValidate ( ) {
		Label = transform.Find("Label").GetComponent<TextMeshProUGUI>( );
		Value = transform.Find("Value").GetComponent<TextMeshProUGUI>( );
	}

    private void Awake ( ) {
		OnValidate( );
    }
}
