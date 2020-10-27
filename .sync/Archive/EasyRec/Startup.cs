using EasyRec.Audio;
using EasyRec.Configuration;
using EasyRec.Gui;
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
	class Startup
	{
		NotifyIcon notifyIcon = new NotifyIcon();
		private readonly MainWindow mainWindow;
		AudioHandler audioHandler = new AudioHandler();
		private ToolStripMenuItem bufferItem;
		private ToolStripMenuItem recordItem;

		public Startup(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;


			Assembly assembly = Assembly.GetExecutingAssembly();
			string resourceName = "EasyRec.Resources.easyrec.ico";

			using Stream stream = assembly.GetManifestResourceStream(resourceName);
			notifyIcon.Icon = new Icon(stream);
			notifyIcon.Visible = true;
			notifyIcon.MouseClick += NotifyIcon_MouseClick;

			notifyIcon.ContextMenuStrip = new ContextMenuStrip();

			ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings");
			settingsItem.Click += SettingsItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(settingsItem);

			ToolStripMenuItem saveBufferItem = new ToolStripMenuItem("Save Buffer");
			saveBufferItem.Click += SaveBufferItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(saveBufferItem);

			bufferItem = new ToolStripMenuItem("Starting...")
			{
				Enabled = false
			};
			bufferItem.Click += BufferItem_Click;
			;
			notifyIcon.ContextMenuStrip.Items.Add(bufferItem);

			recordItem = new ToolStripMenuItem("Starting...")
			{
				Enabled = false
			};
			recordItem.Click += RecordItem_Click;
			;
			notifyIcon.ContextMenuStrip.Items.Add(recordItem);

			ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
			exitItem.Click += ExitItem_Click;
			notifyIcon.ContextMenuStrip.Items.Add(exitItem);

			Task.Run(() => StartAudio(true));
		}

		private void RecordItem_Click(object sender, EventArgs e)
		{
			if (audioHandler.Recording)
				audioHandler.StopRecording();
			else
				audioHandler.StartRecording();
			UpdateLabels();
		}
		private void BufferItem_Click(object sender, EventArgs e)
		{
			if (audioHandler.Buffering)
				audioHandler.StopBuffer();
			else
				audioHandler.StartBuffer();
			UpdateLabels();
		}

		private void SaveBufferItem_Click(object sender, EventArgs e)
		{
			if (audioHandler.Buffering)
				audioHandler.SaveBuffer();
		}

		private void ExitItem_Click(object sender, EventArgs e)
		{
			audioHandler.Dispose();
			Application.Exit();
		}
		private void SettingsItem_Click(object sender, EventArgs e)
		{
			mainWindow.Show();
			mainWindow.Activate();
		}
		private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;
			if (audioHandler.Buffering)
			{
				audioHandler.SaveBuffer();
				SystemSounds.Exclamation.Play();
			}
			else
			{
				mainWindow.Show();
				mainWindow.Activate();
			}
		}

		public async Task RebuildAudio() => await Task.Run(() => StartAudio());

		public void UpdateLabels()
		{
			recordItem.Text = (audioHandler.Recording ? "Stop" : "Start") + " Recording";
			bufferItem.Text = (audioHandler.Buffering ? "Stop" : "Start") + " Buffering";

			recordItem.Enabled = true;
			bufferItem.Enabled = true;
		}

		private void StartAudio(bool first = false)
		{
			recordItem.Enabled = false;
			bufferItem.Enabled = false;

			audioHandler.BuildPipeline();
			if (first)
			{
				if (ConfigHandler.Config.BufferOnStart)
					audioHandler.StartBuffer();
				if (ConfigHandler.Config.RecordOnStart)
					audioHandler.StartRecording();
			}
			UpdateLabels();
		}
	}
}
