using System;
using System.Windows.Input;
using Serilog;

namespace NoteTray.Commands;

public class PerformSearchCommand : ICommand
{
	private readonly Action _performSearchAction;

	public PerformSearchCommand(Action performSearchAction)
	{
		_performSearchAction = performSearchAction;
	}
	
	public bool CanExecute(object parameter)
	{
		return true;
	}

	public void Execute(object parameter)
	{
		Log.Debug("PerformSearch Command Executing");
		_performSearchAction.Invoke();
	}

	public event EventHandler CanExecuteChanged;
}