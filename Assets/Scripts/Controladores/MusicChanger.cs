using UnityEngine;

/// <summary>
/// Muda a música via trigger de collider ou ao iniciar a cena.
/// Requer AudioController no projeto.
/// </summary>
public class MusicChanger : MonoBehaviour
{
    [Header("Clip")]
    [SerializeField] private AudioClip musicClip;

    [Header("Comportamento")]
    [SerializeField] private bool stopMusic = false;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool playOnTriggerEnter = true;
    [SerializeField] private bool useFade = true;
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Tag do objeto que ativa o trigger. Deixe vazio para qualquer objeto.")]
    [SerializeField] private string triggerTag = "Player";

    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (stopMusic)
        {
            AudioController.Instance.StopMusic();
        }
        if (playOnStart)
            TryChangeMusic();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!playOnTriggerEnter) return;
        if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag)) return;
        TryChangeMusic();
    }

    // ─────────────────────────────────────────────────────────────

    private void TryChangeMusic()
    {
        if (musicClip == null)
        {
            Debug.LogWarning($"[MusicChanger] Nenhum AudioClip atribuído em {gameObject.name}.");
            return;
        }

        var ac = AudioController.Instance;
        if (ac == null)
        {
            Debug.LogError("[MusicChanger] AudioController.Instance não encontrado na cena.");
            return;
        }

        // Evita reiniciar a mesma música se já estiver tocando
        if (IsSameMusicPlaying())
        {
            Debug.Log($"[MusicChanger] '{musicClip.name}' já está tocando. Nenhuma mudança feita.");
            return;
        }

        if (useFade)
            ac.PlayMusicWithFade(musicClip, fadeDuration);
        else
            ac.PlayMusic(musicClip);

        Debug.Log($"[MusicChanger] Música mudada para '{musicClip.name}'.");
    }

    /// <summary>
    /// Compara o clip atual do musicSource com o clip desejado.
    /// Acessa o AudioSource de música via reflexão para não poluir o AudioController.
    /// </summary>
    private bool IsSameMusicPlaying()
    {
        // Busca o AudioSource filho chamado "MusicSource" no AudioController
        var acTransform = AudioController.Instance.transform;
        foreach (Transform child in acTransform)
        {
            var src = child.GetComponent<AudioSource>();
            if (src != null && child.name == "MusicSource")
                return src.isPlaying && src.clip == musicClip;
        }
        return false;
    }
}
