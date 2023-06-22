using System;
using TMPro;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public string[] dialogueLines;
    public char[] characterIndicators;
    public Sprite[] characterSprite;

    private int _nextPosition;

    private TextMeshProUGUI _dialogueTextBox;

    private void Start()
    {
        _dialogueTextBox = GetComponent<TextMeshProUGUI>();
    }

    private void ParseNextLine()
    {
        var nextLine = dialogueLines[_nextPosition];
        _dialogueTextBox.text = dialogueLines[_nextPosition];

    }
}