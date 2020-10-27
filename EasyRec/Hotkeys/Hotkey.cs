using EasyRec.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EasyRec.Hotkeys
{
	public class Hotkey : IDisposable
	{
		static KeyboardHook hook = new KeyboardHook();

		public static bool Enabled { get; set; } = true;

		public ModifierKeys Modifier { get; private set; }
		public Keys Key { get; private set; }
		public HotkeyType HotkeyType { get; }
		public bool Registered { get; private set; } = false;
		int Id { get; set; } = -1;

		public event EventHandler<EventArgs> Pressed;

		public Hotkey(ModifierKeys modifier, Keys key, HotkeyType hotkeyType)
		{
			Modifier = modifier;
			Key = key;
			HotkeyType = hotkeyType;
			hook.KeyPressed += Hook_KeyPressed;
		}

		private void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
		{
			if (Enabled && e.Modifier == Modifier && e.Key == Key)
				Pressed?.Invoke(this, EventArgs.Empty);
		}

		public bool Register()
		{
			if (Registered)
				Unregister();

			try
			{
				Id = hook.RegisterHotKey(Modifier, Key);
				Registered = true;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Unregister()
		{
			if (!Registered)
				return;

			hook.UnregisterHotkey(Id);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) => Unregister();

		~Hotkey()
		{
			Dispose(false);
		}
	}
}
