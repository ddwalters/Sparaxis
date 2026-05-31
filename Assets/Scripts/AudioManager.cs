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
    [SerializeField] private AudioClip waterSound;
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
        NodeTreeEvents.Subscribe("ShuttleGenome", PlayShuttleSound);
        NodeTreeEvents.Subscribe("PrintGenome", PlayPrinterSound);
        NodeTreeEvents.Subscribe("SetHasSeenGarden", PlayGardenSound);
    }

    private void OnDisable()
    {
        NodeTreeEvents.Unsubscribe("ShuttleGenome", PlayShuttleSound);
        NodeTreeEvents.Unsubscribe("PrintGenome", PlayPrinterSound);
        NodeTreeEvents.Unsubscribe("SetHasSeenGarden", PlayGardenSound);
    }

    private const float MaxInteractionDuration = 5f;

    private void PlayPrinterSound() => PlaySFXCapped(printerSound);
    public  void PlayGardenSound()  => PlaySFXCapped(gardenSound);
    public  void PlayWaterSound()   => PlaySFXCapped(waterSound);
    private void PlayShuttleSound() => PlaySFXCapped(shuttleSound);

    public void PlaySFXCapped(AudioClip clip)
    {
        if (clip == null) return;
        StopCoroutine(nameof(StopSFXAfterDuration));
        sfxSource.clip = clip;
        sfxSource.Play();
        StartCoroutine(nameof(StopSFXAfterDuration));
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private IEnumerator StopSFXAfterDuration()
    {
        yield return new WaitForSeconds(MaxInteractionDuration);
        sfxSource.Stop();
    }
}
