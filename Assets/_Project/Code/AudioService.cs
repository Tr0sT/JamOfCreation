using UnityEngine;

public class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    [Header("Настройки")]
    [Tooltip("Максимальное количество одновременных источников звука")]
    [SerializeField] private int maxAudioSources = 10;

    private AudioSource[] sources;
    private bool[] isUnstoppable;
    private int nextSourceIndex = 0;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Создаём пул AudioSource
        sources = new AudioSource[maxAudioSources];
        isUnstoppable = new bool[maxAudioSources];
        for (int i = 0; i < maxAudioSources; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            isUnstoppable[i] = false;
        }
    }

    /// <summary>
    /// Проигрывает звук из Resources/Sounds по имени файла (без расширения).
    /// </summary>
    public void PlaySound(string filename, float volume = 1f, bool stopAnother = true, bool unstoppable = false)
    {
        if (string.IsNullOrEmpty(filename))
            return;

        var clip = Resources.Load<AudioClip>("Sound/" + filename);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioService] Не найден звук: {filename}");
            return;
        }

        // Если нужно остановить другие звуки — останавливаем все воспроизводимые, кроме unstoppable
        if (stopAnother)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] != null && sources[i].isPlaying && !isUnstoppable[i])
                {
                    sources[i].Stop();
                    sources[i].clip = null;
                    isUnstoppable[i] = false;
                }
            }
        }

        int index = GetNextSourceIndex();
        if (index == -1)
        {
            Debug.LogWarning($"[AudioService] Нет доступного источника для звука: {filename}");
            return;
        }

        var source = sources[index];
        source.clip = clip;
        source.volume = volume;
        isUnstoppable[index] = unstoppable;
        source.Play();
    }

    private int GetNextSourceIndex()
    {
        // Сначала пробуем найти свободный источник
        for (int i = 0; i < sources.Length; i++)
        {
            int idx = (nextSourceIndex + i) % sources.Length;
            if (!sources[idx].isPlaying)
            {
                nextSourceIndex = (idx + 1) % sources.Length;
                return idx;
            }
        }

        // Если свободных нет — можно переиспользовать прерываемые (не unstoppable)
        for (int i = 0; i < sources.Length; i++)
        {
            int idx = (nextSourceIndex + i) % sources.Length;
            if (!isUnstoppable[idx])
            {
                // Прерываем текущий звук и используем этот источник
                sources[idx].Stop();
                sources[idx].clip = null;
                isUnstoppable[idx] = false;
                nextSourceIndex = (idx + 1) % sources.Length;
                return idx;
            }
        }

        // Все источники заняты и защищены (unstoppable)
        return -1;
    }
}