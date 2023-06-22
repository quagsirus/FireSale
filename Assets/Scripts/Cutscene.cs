using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cutscene : MonoBehaviour
{
    public string[] dialogueLines;
    public char[] characterIndicators;
    public Sprite[] characterSprites;

    private GameActions _actions;

    private TextMeshProUGUI _dialogueTextBox;

    private int _nextPosition;
    private SpriteRenderer _portraitSpriteRenderer;

    private void Awake()
    {
        _actions = new GameActions();
        _actions.cutscene.displayNextLine.performed += ParseNextLine;

        _dialogueTextBox = GetComponent<TextMeshProUGUI>();
        _portraitSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _actions.cutscene.Enable();
    }

    private void OnDisable()
    {
        _actions.cutscene.Disable();
    }

    private void ParseNextLine(InputAction.CallbackContext context)
    {
        var nextLine = dialogueLines[_nextPosition];
        var nextLineSpriteCode = nextLine[0];
        var nextLineSpriteIndex = Array.IndexOf(characterIndicators, nextLineSpriteCode);
        var nextLineSprite = characterSprites[nextLineSpriteIndex];

        _portraitSpriteRenderer.sprite = nextLineSprite;
        _dialogueTextBox.text = nextLine[1..];

        _nextPosition++;
    }
}