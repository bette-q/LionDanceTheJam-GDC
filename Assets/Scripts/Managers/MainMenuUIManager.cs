using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private string startSceneName;

    private Button _startButton;
    private Button _settingButton;
    private Button _exitButton;
    private GameObject _menuPanel;
    private GameObject _settingPanel;
    private GameObject _introPanel;
    private readonly List<GameObject> _introScreens = new List<GameObject>();
    private bool _isIntroPlaying;
    private int _introIndex;

    private Slider _masterSlider;
    private Slider _sfxSlider;
    private Slider _bgmSlider;

    private void Awake()
    {
        AutoHook();
        WireEvents();
        SyncSlidersFromAudio();

        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.PlayMenuBgm();
        }

        SetIntroVisible(false);
    }

    private void AutoHook()
    {
        Transform startBtn = FindByNameInScene("StartBtn");
        Transform settingBtn = FindByNameInScene("SettingBtn");
        Transform exitBtn = FindByNameInScene("ExitBtn");
        Transform menuPanel = FindByNameInScene("MenuPanel");
        Transform settingPanel = FindByNameInScene("SettingPanel");
        Transform introPanel = FindByNameInScene("IntroPanel");

        if (startBtn != null) _startButton = startBtn.GetComponent<Button>();
        if (settingBtn != null) _settingButton = settingBtn.GetComponent<Button>();
        if (exitBtn != null) _exitButton = exitBtn.GetComponent<Button>();
        if (menuPanel != null) _menuPanel = menuPanel.gameObject;
        if (settingPanel != null) _settingPanel = settingPanel.gameObject;
        if (introPanel != null) _introPanel = introPanel.gameObject;

        _introScreens.Clear();
        if (introPanel != null)
        {
            for (int i = 0; i < introPanel.childCount; i++)
            {
                Transform child = introPanel.GetChild(i);
                _introScreens.Add(child.gameObject);
            }
        }

        Transform master = FindByNameInScene("Master");
        Transform sfx = FindByNameInScene("SFX");
        Transform bgm = FindByNameInScene("BGM");

        if (master != null)
        {
            Transform slider = master.Find("Slider");
            if (slider != null) _masterSlider = slider.GetComponent<Slider>();
        }

        if (sfx != null)
        {
            Transform slider = sfx.Find("Slider");
            if (slider != null) _sfxSlider = slider.GetComponent<Slider>();
        }

        if (bgm != null)
        {
            Transform slider = bgm.Find("Slider");
            if (slider != null) _bgmSlider = slider.GetComponent<Slider>();
        }
    }

    private void Update()
    {
        if (!_isIntroPlaying)
        {
            return;
        }

        if (IsAdvanceIntroPressed())
        {
            AdvanceIntro();
        }
    }

    private void WireEvents()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _startButton.onClick.AddListener(OnStartClicked);
        }

        if (_settingButton != null)
        {
            _settingButton.onClick.RemoveListener(OnSettingClicked);
            _settingButton.onClick.AddListener(OnSettingClicked);
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.RemoveListener(OnExitClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
        }

        if (_masterSlider != null)
        {
            _masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            _masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (_bgmSlider != null)
        {
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
            _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }
    }

    private void SyncSlidersFromAudio()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null)
        {
            return;
        }

        if (_masterSlider != null) _masterSlider.SetValueWithoutNotify(audio.GetMasterVolume());
        if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(audio.GetSfxVolume());
        if (_bgmSlider != null) _bgmSlider.SetValueWithoutNotify(audio.GetBgmVolume());
    }

    private void OnStartClicked()
    {
        if (string.IsNullOrWhiteSpace(startSceneName))
        {
            return;
        }

        if (_introScreens.Count > 0)
        {
            _isIntroPlaying = true;
            _introIndex = 0;
            ShowIntroScreen(_introIndex);
            return;
        }

        StartGameScene();
    }

    private void OnSettingClicked()
    {
        if (_settingPanel != null)
        {
            _settingPanel.SetActive(true);
        }
    }

    private void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.SetMasterVolume(value);
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.SetSfxVolume(value);
        }
    }

    private void OnBgmVolumeChanged(float value)
    {
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.SetBgmVolume(value);
        }
    }

    private static Transform FindByNameInScene(string targetName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] roots = activeScene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindByNameRecursive(roots[i].transform, targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Transform FindByNameRecursive(Transform root, string targetName)
    {
        if (root.name == targetName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindByNameRecursive(root.GetChild(i), targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void AdvanceIntro()
    {
        _introIndex++;
        if (_introIndex >= _introScreens.Count)
        {
            _isIntroPlaying = false;
            StartGameScene();
            return;
        }

        ShowIntroScreen(_introIndex);
    }

    private void ShowIntroScreen(int index)
    {
        if (_introScreens.Count == 0)
        {
            return;
        }

        SetIntroVisible(true);
        for (int i = 0; i < _introScreens.Count; i++)
        {
            _introScreens[i].SetActive(i == index);
        }
    }

    private void SetIntroVisible(bool visible)
    {
        if (_introPanel != null)
        {
            _introPanel.SetActive(visible);
        }
    }

    private bool IsAdvanceIntroPressed()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        var pads = Gamepad.all;
        for (int i = 0; i < pads.Count; i++)
        {
            if (pads[i] != null && pads[i].buttonSouth.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void StartGameScene()
    {
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(false);
        }

        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.PlayGameBgm();
        }

        SceneManager.LoadScene(startSceneName);
    }
}
