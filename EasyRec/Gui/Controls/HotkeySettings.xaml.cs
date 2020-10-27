using EasyRec.Hotkeys;
using System;
using System.Collections.Generic;
using System.Text;
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
	/// Interaction logic for Hotkeys.xaml
	/// </summary>
	public partial class HotkeySettings : UserControl
	{
		public Startup Startup { get; set; }
		public HotkeySettings()
		{
			InitializeComponent();

			saveBuffer.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.SaveBuffer);
			startBuffer.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.StartBuffer);
			stopBuffer.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.StopBuffer);
			toggleBuffer.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.ToggleBuffer);

			startRecording.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.StartRecording);
			stopRecording.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.StopRecording);
			toggleRecording.Config = HotkeyHandler.GetHotkey(Configuration.HotkeyType.ToggleRecording);

		}

		private void HotkeyButton_HotkeyChanged(object sender, EventArgs e) => Startup.HotkeyHandler.ChangeHotkey((sender as HotkeyButton).Config);

		private void HotkeyButton_HotkeyRemoved(object sender, EventArgs e) => Startup.HotkeyHandler.RemoveHotkey((sender as HotkeyButton).HotkeyType);
	}
}
