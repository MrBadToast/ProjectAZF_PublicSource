using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldMusicManager : StaticSerializedMonoBehaviour<FieldMusicManager>
{
    public EventReference ActiveMusic { get { return sound.EventReference; } }

    [SerializeField] private EventReference[] oceanFieldMusics;
    [SerializeField] private EventReference[] islandFieldMusics;
    [SerializeField, MinMaxSlider(10f, 500f,ShowFields = true)] private Vector2 fieldPlayChance;

    [ReadOnly()] public bool DoNotPlayFieldmusic = false;

    private StudioEventEmitter sound;

    private Coroutine transitionCoroutine;

    protected override void Awake()
    {
        base.Awake();
        sound = GetComponent<StudioEventEmitter>();
    }

    [SerializeField,ReadOnly()] private float fieldMusicTimer = 0f;
    [SerializeField, ReadOnly()] private float nextFieldMusicTime = 0f;

    private void Start()
    {
        nextFieldMusicTime = Random.Range(fieldPlayChance.x, fieldPlayChance.y);
    }

    float fieldMusicTransitionTime = 5.0f;

    private void Update()
    {
        if (!DoNotPlayFieldmusic)
        {
            if (!sound.IsPlaying())
            {
                fieldMusicTimer += Time.deltaTime;
                if (fieldMusicTimer > nextFieldMusicTime)
                {
                    if(IslandArea.EnteredArea.Count <= 0)
                        ChangeActiveMusic(oceanFieldMusics[Random.Range(0, oceanFieldMusics.Length)], fieldMusicTransitionTime);
                    else
                        ChangeActiveMusic(islandFieldMusics[Random.Range(0, islandFieldMusics.Length)], fieldMusicTransitionTime);

                    fieldMusicTimer = 0f;
                    nextFieldMusicTime = Random.Range(fieldPlayChance.x, fieldPlayChance.y);
                }
            }
        }
    }

    public void ChangeActiveMusic(EventReference music, float fade = 0f, float waitTime = 0f)
    {
        if(transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(Cor_ChangeMusic(music, fade, waitTime));

    }

    public void ChangeActiveMusic(string music)
    {
        if (transitionCoroutine == null)
        {
            transitionCoroutine = StartCoroutine(Cor_ChangeMusic(RuntimeManager.PathToEventReference(music), 1.0f, 0f));
        }
    }

    public void StopActiveMusic(float fade = 0f)
    {
        if(transitionCoroutine == null)
        {
            transitionCoroutine = StartCoroutine(Cor_StopMusic(fade));
        }
        else
        {
            transitionCoroutine = null;
            StopAllCoroutines();
            transitionCoroutine = StartCoroutine(Cor_StopMusic(fade));
        }
    }

    private IEnumerator Cor_ChangeMusic(EventReference music, float fade, float waitTime)
    {
        fieldMusicTimer = 0f;

        if (!ActiveMusic.IsNull)
        {
            for (float t = 0; t < fade; t += Time.deltaTime)
            {
                sound.EventInstance.setVolume(1 - t / fade);
                yield return null;
            }
        }

        sound.EventInstance.setVolume(0f);
        yield return new WaitForSeconds(waitTime);

        sound.Stop();
        sound.ChangeEvent(music);
        sound.Play();
        

        for (float t = 0; t < fade; t += Time.deltaTime)
        {
            sound.EventInstance.setVolume(t / fade);
            yield return null;
        }
        sound.EventInstance.setVolume(1f);

        transitionCoroutine = null;
    }

    private IEnumerator Cor_StopMusic(float fade)
    {
        float volume;
        sound.EventInstance.getVolume(out volume);
        for (float t = volume*fade; t > 0; t -= Time.deltaTime)
        {
            sound.EventInstance.setVolume(t/fade);
            yield return null;
        }

        transitionCoroutine = null;
        sound.EventInstance.setVolume(0f);
        sound.Stop();
    }
}
