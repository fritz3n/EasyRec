using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EasyRec.Gui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Startup application;
		Timer labelTimer;
		public MainWindow()
		{
			InitializeComponent();
			Hide();
			application = new Startup(this);
			application.OnStateChanged += Application_OnStateChanged;

			settingsControl.Startup = application;
			hotkeySettingsControl.Startup = application;

			labelTimer = new Timer(100);
			labelTimer.Elapsed += LabelTimer_Elapsed;
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			labelTimer.Start();
		}

		private void LabelTimer_Elapsed(object sender, ElapsedEventArgs e) => Dispatcher.BeginInvoke(UpdateLabels);
		private void Application_OnStateChanged(object sender, EventArgs e) => Dispatcher.BeginInvoke(UpdateAll);

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			labelTimer.Stop();
			Hide();
		}

		private void UpdateAll()
		{
			bufferButton.Content = (application.Buffering ? "Stop" : "Start") + " Buffering";
			bufferSaveButton.IsEnabled = application.Buffering;
			recorderButton.Content = (application.Recording ? "Stop" : "Start") + " Recording";
			UpdateLabels();
		}

		private void UpdateLabels()
		{
			bufferLabel.Content = "Buffer time: " + application.BufferedTime.ToString("m\\:ss");
			recordedLabel.Content = "Recorded time: " + application.RecordedTime.ToString("m\\:ss");
		}

		private void bufferButton_Click(object sender, RoutedEventArgs e) => application.SetBuffering(!application.Buffering);

		private async void bufferSaveButton_Click(object sender, RoutedEventArgs e) => await application.SaveBuffer();


		private void recorderButton_Click(object sender, RoutedEventArgs e) => application.SetRecording(!application.Recording);
	}
}
