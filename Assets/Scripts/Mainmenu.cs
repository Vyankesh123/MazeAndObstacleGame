using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    [Header("Scene Names (must match Build Settings)")]
    [SerializeField] string mazeSceneName = "MazeNavigation";
    [SerializeField] string obstacleSceneName = "ObstacleClearing";

    public void PlayMaze()
    {
        SceneManager.LoadScene(mazeSceneName);
    }

    public void PlayObstacle()
    {
        SceneManager.LoadScene(obstacleSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
