using CommunityToolkit.Mvvm.Input;
using Díszkertész_kliens.Models;

namespace Díszkertész_kliens.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}