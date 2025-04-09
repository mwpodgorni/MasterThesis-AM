using UnityEngine;

public class StateManager : MonoBehaviour
{

    public static StateManager Instance { get; private set; }
    private GameState currentGameState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentGameState = GameState.MainMenu;
    }
}

public enum GameState
{
    MainMenu,
    Intro,
}
