using System.Windows;

namespace NoteTray;

public partial class PreferencesWindow : Window
{
	public delegate PreferencesWindow Factory(Window parent);
	
	public PreferencesWindow(Window parent, PreferencesViewModel viewModel)
	{
		Owner = parent;
		DataContext = viewModel;
		InitializeComponent();

		if (Owner != null)
		{
			Top = Owner.Top;
			Left = Owner.Left + Owner.Width;
		}
	}
}