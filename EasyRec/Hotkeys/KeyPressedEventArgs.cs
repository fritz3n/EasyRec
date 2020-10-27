using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EasyRec.Hotkeys
{
	public class KeyPressedEventArgs : EventArgs
	{
		internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
		{
			Modifier = modifier;
			Key = key;
		}

		public ModifierKeys Modifier { get; }

		public Keys Key { get; }
	}
}
