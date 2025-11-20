using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System.Text.Json;
using System.Diagnostics;

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
                Microsoft.Maui.Graphics.IImage image; 

                using (Stream stream = new MemoryStream(imageBytes))
                {

                    image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);

                }


                if (image != null)
                {
                    Microsoft.Maui.Graphics.IImage smallImage = image.Resize(900, 900);
                    var smallImageStream = new MemoryStream();
                    smallImage.Save(smallImageStream, ImageFormat.Jpeg, 0.7f);
                    var smallImageBytes = smallImageStream.ToArray();


                    string result = await plantService.Identify(smallImageBytes, organ);


                    if (result.StartsWith("Error"))
                    {
                        string errorMessage = string.Empty;
                        if (result.Contains("NotFound"))
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
                else
                {
                    await Shell.Current.DisplayAlert("Hiba", "Átméretezési hiba", "OK");
                }
            }
            return;
        }

    }
}
