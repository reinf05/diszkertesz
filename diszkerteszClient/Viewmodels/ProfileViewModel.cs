using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private UserService userService;

        [ObservableProperty]
        private ObservableCollection<UserItem>? userItems;

        public ProfileViewModel(UserService authenticationService)
        {
            this.userService = authenticationService;
        }

        [RelayCommand]
        async Task SignInAsync()
        {
            var authResult = await userService.SignInAsync();
            if (authResult is not null)
            {
                await Shell.Current.DisplayAlert("Success", $"Welcome {authResult.Account.Username}!", "OK");
                IsLoaded = true;
                await LoadUserList();
            }
        }

        [RelayCommand]
        async Task SignUpAsync()
        {
            var signUpResult = await userService.SignUpAsync();
            if (signUpResult is not null)
            {
                await Shell.Current.DisplayAlert("Success", $"SignUp {signUpResult.Account.Username}!", "OK");
                IsLoaded = true;
            }
        }

        [RelayCommand]
        async Task SignOutAsync()
        {
            await userService.SignOutAsync();
            UserItems = null;
            IsLoaded = false;
        }

        [RelayCommand]
        async Task GoToAddPageAsync()
        {
            await Shell.Current.GoToAsync(nameof(View.AddPage));
        }

        [RelayCommand]
        async Task<bool> LoadUserList()
        {
            if (IsNotLoaded)
            {
                return false;
            }
            try
            {
                var fetchedList = await userService.GetCurrentUserList();

                if (fetchedList is null) { return false; }

                if (UserItems == null)
                {
                    UserItems = new ObservableCollection<UserItem>(fetchedList);
                    return true;
                }

                foreach (var item in UserItems.ToList())
                {
                    if (!fetchedList.Any(x => x.Id == item.Id))
                    {
                        UserItems.Remove(item);
                    }
                }

                foreach (var item in fetchedList)
                {
                    if (!UserItems.Any(x => x.Id == item.Id))
                    {
                        UserItems.Add(item);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUserList Failed: {ex.Message}");
                return false;
            }
        }

        [RelayCommand]
        async Task GoToEditPageAsync(UserItem item)
        {
            var navigationParams = new Dictionary<string, object>
            {
                { "Item", item}
            };
            await Shell.Current.GoToAsync(nameof(View.EditPage), true, navigationParams);
        }
    }
}
