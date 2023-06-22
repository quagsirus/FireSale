using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    public string[] dialogueLines;
    public char[] characterIndicators;
    public Sprite[] characterSprites;

    private GameActions _actions;

    private TextMeshProUGUI _dialogueText;

    private int _nextPosition;
    public Image portraitImage;

    private void Awake()
    {
        _actions = new GameActions();
        _actions.cutscene.displayNextLine.performed += ParseNextLine;
        _actions.cutscene.skip.performed += SkipCutscene;

        _dialogueText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        ParseNextLine(new InputAction.CallbackContext());
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
        if (_nextPosition >= dialogueLines.Length)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }
        var nextLine = dialogueLines[_nextPosition];
        var nextLineSpriteCode = nextLine[0];
        var nextLineSpriteIndex = Array.IndexOf(characterIndicators, nextLineSpriteCode);
        var nextLineSprite = characterSprites[nextLineSpriteIndex];

        portraitImage.sprite = nextLineSprite;
        _dialogueText.text = nextLine[1..];

        _nextPosition++;
    }

    private static void SkipCutscene(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}