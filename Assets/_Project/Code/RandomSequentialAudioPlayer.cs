using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomSequentialAudioPlayer : MonoBehaviour
{
    [Header("Tracks")]
    [Tooltip("Список аудиоклипов, которые будут проигрываться")]
    public List<AudioClip> tracks = new List<AudioClip>();

    [Header("Playback")]
    [Tooltip("Если true — после проигрывания всех треков плейлист будет заново перемешан и воспроизведён")]
    public bool loopPlaylist = true;

    [Tooltip("Длительность кроссфейда между треками в секундах. 0 — без кроссфейда")]
    [Min(0f)]
    public float crossfadeDuration = 0f;

    [Tooltip("Громкость (0..1)")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    // Внутреннее
    private List<AudioClip> _shuffled = new List<AudioClip>();
    private int _index = 0;

    // Для кроссфейда используем два AudioSource'а и переключаемся между ними
    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private bool _useA = true;
    private Coroutine _playRoutine;

    void Awake()
    {
        // Создаём/находим два источника звука
        _sourceA = GetComponent<AudioSource>();
        _sourceA.playOnAwake = false;
        _sourceA.loop = false;

        // второй источник — либо уже есть, либо добавим
        var other = GetComponents<AudioSource>();
        if (other.Length >= 2)
        {
            _sourceB = other[1];
        }
        else
        {
            _sourceB = gameObject.AddComponent<AudioSource>();
        }
        _sourceB.playOnAwake = false;
        _sourceB.loop = false;

        ApplyVolume();
    }

    void OnValidate()
    {
        // чтобы в инспекторе видеть эффект изменения громкости
        ApplyVolume();
        if (crossfadeDuration < 0f) crossfadeDuration = 0f;
    }

    void Start()
    {
        if (tracks == null || tracks.Count == 0)
        {
            Debug.LogWarning($"[{name}] Нет треков в списке для воспроизведения.");
            return;
        }

        StartPlayback();
    }

    public void StartPlayback()
    {
        StopPlaybackImmediate();
        ShuffleTracks();
        _index = 0;
        _useA = true;
        _playRoutine = StartCoroutine(PlaybackLoop());
    }

    public void StopPlaybackImmediate()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        _sourceA.Stop();
        _sourceB.Stop();
    }

    public void Pause()
    {
        _sourceA.Pause();
        _sourceB.Pause();
    }

    public void UnPause()
    {
        _sourceA.UnPause();
        _sourceB.UnPause();
    }

    private void ApplyVolume()
    {
        if (_sourceA != null) _sourceA.volume = masterVolume;
        if (_sourceB != null) _sourceB.volume = masterVolume;
    }

    private void ShuffleTracks()
    {
        _shuffled.Clear();
        _shuffled.AddRange(tracks);

        // Fisher–Yates shuffle
        for (int i = _shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = _shuffled[i];
            _shuffled[i] = _shuffled[j];
            _shuffled[j] = tmp;
        }
    }

    private IEnumerator PlaybackLoop()
    {
        while (true)
        {
            if (_shuffled.Count == 0)
                yield break;

            var clip = _shuffled[_index];
            if (clip == null)
            {
                // пропускаем null-клип
                _index++;
                if (_index >= _shuffled.Count)
                {
                    if (loopPlaylist) { ShuffleTracks(); _index = 0; }
                    else yield break;
                }
                continue;
            }

            if (crossfadeDuration <= 0f)
            {
                // простое проигрывание без кроссфейда
                var curSource = _useA ? _sourceA : _sourceB;
                curSource.clip = clip;
                curSource.volume = masterVolume;
                curSource.Play();

                // ждём пока трек не закончится
                yield return new WaitForSeconds(clip.length);

                // переключаем источник
                _useA = !_useA;
            }
            else
            {
                // кроссфейд между источниками
                var newSource = _useA ? _sourceA : _sourceB;
                var oldSource = _useA ? _sourceB : _sourceA;

                newSource.clip = clip;
                newSource.volume = 0f;
                newSource.Play();

                float t = 0f;
                float fade = Mathf.Min(crossfadeDuration, clip.length); // не фейдим дольше, чем длина
                while (t < fade)
                {
                    t += Time.unscaledDeltaTime;
                    float p = Mathf.Clamp01(t / fade);
                    newSource.volume = p * masterVolume;
                    oldSource.volume = (1f - p) * masterVolume;
                    yield return null;
                }

                // после фейда оставляем новый источник на полной громкости и останавливаем старый
                newSource.volume = masterVolume;
                oldSource.Stop();

                // ждём оставшуюся часть трека (clip.length - crossfadeDuration)
                float remaining = clip.length - fade;
                if (remaining > 0f) yield return new WaitForSeconds(remaining);

                _useA = !_useA;
            }

            // следующий индекс
            _index++;
            if (_index >= _shuffled.Count)
            {
                if (loopPlaylist)
                {
                    ShuffleTracks();
                    _index = 0;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}
