using System;
using System.Windows.Input;

namespace CompendiumMapCreator
{
	public class DelegateCommand<T> : ICommand

	{
		private readonly Predicate<T> canExecute;
		private readonly Action<T> execute;

		public DelegateCommand(Action<T> execute) : this(execute, null)
		{
		}

		public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(T parameter) => this.canExecute?.Invoke(parameter) ?? true;

		public void Execute(T parameter) => this.execute?.Invoke(parameter);

		protected void OnCanExecuteChanged(EventArgs e) => this.CanExecuteChanged?.Invoke(this, e);

		bool ICommand.CanExecute(object parameter)
		{
			if (parameter is T t)
			{
				return this.canExecute?.Invoke(t) ?? true;
			}
			else
			{
				return false;
			}
		}

		void ICommand.Execute(object parameter)
		{
			if (parameter is T t)
			{
				this.execute?.Invoke(t);
			}
		}
	}
}