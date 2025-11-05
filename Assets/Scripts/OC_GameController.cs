using UnityEngine;
using TMPro;

public class OC_GameController : MonoBehaviour
{
    public OC_Board board;
    public TMP_Text timeText;
    public TMP_Text statusText;
    public TMP_Text objectiveText;

    public GameObject instruction;
    public GameObject levelclear;

    public float timeLimit = 30f;
    float timeLeft;
    bool running;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        board.onChanged = CheckWin;
        instruction.SetActive(true);
        levelclear.SetActive(false);



    }

    // Update is called once per frame
    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        timeText.text = "Time: " + timeLeft.ToString("0");

        if (timeLeft <= 0f)
        {
            Lose();
        }
    }

    public void Restart()
    {
        StartLevel(board.levelIndex);
    }

    public void NextLevel()
    {
        levelclear.SetActive(false);
        board.levelIndex++;
        if (board.levelIndex >= board.levelData.levels.Count)
        {
            board.levelIndex = 0;
        }

        StartLevel(board.levelIndex);
    }

    public void StartGame()
    {
        instruction.SetActive(false);
        StartLevel(0);
    }

    void StartLevel(int index)
    {
        statusText.text = "";
        if (objectiveText) objectiveText.text = "Clear the path from S to G. Tap small rocks, drag big rocks.";
        board.levelIndex = index;
        board.Build();
        timeLeft = timeLimit;
        running = true;

        board.ShowGuidance();
    }

    void CheckWin()
    {
        
        if (board.PathExists())
            Win();
    }

    public void Win()
    {
        running = false;
        levelclear.SetActive(true);
        statusText.text = "Path Cleared!";
    }

    void Lose()
    {
        running = false;
        levelclear.SetActive(true);
        statusText.text = "Sorry, Time Up!";
    }
}
