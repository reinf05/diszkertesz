using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private AuthenticationService authenticationService;

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
                // Handle post sign-in actions (e.g., update UI)
            }
        }

        [RelayCommand]
        public async Task SignOutAsync()
        {
            await authenticationService.SignOutAsync();
            // Handle post sign-out actions (e.g., update UI)
        }

    }
}
