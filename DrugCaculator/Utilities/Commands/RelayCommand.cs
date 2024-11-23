using System;
using System.Windows.Input;

namespace DrugCalculator.Utilities.Commands
{
    /// <summary>
    /// 一个通用的命令实现，支持传递执行逻辑和可执行逻辑。
    /// </summary>
    /// <remarks>
    /// 构造函数，初始化命令逻辑。
    /// </remarks>
    /// <param name="execute">命令的执行逻辑，不能为空。</param>
    /// <param name="canExecute">命令的可执行逻辑，可以为空，默认为总是可执行。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="execute"/> 为 null 时抛出。</exception>
    public class RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) : ICommand
    {
        // 保存命令的执行逻辑
        private readonly Action<object> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        /// <summary>
        /// 当命令的可执行性发生变化时触发。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 确定命令是否可以执行。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        /// <returns>如果可以执行则返回 true，否则返回 false。</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        /// 执行命令的逻辑。
        /// </summary>
        /// <param name="parameter">命令参数。</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// 手动触发 <see cref="CanExecuteChanged"/> 事件，通知绑定系统重新评估命令的可执行性。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}