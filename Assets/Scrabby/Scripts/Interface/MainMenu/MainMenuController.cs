using System;
using System.Linq;
using Scrabby.Networking;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Scrabby.Interface
{
    public class MainMenuController : MonoBehaviour
    {
        private static string _selectedRobot;
        private static string _selectedCategory;
        private static string _selectedMap;

        [Header("Home")]
        public TMP_Dropdown robotDropdown;
        public TMP_Dropdown categoryDropdown;
        public TMP_Dropdown mapDropdown;

        public Button playButton;
        public Button quitButton;

        [Header("Settings")]
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown screenModeDropdown;
        public TMP_Text randomnessSeedText;
        public TMP_InputField randomnessSeedInput;
        public Toggle randomizeSeedToggle;
        public Button randomizeSeedButton;
        
        private void Start()
        {
            robotDropdown.ClearOptions();
            robotDropdown.AddOptions(ScrabbyState.Instance.robots.Select(r => r.name).ToList());
            robotDropdown.onValueChanged.AddListener(OnRobotSelected);

            categoryDropdown.ClearOptions();
            categoryDropdown.AddOptions(ScrabbyState.Instance.maps.Select(m => m.category).Distinct().ToList());
            categoryDropdown.onValueChanged.AddListener(OnCategorySelected);

            mapDropdown.ClearOptions();
            mapDropdown.AddOptions(ScrabbyState.Instance.maps.Select(m => m.name).ToList());
            mapDropdown.onValueChanged.AddListener(OnMapSelected);

            RestoreLastOptions();

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(Screen.resolutions.Select(r => r.width + "x" + r.height).Reverse().ToList());
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(o => o.text == Screen.currentResolution.width + "x" + Screen.currentResolution.height);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionSelected);

            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(Enum.GetNames(typeof(FullScreenMode)).ToList());
            screenModeDropdown.value = screenModeDropdown.options.FindIndex(o => o.text == Screen.fullScreenMode.ToString());
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeSelected);

            playButton.onClick.AddListener(OnPlay);
            quitButton.onClick.AddListener(OnQuit);

            if (ScrabbyState.Instance.randomizeSeed)
            {
                ScrabbyState.Instance.randomSeed = Random.Range(0, int.MaxValue);
            }

            randomnessSeedInput.text = ScrabbyState.Instance.randomSeed.ToString();
            randomnessSeedInput.onValueChanged.AddListener(OnRandomSeedChanged);
            OnRandomSeedChanged(randomnessSeedInput.text);
            
            randomizeSeedToggle.isOn = ScrabbyState.Instance.randomizeSeed;
            randomizeSeedToggle.onValueChanged.AddListener(OnRandomizeSeedToggle);
            
            randomizeSeedButton.onClick.AddListener(RandomizeSeed);
        }

        private void OnDestroy()
        {
            robotDropdown.onValueChanged.RemoveListener(OnRobotSelected);
            categoryDropdown.onValueChanged.RemoveListener(OnCategorySelected);
            mapDropdown.onValueChanged.RemoveListener(OnMapSelected);
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionSelected);
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeSelected);
            playButton.onClick.RemoveListener(OnPlay);
            quitButton.onClick.RemoveListener(OnQuit);
            randomnessSeedInput.onValueChanged.RemoveListener(OnRandomSeedChanged);
            randomizeSeedToggle.onValueChanged.RemoveListener(OnRandomizeSeedToggle);
        }

        private void OnPlay()
        {
            var robotName = robotDropdown.options[robotDropdown.value].text;
            var mapName = mapDropdown.options[mapDropdown.value].text;

            var robot = ScrabbyState.Instance.robots.Find(r => r.name == robotName);
            var map = ScrabbyState.Instance.maps.Find(m => m.name == mapName);

            if (robot == null || map == null)
            {
                Debug.LogError($"Robot or map not found: {robotName} {mapName}");
                return;
            }

            Map.Active = map;
            Robot.Active = robot;
            
            SceneManager.LoadScene(map.sceneIndex);
        }

        private static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnRobotSelected(int index)
        {
            _selectedRobot = robotDropdown.options[index].text;
        }

        private void OnCategorySelected(int index)
        {
            var category = categoryDropdown.options[index].text;
            var maps = ScrabbyState.Instance.maps.Where(m => m.category == category).ToList();
            _selectedCategory = category;

            mapDropdown.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
            mapDropdown.GetComponentInChildren<TMP_Dropdown>().AddOptions(maps.Select(m => m.name).ToList());
            if (_selectedMap == null)
            {
                _selectedMap = mapDropdown.options[0].text;
            }
            else
            {
                mapDropdown.value = mapDropdown.options.FindIndex(o => o.text == _selectedMap);
            }
            OnMapSelected(mapDropdown.value);
        }

        private void OnMapSelected(int index)
        {
            _selectedMap = mapDropdown.options[index].text;
        }

        private void OnResolutionSelected(int index)
        {
            var resolution = Screen.resolutions[index];
            var screenMode = (FullScreenMode)Enum.Parse(typeof(FullScreenMode), screenModeDropdown.options[index].text);
            Screen.SetResolution(resolution.width, resolution.height, screenMode);
        }

        private void OnScreenModeSelected(int index)
        {
            var screenMode = (FullScreenMode)Enum.Parse(typeof(FullScreenMode), screenModeDropdown.options[index].text);
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, screenMode);
        }

        private void OnRandomSeedChanged(string value)
        {
            if (!int.TryParse(value, out var seed))
            {
                return;
            }
            
            ScrabbyState.Instance.randomSeed = seed;
            Random.InitState(seed);
            var direction = Random.Range(0, 2) == 0 ? "N" : "S";
            randomnessSeedText.text = $"Seed ({direction})";
        }

        private void RestoreLastOptions()
        {
            if (_selectedRobot == null)
            {
                _selectedRobot = robotDropdown.options[0].text;
            }
            else
            {
                robotDropdown.value = robotDropdown.options.FindIndex(o => o.text == _selectedRobot);
            }
            OnRobotSelected(robotDropdown.value);

            if (_selectedCategory == null)
            {
                _selectedCategory = categoryDropdown.options[0].text;
            }
            else
            {
                categoryDropdown.value = categoryDropdown.options.FindIndex(o => o.text == _selectedCategory);
            }
            OnCategorySelected(categoryDropdown.value);

            if (_selectedMap == null)
            {
                _selectedMap = mapDropdown.options[0].text;
            }
            else
            {
                mapDropdown.value = mapDropdown.options.FindIndex(o => o.text == _selectedMap);
            }
            OnMapSelected(mapDropdown.value);
        }
        
        private void OnRandomizeSeedToggle(bool value)
        {
            ScrabbyState.Instance.randomizeSeed = value;
            if (!value)
            {
                return;
            }
            
            RandomizeSeed();
        }

        private void RandomizeSeed()
        {
            ScrabbyState.Instance.randomSeed = Random.Range(0, int.MaxValue);
            randomnessSeedInput.text = ScrabbyState.Instance.randomSeed.ToString();   
        }
    }
}
