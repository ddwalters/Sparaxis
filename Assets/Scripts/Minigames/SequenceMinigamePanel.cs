using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SequenceMinigamePanel : MonoBehaviour
{
    [SerializeField] private GameObject arrowTilePrefab;
    [SerializeField] private Transform tileContainer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    private static readonly ArrowDir[] Directions = { ArrowDir.Up, ArrowDir.Down, ArrowDir.Left, ArrowDir.Right };
    private static readonly float[] Rotations = { 0f, 180f, 90f, -90f };
    private static readonly Color Pressed = new Color(0.35f, 0.35f, 0.35f);

    private ArrowDir[] _sequence;
    private Image[] _tileImages;
    private int _progress;
    private int _totalScore;
    private float _elapsed;

    public Action OnComplete;

    private const float TimerDuration = 15f;

    private void Update()
    {
        _elapsed -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(_elapsed).ToString();

        if (_elapsed <= 0f)
        {
            timerText.text = "0";
            OnComplete?.Invoke();
            return;
        }

        ArrowDir? pressed = GetPressedDirection();
        if (pressed == null) return;

        if (pressed == _sequence[_progress])
        {
            _tileImages[_progress].color = Pressed;
            _progress++;

            if (_progress >= _sequence.Length)
            {
                _totalScore += ScoreForSequence(_sequence.Length);
                scoreText.text = _totalScore.ToString();
                NewSequence();
            }
        }
        else
        {
            NewSequence();
        }
    }

    public void StartFresh()
    {
        _totalScore = 0;
        _elapsed = TimerDuration;
        scoreText.text = "0";
        NewSequence();
    }

    private void NewSequence()
    {
        GenerateSequence();
        SpawnTiles();
        _progress = 0;
    }

    private void GenerateSequence()
    {
        int length = UnityEngine.Random.Range(5, 13);
        _sequence = new ArrowDir[length];
        for (int i = 0; i < length; i++)
            _sequence[i] = Directions[UnityEngine.Random.Range(0, Directions.Length)];
    }

    private void SpawnTiles()
    {
        foreach (Transform child in tileContainer) Destroy(child.gameObject);
        _tileImages = new Image[_sequence.Length];

        for (int i = 0; i < _sequence.Length; i++)
        {
            GameObject tile = Instantiate(arrowTilePrefab, tileContainer);
            Image img = tile.GetComponent<Image>();
            img.color = Color.white;
            tile.transform.localEulerAngles = new Vector3(0f, 0f, Rotations[(int)_sequence[i]]);
            _tileImages[i] = img;
        }
    }

    private static int ScoreForSequence(int length)
    {
        int extra = Mathf.Max(0, length - 6);
        return Mathf.RoundToInt(100f * Mathf.Pow(1.05f, extra));
    }

    private static ArrowDir? GetPressedDirection()
    {
        var kb = Keyboard.current;
        if (kb.upArrowKey.wasPressedThisFrame    || kb.wKey.wasPressedThisFrame) return ArrowDir.Up;
        if (kb.downArrowKey.wasPressedThisFrame  || kb.sKey.wasPressedThisFrame) return ArrowDir.Down;
        if (kb.leftArrowKey.wasPressedThisFrame  || kb.aKey.wasPressedThisFrame) return ArrowDir.Left;
        if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) return ArrowDir.Right;
        return null;
    }

    private enum ArrowDir { Up, Down, Left, Right }
}
