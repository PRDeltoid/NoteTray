using System;
using System.Windows.Input;
using Serilog;

namespace NoteTray.Commands;

public class ClearSearchCommand : ICommand
{
	private readonly Action _clearSearchAction;

	public ClearSearchCommand(Action clearSearchAction)
	{
		_clearSearchAction = clearSearchAction;
	}
	
	public bool CanExecute(object parameter)
	{
		return true;
	}

	public void Execute(object parameter)
	{
		Log.Debug("ClearSearch Command Executing");
		_clearSearchAction.Invoke();
	}

	public event EventHandler CanExecuteChanged;
}