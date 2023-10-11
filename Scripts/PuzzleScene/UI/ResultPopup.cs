using Assets.Scripts.Internal.Character;
using DataContainer;
using DataContainer.Generated;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GameContents;
using TemplateContainers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResultPopup : MonoBehaviour
{
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Button _retryButton;
    [SerializeField]
    private TextMeshProUGUI _resultText;
    [SerializeField]
    private TextMeshProUGUI _messageText;
    [SerializeField]
    private TextMeshProUGUI _exitButtonText;
    [SerializeField]
    private TextMeshProUGUI _retryButtonText;
    [SerializeField]
    private TextMeshProUGUI _coinText;
    [SerializeField]
    private Image _titleFrameImage;
    [SerializeField]
    private Sprite _failFrameSprite;
    [SerializeField]
    private Sprite _clearFrameSprite;
    [SerializeField]
    private Image _haloEffectImage;
    [SerializeField]
    private UICharacterHandler _uiCharacterHandler;

    private TweenerCore<Quaternion, Vector3, QuaternionOptions> _haloEffectTweener;

    public void Init(string result, PuzzleBoard puzzleBoard, LanguageType languageType,
        UnityAction onExit, UnityAction onRetry)
    {
        switch (result)
        {
            case "GameOver":
                {
                    var resultTemplate = TemplateContainer<StringTemplate>.Find(40002);
                    if (resultTemplate.Invalid())
                    {
                        throw new System.Exception($"not found template id : {40002}");
                    }

                    var resultText = resultTemplate.GetString(languageType);
                    _resultText.text = resultText;

                    int messageId = puzzleBoard.StageTemplate.CharacterTemplate.FailTalkIDRef.Id;
                    var messageTemplate = TemplateContainer<StringTemplate>.Find(messageId);
                    if (messageTemplate.Invalid())
                    {
                        throw new System.Exception($"not found template id : {messageId}");
                    }

                    var messageText = messageTemplate.GetString(languageType);
                    _messageText.text = messageText;
                    _titleFrameImage.sprite = _failFrameSprite;
                    _retryButton.gameObject.SetActive(true);
                }
                break;

            case "GameClear":
                {
                    _haloEffectImage.gameObject.SetActive(true);
                    _haloEffectTweener = _haloEffectImage.transform.DOLocalRotate(Vector3.forward * 360f, 15f)
                        .SetRelative(true).SetLoops(-1).SetEase(Ease.Linear);

                    var resultTemplate = TemplateContainer<StringTemplate>.Find(40001);
                    if (resultTemplate.Invalid())
                    {
                        throw new System.Exception($"not found template id : {40001}");
                    }

                    var resultText = resultTemplate.GetString(languageType);
                    _resultText.text = resultText;

                    int messageId = puzzleBoard.StageTemplate.CharacterTemplate.SuccessTalkIDRef.Id;
                    var messageTemplate = TemplateContainer<StringTemplate>.Find(messageId);
                    if (messageTemplate.Invalid())
                    {
                        throw new System.Exception($"not found template id : {messageId}");
                    }

                    var messageText = messageTemplate.GetString(languageType);
                    _messageText.text = messageText;
                    _titleFrameImage.sprite = _clearFrameSprite;
                    _retryButton.gameObject.SetActive(false);
                }
                break;
        }

        var ExitButtonTextTemplate = TemplateContainer<StringTemplate>.Find(40003);
        if (ExitButtonTextTemplate.Invalid())
        {
            throw new System.Exception($"not found template id : {40003}");
        }

        var buttonText = ExitButtonTextTemplate.GetString(languageType);
        _exitButtonText.text = buttonText;

        var RetryButtonTextTemplate = TemplateContainer<StringTemplate>.Find(40004);
        if (RetryButtonTextTemplate.Invalid())
        {
            throw new System.Exception($"not found template id : {40004}");
        }
        buttonText = RetryButtonTextTemplate.GetString(languageType);
        _retryButtonText.text = buttonText;

        _coinText.text = string.Format("+{0:N0}", puzzleBoard.GetAcquireGold());

        _exitButton.onClick.AddListener(() =>
        {
            _exitButton.onClick.RemoveAllListeners();
            _retryButton.onClick.RemoveAllListeners();
            _haloEffectTweener.Kill();
            _haloEffectImage.gameObject.SetActive(false);
            gameObject.SetActive(false);
            onExit?.Invoke();
        });

        _retryButton.onClick.AddListener(() =>
        {
            _retryButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
            _haloEffectTweener.Kill();
            _haloEffectImage.gameObject.SetActive(false);
            gameObject.SetActive(false);

            onRetry?.Invoke();
        });

        _uiCharacterHandler.Init(puzzleBoard.StageTemplate.CharacterTemplate.CharacterImage);
        gameObject.SetActive(true);
    }
}
