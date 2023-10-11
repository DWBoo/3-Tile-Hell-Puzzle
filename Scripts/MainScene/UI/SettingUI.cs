using Assets.Scripts.Internal;
using Assets.Scripts.Internal.Audio;
using Assets.Scripts.Scene.PuzzleScene;
using DataContainer.Generated;
using Dignus.Unity;
using Dignus.Unity.Extensions.DependencyInjection;
using TemplateContainers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Scene.MainScene.UI
{
    [PrefabPath(Consts.Path.MainScene)]
    public class SettingUI : UIItem
    {
        //메인 화면에서 세팅 팝업의 나가기 버튼.
        [SerializeField]
        private Button _exitBottomButton;
        //인게임에서 세팅 팝업의 나가기 버튼.
        [SerializeField]
        private Button _exitTopButton;
        [SerializeField]
        private Button _giveUpButton;
        [SerializeField]
        private TextMeshProUGUI[] _uiTextMeshes;
        [SerializeField]
        private Toggle _sfxToggle;
        [SerializeField]
        private Toggle _bgmToggle;

        private UnityAction onExit;
        private bool isInitialized;
        private int[] localizedStringIds = { 30011, 30012, 30013, 30014, 30015 };
        private PlayerManager _playerManager;
        private void Awake()
        {
            _playerManager = DignusUnityServiceContainer.Resolve<PlayerManager>();
            _sfxToggle.isOn = _playerManager.GetSFXVolume() == 1;
            _bgmToggle.isOn = _playerManager.GetBGMVolume() == 1;
        }
        public void Init(MainSceneController sceneController, UnityAction onExitCallback)
        {
            onExit = onExitCallback;
            _exitTopButton.gameObject.SetActive(false);
            _giveUpButton.gameObject.SetActive(false);
            _exitBottomButton.gameObject.SetActive(true);
            if (isInitialized)
            {
                return;
            }

            for (int i = 0; i < _uiTextMeshes.Length; i++)
            {
                var titleStringTemplate = TemplateContainer<StringTemplate>.Find(localizedStringIds[i]);
                if (titleStringTemplate.Invalid())
                {
                    throw new System.Exception($"not found template id : {localizedStringIds[i]}");
                }

                _uiTextMeshes[i].text = titleStringTemplate.GetString(sceneController.PlayerManager.GetLanguageType());
            }
            isInitialized = true;
        }

        public void Init(PuzzleSceneController sceneController, UnityAction onExitCallback)
        {
            onExit = onExitCallback;
            _exitBottomButton.gameObject.SetActive(false);
            _exitTopButton.gameObject.SetActive(true);
            _giveUpButton.gameObject.SetActive(true);
            if (isInitialized)
            {
                return;
            }

            for (int i = 0; i < _uiTextMeshes.Length; i++)
            {
                var titleStringTemplate = TemplateContainer<StringTemplate>.Find(localizedStringIds[i]);
                if (titleStringTemplate.Invalid())
                {
                    throw new System.Exception($"not found template id : {localizedStringIds[i]}");
                }

                _uiTextMeshes[i].text = titleStringTemplate.GetString(sceneController.PlayerManager.GetLanguageType());
            }
            isInitialized = true;
        }
        public void OnGiveUpButtonClick()
        {
            DignusUnitySceneManager.Instance.LoadScene(SceneType.MainScene);
            onExit?.Invoke();
        }

        public void OnExitButtonClick()
        {
            _playerManager.Save();
            onExit?.Invoke();
        }

        public void OnBgmToggleValueChanged(Toggle toggle)
        {
            if (!toggle.isOn)
            {
                _playerManager.SetBGMVolume(0);
                AudioManager.Instance.GetBGMPlayer().SetVolume(0);
                return;
            }
            _playerManager.SetBGMVolume(1);
            AudioManager.Instance.GetBGMPlayer().SetVolume(1);
        }

        public void OnSfxToggleValueChanged(Toggle toggle)
        {

            if (!toggle.isOn)
            {
                _playerManager.SetSFXVolume(0);
                AudioManager.Instance.GetSFXPlayer().SetVolume(0);
                return;
            }
            _playerManager.SetSFXVolume(1);
            AudioManager.Instance.GetSFXPlayer().SetVolume(1);
        }
    }
}
