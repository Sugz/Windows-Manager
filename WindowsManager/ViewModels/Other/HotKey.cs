using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Helpers;
using System;
using System.Windows.Input;

namespace WindowsManager.ViewModels
{
    public class HotKey : ViewModelBase, ICommand, IEquatable<HotKey>
    {

        private bool _IsEnable = true;
        private string _Name;
        private string _Description;
        private ModifierKeys _Modifiers;
        private Key _Key;
        private readonly WeakAction _Execute;
        private readonly WeakFunc<bool> _CanExecute;



        public bool IsEnable
        {
            get => _IsEnable;
            set => Set(ref _IsEnable, value);
        }

        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }

        public string Description
        {
            get => _Description;
            set => Set(ref _Description, value);
        }

        public ModifierKeys Modifiers
        {
            get => _Modifiers;
            set => Set(ref _Modifiers, value);
        }

        public Key Key
        {
            get => _Key;
            set => Set(ref _Key, value);
        }





        private EventHandler _requerySuggestedLocal;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_CanExecute != null)
                {
                    // add event handler to local handler backing field in a thread safe manner
                    EventHandler handler2;
                    EventHandler canExecuteChanged = _requerySuggestedLocal;

                    do
                    {
                        handler2 = canExecuteChanged;
                        EventHandler handler3 = (EventHandler)Delegate.Combine(handler2, value);
                        canExecuteChanged = System.Threading.Interlocked.CompareExchange<EventHandler>(
                            ref _requerySuggestedLocal,
                            handler3,
                            handler2);
                    }
                    while (canExecuteChanged != handler2);

                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_CanExecute != null)
                {
                    // removes an event handler from local backing field in a thread safe manner
                    EventHandler handler2;
                    EventHandler canExecuteChanged = this._requerySuggestedLocal;

                    do
                    {
                        handler2 = canExecuteChanged;
                        EventHandler handler3 = (EventHandler)Delegate.Remove(handler2, value);
                        canExecuteChanged = System.Threading.Interlocked.CompareExchange<EventHandler>(
                            ref this._requerySuggestedLocal,
                            handler3,
                            handler2);
                    }
                    while (canExecuteChanged != handler2);

                    CommandManager.RequerySuggested -= value;
                }
            }
        }





        /// <summary>
        /// Initializes a new instance of the RelayCommand class that 
        /// can always execute.
        /// </summary>
        /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="keepTargetAlive">If true, the target of the Action will
        /// be kept as a hard reference, which might cause a memory leak. You should only set this
        /// parameter to true if the action is causing a closure. See
        /// http://galasoft.ch/s/mvvmweakaction. </param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public HotKey(string name, Action execute, Key key, ModifierKeys modifiers = ModifierKeys.None, string description = null, bool keepTargetAlive = false)
            : this(name, execute, null, key, modifiers, description, keepTargetAlive)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="canExecute">The execution status logic.  IMPORTANT: If the func causes a closure,
        /// you must set keepTargetAlive to true to avoid side effects. </param>
        /// <param name="keepTargetAlive">If true, the target of the Action will
        /// be kept as a hard reference, which might cause a memory leak. You should only set this
        /// parameter to true if the action is causing a closures. See
        /// http://galasoft.ch/s/mvvmweakaction. </param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public HotKey(string name, Action execute, Func<bool> canExecute, Key key, ModifierKeys modifiers = ModifierKeys.None, string description = null, bool keepTargetAlive = false)
        {
            _Name = name ?? throw new ArgumentNullException(nameof(name));
            _Description = description;

            if (key == Key.None)
                throw new ArgumentNullException(nameof(key));
            _Key = key;
            _Modifiers = modifiers;


            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _Execute = new WeakAction(execute, keepTargetAlive);

            if (canExecute != null)
                _CanExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
        }



        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }


        public bool CanExecute()
        {
            return CanExecute(null);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || (_CanExecute.IsStatic || _CanExecute.IsAlive) && _CanExecute.Execute();
        }


        public virtual void Execute()
        {
            Execute(null);
        }


        /// <summary>
        /// Defines the method to be called when the command is invoked. 
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        public virtual void Execute(object parameter)
        {
            if (CanExecute(parameter) && _Execute != null && (_Execute.IsStatic || _Execute.IsAlive))
            {
                _Execute.Execute();
            }
        }



        public override bool Equals(object obj)
        {
            if (obj is HotKey hotKey)
                return Equals(hotKey);
            else
                return false;
        }

        public bool Equals(HotKey other)
        {
            return (Key == other.Key && Modifiers == other.Modifiers);
        }

        public override int GetHashCode()
        {
            return (int)Modifiers + 10 * (int)Key;
        }
    }
}