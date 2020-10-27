using EasyRec.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyRec.Gui.Controls
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : System.Windows.Controls.UserControl
	{
		private bool criticalChange = false;
		private bool recordChange = false;
		public Startup Startup { get; set; }

		public Settings()
		{
			InitializeComponent();
			Load();
		}

		private void Load()
		{
			Config conf = ConfigHandler.Config;
			bufferPath.Text = conf.BufferPath;
			bufferPattern.Text = conf.BufferPattern;
			bufferWriter.SelectedIndex = (int)conf.BufferWriter;
			bufferLength.Text = conf.BufferLength.ToString();

			recordPath.Text = conf.RecordPath;
			recordPattern.Text = conf.RecordPattern;
			recordWriter.SelectedIndex = (int)conf.RecordWriter;

			mixdownType.SelectedIndex = (int)conf.MixdownType;
			bufferOnStart.IsChecked = conf.BufferOnStart;
			recordOnStart.IsChecked = conf.RecordOnStart;

			inputListBox.Items.Clear();
			foreach (KeyValuePair<string, string> pair in WasapiGui.GetInputs())
			{
				CheckBox checkbox = new CheckBox()
				{
					Content = pair.Value,
					Tag = pair.Key,
					IsChecked = conf.Inputs.Contains(pair.Key)
				};
				checkbox.Checked += CriticalChecked;
				checkbox.Unchecked += CriticalChecked;
				inputListBox.Items.Add(checkbox);
			}

			recordChange = false;
			criticalChange = false;
		}


		private void NumberPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex(@"[^0-9]+(?:\.[^0-9]+)");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void RecordTextChanged(object sender, TextChangedEventArgs e) => recordChange = true;
		private void RecordSelectionChanged(object sender, SelectionChangedEventArgs e) => recordChange = true;
		private void CriticalSelectionChanged(object sender, SelectionChangedEventArgs e) => criticalChange = true;
		private void CriticalChecked(object sender, RoutedEventArgs e) => criticalChange = true;
		private void CriticalTextChanged(object sender, TextChangedEventArgs e) => criticalChange = true;


		private async void saveButton_Click(object sender, RoutedEventArgs e)
		{
			string[] inputs = inputListBox.Items.Cast<CheckBox>().Where(i => i.IsChecked ?? false).Select(i => (string)i.Tag).ToArray();

			if (inputs.Length == 0)
			{
				MessageBox.Show("Please select at least one input.", "Too few inputs", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (criticalChange && (Startup.Buffering || Startup.Recording))
			{
				if (MessageBoxResult.Cancel ==
					MessageBox.Show("A critical setting was changed. If you continue recording and the buffer will be stopped.",
									"Critical change",
									MessageBoxButton.OKCancel,
									MessageBoxImage.Warning))
				{
					return;
				}
			}
			else if (recordChange && Startup.Recording)
			{
				if (MessageBoxResult.Cancel ==
				MessageBox.Show("You changed settings concerning recording. If you continue recording will bes stopped.",
								"Recording settings changed",
								MessageBoxButton.OKCancel,
								MessageBoxImage.Warning))
				{
					return;
				}
			}


			Config conf = new Config()
			{
				BufferPath = bufferPath.Text,
				BufferPattern = bufferPattern.Text,
				BufferLength = float.Parse(bufferLength.Text),
				BufferWriter = (FileWriterType)bufferWriter.SelectedIndex,

				RecordPath = recordPath.Text,
				RecordPattern = recordPattern.Text,
				RecordWriter = (FileWriterType)recordWriter.SelectedIndex,

				MixdownType = (MixdownType)mixdownType.SelectedIndex,
				BufferOnStart = bufferOnStart.IsChecked ?? false,
				RecordOnStart = recordOnStart.IsChecked ?? false,

				Inputs = inputs,
				Bits = ConfigHandler.Config.Bits,
				SampleRate = ConfigHandler.Config.SampleRate
			};

			ConfigHandler.Config = conf;
			ConfigHandler.Save();

			if (recordChange)
				Startup.SetRecording(false);
			if (criticalChange)
				await Startup.RebuildAudio();

			recordChange = false;
			criticalChange = false;
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e) => Load();

	}
}
