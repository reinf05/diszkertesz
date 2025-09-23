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
        private ImageSource image;

        private byte[]? imageBytes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotIdentified))]
        private bool isIdentified = false;

        public bool IsNotIdentified => !IsIdentified;

        [ObservableProperty]
        private IdentificationShow identificationShow = new IdentificationShow();

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
        private async Task NewImageAsync()
        {
            Image = null;
            imageBytes = null;
            IsLoaded = false;
            IsIdentified = false;
        }

        [RelayCommand]
        private async Task IdentifyAsync()
        {

            string organ = "auto";
            if (imageBytes != null)
            {
                string result = await plantService.Identify(imageBytes, organ);
                IdentificationResult data = JsonSerializer.Deserialize<IdentificationResult>(result);

                
                IdentificationShow.Percent = data.results[0].score * 100;
                IdentificationShow.Scientific = data.results[0].species.scientificNameWithoutAuthor;
                IdentificationShow.CommonNames = data.results[0].species.commonNames;

                IsIdentified = true;
            }
            return;
        }

    }
}
