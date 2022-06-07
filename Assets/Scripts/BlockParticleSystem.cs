using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockParticleSystem : MonoBehaviour {
	[SerializeField] private ParticleSystem particleSystem2D;

	public void SetSprite (Sprite sprite) {
		Texture2D texture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
		texture.SetPixels(sprite.texture.GetPixels((int) sprite.rect.x, (int) sprite.rect.y, (int) sprite.rect.width, (int) sprite.rect.height));
		texture.Apply( );

		GetComponent<ParticleSystemRenderer>( ).material.SetTexture("_MainTex", texture);
	}

	public void Play ( ) {
		particleSystem2D.Play( );
	}
}
