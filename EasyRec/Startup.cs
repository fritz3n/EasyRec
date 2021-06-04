using EasyRec.Audio;
using EasyRec.Configuration;
using EasyRec.Gui;
using EasyRec.Hotkeys;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyRec
{
	public class Startup : IDisposable
	{
		private static ILog log = LogManager.GetLogger(nameof(Startup));

		NotifyIcon notifyIcon = new NotifyIcon();
		private readonly MainWindow mainWindow;
		AudioHandler audioHandler = new AudioHandler();
		private ToolStripMenuItem bufferItem;
		private ToolStripMenuItem recordItem;
		public event EventHandler OnStateChanged;

		public float Volume => audioHandler.Volume;

		public bool VolumeActive { get => audioHandler.VolumeActive; set => audioHandler.VolumeActive = value; }
		public bool Buffering => audioHandler.Buffering;
		public bool Recording => audioHandler.Recording;
		public TimeSpan RecordedTime => audioHandler.RecordedTime;
		public TimeSpan BufferedTime => audioHandler.BufferedTime;


		public HotkeyHandler HotkeyHandler { get; }

		public Startup(MainWindow mainWindow)
		{
			var f = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);

			Directory.SetCurrentDirectory(f.Directory.FullName);

			log.Info("Start");

			this.mainWindow = mainWindow;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = "EasyRec.Resources.easyrec.ico";

			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			notifyIcon.Icon = new Icon(stream);
			notifyIcon.Visible = true;
			notifyIcon.MouseClick += NotifyIcon_MouseClick;

			notifyIcon.ContextMenuStrip = new ContextMenuStrip();

			var settingsItem = new ToolStripMenuItem("Settings");
			settingsItem.Click += SettingsItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(settingsItem);

			var restartItem = new ToolStripMenuItem("Restart Audio");
			restartItem.Click += RestartItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(restartItem);

			var saveBufferItem = new ToolStripMenuItem("Save Buffer");
			saveBufferItem.Click += SaveBufferItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(saveBufferItem);

			bufferItem = new ToolStripMenuItem("Starting...")
			{
				Enabled = false
			};
			bufferItem.Click += BufferItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(bufferItem);

			recordItem = new ToolStripMenuItem("Starting...")
			{
				Enabled = false
			};
			recordItem.Click += RecordItem_Click;
			;
			notifyIcon.ContextMenuStrip.Items.Add(recordItem);

			var exitItem = new ToolStripMenuItem("Exit");
			exitItem.Click += ExitItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(exitItem);

			Task.Run(() => StartAudio(true));

			HotkeyHandler = new HotkeyHandler(this);
			HotkeyHandler.Initialize();
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.IsTerminating)
				MessageBox.Show(e.ExceptionObject.ToString());
		}

		private async void RestartItem_Click(object sender, EventArgs e) => await RebuildAudio();
		private void RecordItem_Click(object sender, EventArgs e) => SetRecording(!Recording);
		private void BufferItem_Click(object sender, EventArgs e) => SetBuffering(!Buffering);

		private async void SaveBufferItem_Click(object sender, EventArgs e)
		{
			if (audioHandler.Buffering)
				await SaveBuffer();
		}

		private void ExitItem_Click(object sender, EventArgs e)
		{
			notifyIcon.Visible = false;
			notifyIcon.Dispose();
			audioHandler.Dispose();
			System.Windows.Application.Current.Shutdown();
		}
		private void SettingsItem_Click(object sender, EventArgs e)
		{
			mainWindow.Show();
			mainWindow.Activate();
		}
		private async void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;
			if (Buffering)
			{
				await SaveBuffer();
			}
			else
			{
				mainWindow.Show();
				mainWindow.Activate();
			}
		}

		public void SetBuffering(bool buffering, bool notify = false)
		{
			log.Info("Buffering set " + buffering);

			if (buffering)
				audioHandler.StartBuffer();
			else
				audioHandler.StopBuffer();
			StateChanged();
			if (notify)
				notifyIcon.ShowBalloonTip(1000, "Buffer " + (buffering ? "on" : "off"), "Buffer has been turned " + (buffering ? "on" : "off"), ToolTipIcon.Info);
		}

		public void SetRecording(bool recording, bool notify = false)
		{
			log.Info("Recording set " + recording);

			if (recording)
				audioHandler.StartRecording();
			else
				audioHandler.StopRecording();
			StateChanged();
			if (notify)
				notifyIcon.ShowBalloonTip(1000, "Recording " + (recording ? "started" : "stopped"), "Recording has been " + (recording ? "started" : "stopped"), ToolTipIcon.Info);
		}

		public async Task SaveBuffer()
		{
			log.Info("Buffering");

			notifyIcon.ShowBalloonTip(500, "Saving buffer..", "Buffer is being saved..", ToolTipIcon.Info);
			TimeSpan saved = await audioHandler.SaveBuffer();
			if (saved == TimeSpan.Zero)
				return;
			notifyIcon.ShowBalloonTip(2000, "Buffer saved", saved.ToString("m\\:ss") + " were saved to Disk", ToolTipIcon.Info);
		}

		private void StateChanged()
		{
			UpdateLabels();
			OnStateChanged?.Invoke(this, EventArgs.Empty);
		}
		public async Task RebuildAudio() => await Task.Run(() => StartAudio());

		public void UpdateLabels(bool disable = false)
		{
			if (notifyIcon.ContextMenuStrip.InvokeRequired)
			{
				notifyIcon.ContextMenuStrip.Invoke(new Action<bool>(UpdateLabels), new object[] { disable });
				return;
			}

			if (disable)
			{
				recordItem.Text = "Starting...";
				bufferItem.Text = "Starting...";

				recordItem.Enabled = false;
				bufferItem.Enabled = false;
			}
			else
			{
				recordItem.Text = (Recording ? "Stop" : "Start") + " Recording";
				bufferItem.Text = (Buffering ? "Stop" : "Start") + " Buffering";

				recordItem.Enabled = true;
				bufferItem.Enabled = true;
			}
		}

		private void StartAudio(bool first = false)
		{
			UpdateLabels(true);
			if (!first)
			{
				log.Info("First Audiostart");

				bool recording = audioHandler.Recording;
				bool buffering = audioHandler.Buffering;
				audioHandler.BuildPipeline();

				SetBuffering(buffering);
				SetRecording(recording);
			}
			else
			{
				log.Info("Subsequent Audiostart");

				if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "autostart")
					Thread.Sleep(30 * 1000); // Delay 30s to guarantee that everything will work

				audioHandler.BuildPipeline();

				if (ConfigHandler.Config.BufferOnStart)
					audioHandler.StartBuffer();
				if (ConfigHandler.Config.RecordOnStart)
					audioHandler.StartRecording();

			}
			StateChanged();
		}

		public void Dispose()
		{
			notifyIcon.Dispose();
			audioHandler.Dispose();
		}
	}
}
