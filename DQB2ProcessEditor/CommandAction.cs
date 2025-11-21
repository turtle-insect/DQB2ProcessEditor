using System;
using System.Windows.Input;

namespace DQB2ProcessEditor
{
	internal class CommandAction : ICommand
	{
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

		private readonly Action<object?> mAction;
		public bool CanExecute(object? parameter) => true;
		public CommandAction(Action<object?> action) => mAction = action;
		public void Execute(object? parameter) => mAction(parameter);
	}
}
