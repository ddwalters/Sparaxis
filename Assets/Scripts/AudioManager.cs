using System.Collections;
using System.Collections.Generic;
using NodeTree;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private List<AudioClip> musicTracks;

    [Header("Interaction SFX")]
    [SerializeField] private AudioClip printerSound;
    [SerializeField] private AudioClip gardenSound;
    [SerializeField] private AudioClip shuttleSound;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (musicTracks.Count > 0)
            StartCoroutine(PlayPlaylist());
    }

    private IEnumerator PlayPlaylist()
    {
        int index = 0;
        while (true)
        {
            musicSource.clip = musicTracks[index];
            musicSource.Play();
            yield return new WaitForSeconds(musicTracks[index].length);
            index = (index + 1) % musicTracks.Count;
        }
    }

    private void OnEnable()
    {
        NodeTreeEvents.Subscribe("ShuttleGenome",     PlayShuttleSound);
        NodeTreeEvents.Subscribe("SetHasSeenPrinter", PlayPrinterSound);
        NodeTreeEvents.Subscribe("SetHasSeenGarden",  PlayGardenSound);
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("ShuttleGenome",     PlayShuttleSound);
        NodeTreeEvents.Unsubscribe("SetHasSeenPrinter", PlayPrinterSound);
        NodeTreeEvents.Unsubscribe("SetHasSeenGarden",  PlayGardenSound);
    }

    private void PlayPrinterSound() => PlaySFX(printerSound);
    private void PlayGardenSound()  => PlaySFX(gardenSound);
    private void PlayShuttleSound() => PlaySFX(shuttleSound);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
