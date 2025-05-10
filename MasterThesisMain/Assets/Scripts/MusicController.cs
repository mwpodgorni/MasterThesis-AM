using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    AudioSource[] sources;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        sources = GetComponents<AudioSource>();
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MainMenu" || scene == "StageOne")
            PlayNNMusic();
        else
            PlayRLMusic();
    }

    public void PlayRLMusic()
    {
        sources[0].Stop();
        sources[1].Play();
    }

    public void PlayNNMusic()
    {
        sources[1].Stop();
        sources[0].Play();
    }

}
