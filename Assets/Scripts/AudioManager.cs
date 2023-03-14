using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public enum SoundEffectClipType {
	MOVE_BLOCK_GROUP, ROTATE_BLOCK_GROUP, BOOM_BLOCK_FRAME, WIN, BREAK_BLOCK, SPAWN_MINO, BUILD_WALL
};

public class AudioManager : MonoBehaviour {
	[Header("Prefabs")]
	[SerializeField] private AudioSource effectAudioSourcePrefab;
	[Header("Components")]
	[SerializeField] private AudioSource musicAudioSource;
	[Header("Properties")]
	[SerializeField, Tooltip("All background music tracks.")] private List<AudioClip> musicClips;
	[SerializeField, Tooltip("All of the sound effects for the game. They are in a specific order to match the enum in the AudioManager class.")] private List<AudioClip> effectClips;

	private int currentMusicClipIndex;

	private float[ ] effectCooldowns;
	private Queue<AudioSource> effectAudioSources = new Queue<AudioSource>( );
	private LinkedList<AudioSource> effectAudioSourcesInUse = new LinkedList<AudioSource>( );
	private int lastCheckFrame = -1;

	private void Start ( ) {
		effectCooldowns = new float[Utils.GetEnumSize(typeof(SoundEffectClipType))];
	}

	private void Update ( ) {
		for (int i = 0; i < effectCooldowns.Length; i++) {
			effectCooldowns[i] -= Time.deltaTime;
		}

		if (!musicAudioSource.isPlaying) {
			PlayRandomMusicClip( );
		}
	}

	private void PlayRandomMusicClip ( ) {
		musicAudioSource.time = 0f;
		currentMusicClipIndex = Utils.GetRandomArrayIndexExcluded(musicClips, currentMusicClipIndex);
		musicAudioSource.clip = musicClips[currentMusicClipIndex];
		musicAudioSource.Play( );
	}

	public void PlaySoundEffect (SoundEffectClipType soundEffectClipType) {
		// https://forum.unity.com/threads/perfomant-audiosource-pool.503056/
		int soundEffectIndex = (int) soundEffectClipType;

		// Wait for the cooldown to finish before playing the same sound effect
		if (effectCooldowns[soundEffectIndex] > 0f) {
			return;
		}

		AudioSource source;

		// Update what audio sources are in use
		if (lastCheckFrame != Time.frameCount) {
			lastCheckFrame = Time.frameCount;
			UpdateAudioSourcesInUse( );
		}

		// Get an unused audio source
		// If there are no unused audio sources, create one
		if (effectAudioSources.Count == 0) {
			source = GameObject.Instantiate(effectAudioSourcePrefab);
		} else {
			source = effectAudioSources.Dequeue( );
		}

		// Play the sound effect
		effectAudioSourcesInUse.AddLast(source);
		source.clip = effectClips[soundEffectIndex];
		source.Play( );
	}

	private void UpdateAudioSourcesInUse ( ) {
		var node = effectAudioSourcesInUse.First;
		while (node != null) {
			var current = node;
			node = node.Next;

			if (!current.Value.isPlaying) {
				effectAudioSources.Enqueue(current.Value);
				effectAudioSourcesInUse.Remove(current);
			}
		}
	}
}
