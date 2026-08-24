using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.SessionFiles
{
	public sealed class SessionFilesJanitor : ISessionFilesJanitor
	{
		private readonly IFolderPathService folderPathService;
		private readonly IDisposable optionsChangeDisposable;
		private int sessionStateTimeoutInMinutes;

		private readonly IDictionary<string, DateTimeOffset> sessionsLastAccessed;
		private readonly Timer timer;
		private readonly object lockKey = new object();
		private bool disposedValue;

		public SessionFilesJanitor(IFolderPathService folderPathService, IOptionsMonitor<Config> configOptionsMonitor)
		{
			this.sessionsLastAccessed = new Dictionary<string, DateTimeOffset>();

			this.folderPathService = folderPathService;
			this.sessionStateTimeoutInMinutes = configOptionsMonitor.CurrentValue.SessionStateTimeout;
			this.optionsChangeDisposable = configOptionsMonitor.OnChange(c =>
			{
				if (this.sessionStateTimeoutInMinutes != c.SessionStateTimeout)
				{
					this.sessionStateTimeoutInMinutes = c.SessionStateTimeout;
					this.timer.Interval = GetTimerInterval(c.SessionStateTimeout);
				}
			});

			this.timer = new Timer(GetTimerInterval(this.sessionStateTimeoutInMinutes));
			this.timer.Elapsed += (sender, eventargs) => RunCleanup();
			this.timer.Start();
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void RefreshTimeout(string sessionId)
		{
			lock (this.lockKey)
			{
				if (this.sessionsLastAccessed.ContainsKey(sessionId))
				{
					this.sessionsLastAccessed[sessionId] = DateTimeOffset.UtcNow;
				}
				else
				{
					this.sessionsLastAccessed.Add(sessionId, DateTimeOffset.UtcNow);
				}
			}
		}

		public void ClearSession(string sessionId)
		{
			lock (this.lockKey)
			{
				this.sessionsLastAccessed.Remove(sessionId);
			}

			RunCleanup();
		}

		private void RunCleanup()
		{
			var activeSessionIds = new HashSet<string>();

			lock (this.lockKey)
			{
				var sessionLastAccessedStructs = this.sessionsLastAccessed.Select(e => (sessionId: e.Key, lastAccessed: e.Value)).ToArray();
				foreach ((string sessionId, var lastAccessed) in sessionLastAccessedStructs)
				{
					var sessionTimeoutTime = lastAccessed + TimeSpan.FromMinutes(this.sessionStateTimeoutInMinutes);
					if (sessionTimeoutTime < DateTimeOffset.UtcNow)
					{
						this.sessionsLastAccessed.Remove(sessionId);
					}
					else
					{
						activeSessionIds.Add(sessionId);
					}
				}
			}

			if (!Directory.Exists(this.folderPathService.SessionsFolderPath))
				return;

			string[] sessionFolderPaths = Directory.GetDirectories(this.folderPathService.SessionsFolderPath);
			foreach (string sessionFolderPath in sessionFolderPaths)
			{
				string sessionId = new DirectoryInfo(sessionFolderPath).Name;
				if (!activeSessionIds.Contains(sessionId))
				{
					try
					{
						Directory.Delete(sessionFolderPath, true);
					}
					catch { }
				}
			}
		}

		private double GetTimerInterval(int sessionStateTimeoutInMinutes)
		{
			double interval = TimeSpan.FromMinutes(sessionStateTimeoutInMinutes).TotalMilliseconds / 2;
			return interval > int.MaxValue ? int.MaxValue : interval;
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					this.optionsChangeDisposable.Dispose();
					this.timer.Dispose();
				}

				this.disposedValue = true;
			}
		}
	}
}
