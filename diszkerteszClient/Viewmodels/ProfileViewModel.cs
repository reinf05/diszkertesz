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
        private AuthenticationService authenticationService;

        [ObservableProperty]
        private List<UserItem> userItems;

        public ProfileViewModel(AuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        [RelayCommand]
        public async Task SignInAsync()
        {
            var authResult = await authenticationService.SignInAsync();
            if(authResult is not null)
            {
                await Shell.Current.DisplayAlert("Success", $"Welcome {authResult.Account.Username}!", "OK");
                IsLoaded = true;
            }
        }

        [RelayCommand]
        public async Task SignUpAsync()
        {
            var signUpResult = await authenticationService.SignUpAsync();
            if (signUpResult is not null)
            {
                await Shell.Current.DisplayAlert("Success", $"SignUp {signUpResult.Account.Username}!", "OK");
                IsLoaded = true;
            }
        }

        [RelayCommand]
        public async Task SignOutAsync()
        {
            await authenticationService.SignOutAsync();
            UserItems = null; 
            IsLoaded = false;
        }

        [RelayCommand]
        public async Task GoToAddPageAsync()
        {
            await Shell.Current.GoToAsync(nameof(View.AddPage));
        }

        [RelayCommand]
        public async Task<bool> LoadUserList()
        {
            if (IsNotLoaded)
            {
                return false;
            }
            try
            {
                var userList = await authenticationService.GetCurrentUserList();
                if (userList is not null)
                {
                    UserItems = userList;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadUserList Failed: {ex.Message}");
                return false;
            }
            return false;
        }
    }
}
