using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace SnakeRemake.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        private readonly IWindowManager _windowManager;
        public MainViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
        public Task Start()
        {
            return _windowManager.ShowDialogAsync(IoC.Get<GameViewModel>());
        }
        public Task StartRanking()
        {
            return _windowManager.ShowDialogAsync(IoC.Get<RankingViewModel>());
        }
    }

}
