using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public GameStage CurrentStage { get; private set; }
    public bool MiniGame1Solved { get; private set; }
    public bool MiniGame2Solved { get; private set; }
    public bool MiniGame3Solved { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentStage = GameStage.Tutorial;
    }

    public void MarkMiniGameSolved(int miniGameIndex)
    {
        switch (miniGameIndex)
        {
            case 1:
                MiniGame1Solved = true;
                CurrentStage = GameStage.MiniGame2;
                StageOneController.Instance.NetworkController().ResetNetwork();
                break;
            case 2:
                MiniGame2Solved = true;
                CurrentStage = GameStage.MiniGame3;
                StageOneController.Instance.NetworkController().EnableTraining();
                break;
            case 3:
                MiniGame3Solved = true;
                CurrentStage = GameStage.Completed;
                break;
        }
    }
}

public enum GameStage
{
    Tutorial,
    MiniGame1,
    MiniGame2,
    MiniGame3,
    Completed
}
