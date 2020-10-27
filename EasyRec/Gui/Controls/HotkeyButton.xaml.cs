using EasyRec.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyRec.Gui.Controls
{
	public partial class HotkeyButton : System.Windows.Controls.UserControl
	{
		bool recording = false;
		private HotkeyConfig config;
		public bool Enabled { get; private set; }


		public HotkeyType HotkeyType
		{
			get => (HotkeyType)GetValue(HotkeyTypeProperty);
			set => SetValue(HotkeyTypeProperty, value);
		}

		// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HotkeyTypeProperty =
			DependencyProperty.Register("HotkeyType", typeof(HotkeyType), typeof(HotkeyButton), new PropertyMetadata(HotkeyType.SaveBuffer));


		//HotkeyType HotkeyType { get; set; }

		public HotkeyConfig Config { get => Enabled ? config : null; set { config = value ?? null; Enabled = !(value is null); UpdateText(); } }

		public event EventHandler HotkeyChanged;
		public event EventHandler HotkeyRemoved;

		public HotkeyButton()
		{
			InitializeComponent();
			UpdateText();
		}

		private void UpdateText()
		{
			string text;
			if (Enabled)
			{
				text = config.ModifierKeys.ToString().Replace(",", " +");
				text += " + " + config.Key.ToString();
			}
			else
			{
				text = "No Hotkey set.";
			}
			KeyButton.Content = text;
		}

		private void HotkeyButton_Click(object sender, RoutedEventArgs e)
		{
			recording = !recording;

			if (recording)
			{
				KeyButton.Content = "Press keys to assign Hotkey";
			}
			else
			{
				UpdateText();
			}
		}

		private void CallHotkeyChanged() => HotkeyChanged?.Invoke(this, EventArgs.Empty);
		private void CallHotkeyRemoved() => HotkeyRemoved?.Invoke(this, EventArgs.Empty);

		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			if (!recording)
				return;

			// Filter out Modifier-Keys
			if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.System ||
				e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
				e.Key == Key.LeftShift || e.Key == Key.RightShift ||
				e.Key == Key.LWin || e.Key == Key.RWin)
			{
				return;
			}

			if (e.Key == Key.Escape)
			{
				Enabled = false;
				e.Handled = true;
				CallHotkeyRemoved();
				UpdateText();
				recording = false;
				return;
			}

			Enabled = true;

			e.Handled = true;
			Config = new HotkeyConfig((Hotkeys.ModifierKeys)Keyboard.Modifiers, (Keys)KeyInterop.VirtualKeyFromKey(e.Key), HotkeyType);

			CallHotkeyChanged();
			UpdateText();
			recording = false;
		}
	}
}
