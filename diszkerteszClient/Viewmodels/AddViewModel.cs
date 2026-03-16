using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
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
        private readonly UserService _authenticationService;

        [ObservableProperty]
        private UserItem userItem;

        private readonly PlantService _plantService;

        public AddViewModel(UserService authenticationService, PlantService plantService)
        {
            _authenticationService = authenticationService;
            userItem = new UserItem();
            _plantService = plantService;
        }

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

                bool tipsResult = await GetPlantTipsFromImage();
                if (!tipsResult)
                {
                    await Shell.Current.DisplayAlert("Hiba", "Nem sikerült növényt azonosítani a képről. Kérem próbálja meg egy másik képpel vagy töltse ki a növény nevét manuálisan.", "OK");
                }
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

                    bool tipsResult = await GetPlantTipsFromImage();
                    if (!tipsResult)
                    {
                        await Shell.Current.DisplayAlert("Hiba", "Nem sikerült növényt azonosítani a képről. Kérem próbálja meg egy másik képpel vagy töltse ki a növény nevét manuálisan.", "OK");
                    }
                }
            }
        }

        private async Task<bool> GetPlantTipsFromImage()
        {
            //Future improvement
            string organ = "auto";
            var tip = await _plantService.GetTipsFromImage(imageBytes, organ);

            if (tip != null) 
            {
                UserItem.PlantTips = tip;
                if (!String.IsNullOrEmpty(tip.LatinName))
                {
                    userItem.LatinName = tip.LatinName;
                }
                if(tip.HungarianName != null && tip.HungarianName.Count > 0)
                {
                    userItem.HungarianName = tip.HungarianName[0];
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<string> UploadImageAsync()
        {
            try
            {
                var result = await _authenticationService.UploadImageAsync(imageBytes, $"{userItem.LatinName}-{Guid.NewGuid()}.jpeg");
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

        //ÁT KELL NÉZNI, NÉV AUTOMATIKUS KITÖLTÉSE, KÉP FELTÖLTÉSE, VAGY NÉV KITÖLTÉSE UTÁN FELTÖLTÉS
        [RelayCommand]
        async Task SubmitAsync()
        {
            if(string.IsNullOrWhiteSpace(UserItem.HungarianName))
            {
                await Shell.Current.DisplayAlert("Error", "Nevet kötelező megadni", "OK");
                return;
            }

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
