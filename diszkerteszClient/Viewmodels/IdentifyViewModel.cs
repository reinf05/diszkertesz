using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    public partial class IdentifyViewModel : BaseViewModel
    {
        private PlantService plantService;

        [ObservableProperty]
        private ImageSource? image;

        private byte[]? imageBytes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotIdentified))]
        private bool isIdentified = false;

        public bool IsNotIdentified => !IsIdentified;

        [ObservableProperty]
        private IdentificationShow? identificationShow;

        public IdentifyViewModel(PlantService plantService)
        {
            this.plantService = plantService;
            Title = "Azonosítás";
        }

        [RelayCommand]
        private async Task CaptureAsync(CameraView camera)
        {
            if (camera == null) return;

            var photo = await camera.CaptureImage(CancellationToken.None);
            
            if(photo != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
                Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                IsLoaded = true;
            }
        }

        [RelayCommand]
        private Task NewImage()
        {
            Image = null;
            imageBytes = null;
            IsLoaded = false;
            IsIdentified = false;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task IdentifyAsync()
        {

            string organ = "auto";
            if (imageBytes != null)
            {

                string result = await plantService.Identify(imageBytes, organ);

                if (result.StartsWith("Error"))
                {
                    string errorMessage = string.Empty;
                    if(result.Contains("NotFound"))
                    {
                        errorMessage = "Nem ismerhető fel a növény";
                    }
                    await Shell.Current.DisplayAlert("Hiba", "Azonosítási hiba: " + errorMessage, "OK");
                    await NewImage();
                    return;
                }
                try
                {
                    IdentificationResult data = JsonSerializer.Deserialize<IdentificationResult>(result);
                    IdentificationShow temp = new();
                    temp.Percent = data.results[0].score * 100;
                    temp.Scientific = data.results[0].species.scientificNameWithoutAuthor;
                    temp.CommonNames = data.results[0].species.commonNames;

                    IdentificationShow = temp;
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Hiba", "Azonosítási hiba: " + ex.Message, "OK");
                    return;
                }



                IsIdentified = true;
            }
            return;
        }

    }
}
