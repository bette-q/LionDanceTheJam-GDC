using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private string startSceneName;

    private Button _startButton;
    private Button _settingButton;
    private Button _exitButton;
    private GameObject _settingPanel;

    private Slider _masterSlider;
    private Slider _sfxSlider;
    private Slider _bgmSlider;

    private void Awake()
    {
        AutoHook();
        WireEvents();
        SyncSlidersFromAudio();
    }

    private void AutoHook()
    {
        Transform startBtn = FindByNameInScene("StartBtn");
        Transform settingBtn = FindByNameInScene("SettingBtn");
        Transform exitBtn = FindByNameInScene("ExitBtn");
        Transform settingPanel = FindByNameInScene("SettingPanel");

        if (startBtn != null) _startButton = startBtn.GetComponent<Button>();
        if (settingBtn != null) _settingButton = settingBtn.GetComponent<Button>();
        if (exitBtn != null) _exitButton = exitBtn.GetComponent<Button>();
        if (settingPanel != null) _settingPanel = settingPanel.gameObject;

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

        SceneManager.LoadScene(startSceneName);
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
}
