using System;
using System.Windows.Input;

namespace DQB2ProcessEditor
{
	internal class CommandAction : ICommand
	{
		private readonly Action<object?> mAction;
		public event EventHandler? CanExecuteChanged;
		public bool CanExecute(object? parameter) => true;
		public CommandAction(Action<object?> action) => mAction = action;
		public void Execute(object? parameter) => mAction(parameter);
	}
}
