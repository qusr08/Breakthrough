using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour {
	[SerializeField] private GameObject blockParticlePrefab;
	[SerializeField] private GameObject blockDebrisPrefab;
	[SerializeField] private GameObject boomBlockParticlePrefab;

	public void SpawnBlockDebris (Vector2Int position, Color color) {
		ParticleSystem blockDebris = Instantiate(blockDebrisPrefab, (Vector3Int) position, Quaternion.identity, transform).GetComponent<ParticleSystem>( );
		ParticleSystem.MainModule blockDebrisMainModule = blockDebris.main;
		blockDebrisMainModule.startColor = color;
		blockDebris.Play( );
	}

	public void SpawnBlockParticle (Vector2Int position, Color color) {
		SpriteRenderer blockParticleSpriteRenderer = Instantiate(blockParticlePrefab, (Vector3Int) position, Quaternion.identity, transform).GetComponent<SpriteRenderer>( );
		blockParticleSpriteRenderer.color = color;
	}

	public void SpawnBoomBlockParticle (Vector2Int position, Color color) {
		ParticleSystem boomBlockParticle = Instantiate(boomBlockParticlePrefab, (Vector3Int) position, Quaternion.identity, transform).GetComponent<ParticleSystem>( );
		ParticleSystem.MainModule boomBlockParticleMainModule = boomBlockParticle.main;
		boomBlockParticleMainModule.startColor = color;
		boomBlockParticle.Play( );
	}
}
