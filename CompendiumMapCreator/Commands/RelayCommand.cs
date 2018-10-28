using System;
using System.Windows.Input;

namespace CompendiumMapCreator
{
	public class RelayCommand : ICommand
	{
		private readonly Predicate<object> canExecute;
		private readonly Action<object> execute;

		public RelayCommand(Action<object> execute) : this(execute, null)
		{
		}

		public RelayCommand(Action<object> execute, Predicate<object> canExecute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter) => this.canExecute?.Invoke(parameter) ?? true;

		public void Execute(object parameter) => this.execute?.Invoke(parameter);

		protected void OnCanExecuteChanged(EventArgs e) => this.CanExecuteChanged?.Invoke(this, e);
	}
}