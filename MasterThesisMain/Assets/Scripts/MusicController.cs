using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicController : MonoBehaviour
{
    AudioSource[] sources;
    void Start()
    {
        sources = GetComponents<AudioSource>();
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MainMenu" || scene == "StageOne")
            sources[0].Play();
        else
            sources[1].Play();
    }
}
