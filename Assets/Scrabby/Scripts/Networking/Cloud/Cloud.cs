using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrabby.Configuration;
using Scrabby.ScriptableObjects;
using Scrabby.State;
using Scrabby.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scrabby.Networking
{
    public class Cloud : MonoSingleton<Cloud>
    {
        public string apiVersion = "v1";

        private string _runId;

        private void Start()
        {
            if (!IsCloudSession())
            {
                Destroy(gameObject);
            }

            Load();
        }

        private string GetUrl(string endpoint)
        {
#if UNITY_EDITOR
            return "http://localhost:3000/api/" + apiVersion + "/" + endpoint;
#else
            return "https://sim.soonerrobotics.org/api/" + apiVersion + "/" + endpoint;
#endif
        }

        private static string GetAuthorizationToken()
        {
            return CommandLineArgs.Get("auth");
        }

        private static string GetChallengeId()
        {
            return CommandLineArgs.Get("challenge");
        }

        private static string GetServerToken()
        {
            return CommandLineArgs.Get("server_token");
        }

        private Dictionary<string, string> GetHeaders()
        {
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Authorization", GetAuthorizationToken() },
                { "Server-Authorization", GetServerToken() },
                { "Run", _runId ?? string.Empty }
            };
            return headers;
        }

        private async Task<Challenge> GetChallenge()
        {
            var headers = GetHeaders();
            var url = GetUrl("challenges/" + GetChallengeId());
            var result = await HttpRequest<Challenge>.Get(url, headers);
            return result.IsSuccess ? result.Data : null;
        }

        public async Task Log(LogLevel level, string message, params StringKvp[] data)
        {
            var headers = GetHeaders();
            var url = GetUrl($"run/{_runId}/log");
            var body = new Dictionary<string, string>
            {
                { "level", level.ToString() },
                { "message", message },
                { "data", JsonConvert.SerializeObject(data) }
            };

            var result = await HttpRequest<object>.Post(url, headers, JsonConvert.SerializeObject(body));
            if (!result.IsSuccess)
            {
                Debug.LogWarning($"Failed to log message: {result.Error}");
            }
        }

        public async Task SetRunStatus(RunStatus status)
        {
            var headers = GetHeaders();
            var url = GetUrl($"run/{_runId}/status");
            var statusString = status.ToString().ToLower();
            var body = new Dictionary<string, string>
            {
                { "status", statusString },
            };

            var result = await HttpRequest<object>.Post(url, headers, JsonConvert.SerializeObject(body));
            if (!result.IsSuccess)
            {
                Debug.LogError($"Failed to set run status to {statusString}: {result.Error}");
            }
        }

        private async Task CreateRun()
        {
            var url = GetUrl("run");
            var body = new Dictionary<string, string>
            {
                { "challenge_id", GetChallengeId() }
            };
            var result = await HttpRequest<JObject>.Post(url, GetHeaders(), JsonConvert.SerializeObject(body));
            if (!result.IsSuccess)
            {
                Debug.LogError($"Failed to create run: {result.Error}");
                SceneHelper.Quit();
            }

            _runId = result.Data["id"]?.ToString();
            if (_runId == null)
            {
                Debug.LogError($"Failed to create run: {result.Error}");
                SceneHelper.Quit();
            }
        }

        private async void InitializeChallenge(Challenge challenge)
        {
            if (challenge == null)
            {
                await SetRunStatus(RunStatus.Errored);
                await Log(LogLevel.Critical, "INIT_FAILED/BAD_CHALLENGE");
                SceneHelper.Quit();
                return;
            }

            var robot = ScrabbyState.instance.GetRobotById(challenge.robotId);
            var map = ScrabbyState.instance.GetMapById(challenge.mapId);

            if (robot == null)
            {
                await SetRunStatus(RunStatus.Errored);
                await Log(LogLevel.Critical, "INIT_FAILED/BAD_ROBOT");
                SceneHelper.Quit();
            }

            if (map == null)
            {
                await SetRunStatus(RunStatus.Errored);
                await Log(LogLevel.Critical, "INIT_FAILED/BAD_MAP");
                SceneHelper.Quit();
            }

            Robot.Active = robot;
            Map.Active = map;

            ScrabbyState.instance.movementEnabled = false;
            ConfigManager.RobotConfig.EnsureRobot(robot);
            SceneManager.sceneLoaded += async (_, _) =>
            {
                ScrabbyState.instance.movementEnabled = true;
                await SetRunStatus(RunStatus.Simulating);
                Invoke(nameof(OnTimeout), challenge.timeout / 1000.0f);
            };
            SceneHelper.Switch(map.sceneIndex);

            await Log(LogLevel.Info, "INITIALIZED");
        }

        private async void OnTimeout()
        {
            await SetRunStatus(RunStatus.Timeout);
            SceneHelper.Quit();
        }

        private async void Load()
        {
            if (!IsCloudSession())
            {
                return;
            }

            await CreateRun();
            await SetRunStatus(RunStatus.Initializing);
            var challenge = await GetChallenge();
            if (challenge == null)
            {
                await SetRunStatus(RunStatus.Errored);
                await Log(LogLevel.Critical, "LOAD_FAILED/CHALLENGE_NOT_FOUND");
                SceneHelper.Quit();
            }

            InitializeChallenge(challenge);
        }

        private static bool IsCloudSession()
        {
#if UNITY_EDITOR
            return EditorPrefs.GetBool("SCR/Is Cloud Session", false);
#else
            return Application.isBatchMode;
#endif
        }
    }

    public enum RunStatus
    {
        Created,
        Initializing,
        Simulating,
        Finished,
        Timeout,
        Errored
    }
}