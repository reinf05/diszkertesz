using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using IntelliJ.Lang.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    public partial class AddViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authenticationService;
        private UserItem userItem;

        public AddViewModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            userItem = new UserItem();
        }

        [ObservableProperty]
        private string itemName;
        [ObservableProperty]
        private string itemDescription;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotImage))]
        private bool isImage = false;

        public bool IsNotImage => !IsImage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNotImage))]
        private bool hasImage = false;
        public bool HasNotImage => !HasImage;

        [ObservableProperty]
        private ImageSource? image;
        private byte[]? imageBytes;

        [RelayCommand]
        async Task ChangeImageStateAsync()
        {
            IsImage = !IsImage;
        }

        [RelayCommand]
        private async Task CaptureAsync(CameraView camera)
        {
            if (camera == null) return;

            var photo = await camera.CaptureImage(CancellationToken.None);

            if (photo != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                imageBytes = memoryStream.ToArray();
                HasImage = true;
            }
        }

        [RelayCommand]
        private Task NewImage()
        {
            Image = null;
            HasImage = false;
            imageBytes = null;
            return Task.CompletedTask;
        }

        [RelayCommand]
        async Task PickImageAsync()
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Válassz egy képet"
            });
            if (result != null)
            {
                using (var stream = await result.OpenReadAsync())
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                    imageBytes = memoryStream.ToArray();
                    HasImage = true;
                }
            }
        }

        private async Task<string> UploadImageAsync()
        {
            try
            {
                var result = await _authenticationService.UploadImageAsync(imageBytes, $"{ItemName}.jpeg");
                if (string.IsNullOrEmpty(result))
                {
                    await Shell.Current.DisplayAlert("Error", "Hiba történt a kép feltöltése során", "OK");
                    return string.Empty;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Hiba történt a kép feltöltése során: {ex.Message}", "OK");
                return string.Empty;
            }
        }

        [RelayCommand]
        async Task SubmitAsync()
        {
            if(string.IsNullOrWhiteSpace(ItemName))
            {
                await Shell.Current.DisplayAlert("Error", "Nevet kötelező megadni", "OK");
                return;
            }

            userItem.Name = ItemName;
            userItem.Description = ItemDescription;

            if(Image != null)
            {
                string pictureURL = await UploadImageAsync();
                userItem.Pictureurl = pictureURL;
            }

            var result = await _authenticationService.UploadItemAsync(userItem);
            if(result)
            {
                await Shell.Current.DisplayAlert("Siker", "Tétel sikeresen feltöltve", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Hiba történt a feltöltés során", "OK");
            }

        }


    }
}
