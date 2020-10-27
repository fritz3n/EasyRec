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
	/// <summary>
	/// Interaction logic for PathChooser.xaml
	/// </summary>
	public partial class PathChooser : System.Windows.Controls.UserControl
	{
		public event TextChangedEventHandler TextChanged;
		public string Text { get => Path.Text; set => Path.Text = value; }

		public PathChooser()
		{
			InitializeComponent();
		}



		private void Path_TextChanged(object sender, TextChangedEventArgs e) => TextChanged?.Invoke(this, e);

		private void OpenFileButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Audio Files (*.mp3;*.wav;*.aac)|*.mp3;*.wav;*.aac|All files (*.*)|*.*"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				Path.Text = saveFileDialog.FileName;
			}
		}
	}
}
