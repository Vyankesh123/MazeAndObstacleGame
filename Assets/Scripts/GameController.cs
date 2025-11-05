using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public MazeBuilder maze;
    public PlayerController player;
    public LevelData levels;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timeText;

    float time;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maze.levelData = levels;
        maze.Build();
        player.maze = maze;
        player.SendMessage("Start");
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        timeText.text = "Time: " + time.ToString("0.0");
    }

    public void Restart()
    {
        time = 0;
        maze.Build();
        player.SendMessage("Start");
        UpdateUI();
    }
   

    public void NextLevel()
    {
        maze.levelIndex++;

        if (maze.levelIndex >= levels.levels.Count)
            maze.levelIndex = 0;  // loop back

        LoadCurrentLevel();
    }

    void LoadCurrentLevel()
    {
        time = 0;
        maze.Build();
        player.maze = maze;
        player.SendMessage("Start");
        UpdateUI();
    }

    void UpdateUI()
    {
        levelText.text = "Level: " + (maze.levelIndex + 1);
    }
}
