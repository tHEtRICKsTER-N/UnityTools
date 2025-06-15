using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class InternetManager : MonoBehaviour
{
	private static readonly object _lock = new object();
	private static InternetManager instance;
	private Coroutine checkConnectionCoroutine;

	public static InternetManager Instance
	{
		get
		{
			lock (_lock)
			{
				if (instance == null)
				{
					var obj = new GameObject("InternetManager");
					instance = obj.AddComponent<InternetManager>();
					DontDestroyOnLoad(obj);
				}
			}
			return instance;
		}
	}

	[Tooltip("Time in seconds between checks")]
	public float checkInterval = 5f;

	[Tooltip("Maximum time in seconds for a request to wait for a response")]
	public float requestTimeout = 3f;

	[Tooltip("Number of retries before confirming disconnection")]
	public int retryCount = 3;

	public bool debugMode = false;

	[Tooltip("URLs to check for connectivity")]
	public List<string> checkUrls = new List<string> { "https://www.google.com", "https://www.bing.com" };

	private bool isInternetAvailable = true;
	private bool isCheckingConnection = false;
	private DateTime lastConnectedTime = DateTime.MinValue;
	private DateTime lastDisconnectedTime = DateTime.MinValue;

	public delegate void InternetConnectionHandler();
	public event InternetConnectionHandler OnInternetDisconnected;
	public event InternetConnectionHandler OnInternetConnected;

	private void Start()
	{
		LoadSettings();
		InitialInternetCheck();
		StartCheckingConnection();
    }

	private void OnDestroy()
	{
		if (instance == this)
		{
			StopCheckingConnection();
			instance = null;
		}
	}

	private void InitialInternetCheck()
	{
		TriggerInternetCheck();
	}

	private void StartCheckingConnection()
	{
		checkConnectionCoroutine = StartCoroutine(CheckConnectionRoutine());
	}

	private void StopCheckingConnection()
	{
		if (checkConnectionCoroutine != null)
		{
			StopCoroutine(checkConnectionCoroutine);
			checkConnectionCoroutine = null;
		}
	}

	private IEnumerator CheckConnectionRoutine()
	{
		while (true)
		{
			if (!isCheckingConnection)
			{
				TriggerInternetCheck();
			}
			yield return new WaitForSeconds(checkInterval);
		}
	}

	private async void TriggerInternetCheck()
	{
		lock (_lock)
		{
			if (isCheckingConnection)
				return;

			isCheckingConnection = true;
		}

		try
		{
			bool currentInternetStatus = await CheckInternetAsync();
			if (currentInternetStatus != isInternetAvailable)
			{
				isInternetAvailable = currentInternetStatus;

				if (isInternetAvailable)
				{
					Debug.Log("Internet is back.");
					OnInternetConnected?.Invoke();
					lastConnectedTime = DateTime.Now;
				}
				else
				{
                    Debug.Log("Internet is gone.");
                    OnInternetDisconnected?.Invoke();
                    lastDisconnectedTime = DateTime.Now;
                }
			}
		}
		catch (Exception ex)
		{
			LogError($"Error during internet check: {ex.Message}");
		}
		finally
		{
			lock (_lock)
			{
				isCheckingConnection = false;
			}
		}
	}

	private async Task<bool> CheckInternetAsync()
	{
		for (int retries = 0; retries < retryCount; retries++)
		{
			foreach (var url in checkUrls)
			{
				try
				{
					if (await CheckUrlAsync(url))
					{
						return true;
					}
				}
				catch (UnityWebRequestException ex)
				{
					LogWarning($"Failed to reach {url}: {ex.Message}");
				}
			}
			await Task.Delay((int)Math.Pow(2, retries) * 1000); // Exponential backoff
		}
		return false;
	}

	private async Task<bool> CheckUrlAsync(string url)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
			var operation = request.SendWebRequest();
			float startTime = Time.time;

			while (!operation.isDone)
			{
				if (Time.time - startTime > requestTimeout)
				{
					request.Abort();
					break;
				}
				await Task.Yield();
			}

			if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
			{
				throw new UnityWebRequestException(request.error);
			}

			return request.result == UnityWebRequest.Result.Success;
		}
	}

	public bool IsInternetAvailable()
	{
		lock (_lock)
		{
			return isInternetAvailable;
		}
	}

	public void SetCheckInterval(float interval)
	{
		checkInterval = interval;
		SaveSettings();
	}

	public void SetRequestTimeout(float timeout)
	{
		requestTimeout = timeout;
		SaveSettings();
	}

	public void SetRetryCount(int count)
	{
		retryCount = count;
		SaveSettings();
	}

	public void AddCheckUrl(string url)
	{
		if (!checkUrls.Contains(url))
		{
			checkUrls.Add(url);
			SaveSettings();
		}
	}

	public void RemoveCheckUrl(string url)
	{
		if (checkUrls.Remove(url))
		{
			SaveSettings();
		}
	}

	private void Log(string message)
	{
		if (debugMode)
		{
			Debug.Log($"{DateTime.Now}: {message}");
		}
	}

	private void LogWarning(string message)
	{
		if (debugMode)
		{
			Debug.LogWarning($"{DateTime.Now}: {message}");
		}
	}

	private void LogError(string message)
	{
		if (debugMode)
		{
			Debug.LogError($"{DateTime.Now}: {message}");
		}
	}

	private void SaveSettings()
	{
		try
		{
			InternetManagerSettings settings = new InternetManagerSettings
			{
				CheckInterval = checkInterval,
				RequestTimeout = requestTimeout,
				RetryCount = retryCount,
				CheckUrls = checkUrls
			};

			string json = JsonUtility.ToJson(settings);
			File.WriteAllText(GetSettingsFilePath(), json);
		}
		catch (Exception ex)
		{
			LogError($"Error saving settings: {ex.Message}");
		}
	}

	private void LoadSettings()
	{
		try
		{
			if (File.Exists(GetSettingsFilePath()))
			{
				string json = File.ReadAllText(GetSettingsFilePath());
				InternetManagerSettings settings = JsonUtility.FromJson<InternetManagerSettings>(json);

				checkInterval = settings.CheckInterval;
				requestTimeout = settings.RequestTimeout;
				retryCount = settings.RetryCount;
				checkUrls = settings.CheckUrls;
			}
			else
			{
				LogWarning("Settings file not found. Using default settings.");
			}
		}
		catch (Exception ex)
		{
			LogError($"Error loading settings: {ex.Message}");
		}
	}

	private string GetSettingsFilePath()
	{
		return Path.Combine(Application.persistentDataPath, "InternetManagerSettings.json");
	}
}

[Serializable]
public class InternetManagerSettings
{
	public float CheckInterval;
	public float RequestTimeout;
	public int RetryCount;
	public List<string> CheckUrls;
}

public class UnityWebRequestException : Exception
{
	public UnityWebRequestException(string message) : base(message) { }
}
