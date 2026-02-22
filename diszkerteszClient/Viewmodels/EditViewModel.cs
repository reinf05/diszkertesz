using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    [QueryProperty("Item", "Item")]
    public partial class EditViewModel : BaseViewModel
    {
        //private readonly UserService _userService;
        //private UserItem userItem;

        //[ObservableProperty]
        //private UserItem item;

        //partial void OnItemChanged(UserItem value)
        //{
        //    userItem = value;
        //    ItemName = userItem.Name;
        //    ItemDescription = userItem.Description;

        //    if (!string.IsNullOrEmpty(userItem.Pictureurl))
        //    {
        //        Image = ImageSource.FromUri(new Uri(userItem.Pictureurl));
        //    }
        //}

        //public EditViewModel(UserService userService)
        //{
        //    _userService = userService;
        //}

        //[ObservableProperty]
        //private string itemName;
        //[ObservableProperty]
        //private string itemDescription;

        //[ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(IsNotImage))]
        //private bool isImage = false;

        //public bool IsNotImage => !IsImage;

        //[ObservableProperty]
        //[NotifyPropertyChangedFor(nameof(HasNotImage))]
        //private bool hasImage = false;
        //public bool HasNotImage => !HasImage;

        //[ObservableProperty]
        //private ImageSource? image;
        //private byte[]? imageBytes;

        //private bool imageChanged = false;
        //private string oldImageUrl = string.Empty;

        //[RelayCommand]
        //async Task ChangeImageStateAsync()
        //{
        //    IsImage = !IsImage;
        //}

        //[RelayCommand]
        //private async Task CaptureAsync(CameraView camera)
        //{
        //    if (camera == null) return;

        //    var photo = await camera.CaptureImage(CancellationToken.None);

        //    if (photo != null)
        //    {
        //        MemoryStream memoryStream = new MemoryStream();
        //        await photo.CopyToAsync(memoryStream);
        //        Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        //        imageBytes = memoryStream.ToArray();
        //        HasImage = true;
        //        imageChanged = true;
        //    }
        //}

        //[RelayCommand]
        //private Task NewImage()
        //{
        //    Image = null;
        //    HasImage = false;
        //    imageBytes = null;
        //    imageChanged = true;
        //    return Task.CompletedTask;
        //}

        //[RelayCommand]
        //async Task PickImageAsync()
        //{
        //    var result = await FilePicker.Default.PickAsync(new PickOptions
        //    {
        //        FileTypes = FilePickerFileType.Images,
        //        PickerTitle = "Válassz egy képet"
        //    });
        //    if (result != null)
        //    {
        //        using (var stream = await result.OpenReadAsync())
        //        {
        //            MemoryStream memoryStream = new MemoryStream();
        //            await stream.CopyToAsync(memoryStream);
        //            Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        //            imageBytes = memoryStream.ToArray();
        //            HasImage = true;
        //            imageChanged = true;
        //        }
        //    }
        //}

        //private async Task<string> UploadImageAsync()
        //{
        //    try
        //    {
        //        var result = await _userService.UploadImageAsync(imageBytes, $"{userItem.LatinName}-{Guid.NewGuid()}.jpeg");
        //        if (string.IsNullOrEmpty(result))
        //        {
        //            await Shell.Current.DisplayAlert("Error", "Hiba történt a kép feltöltése során", "OK");
        //            return string.Empty;
        //        }
        //        else
        //        {
        //            return result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Shell.Current.DisplayAlert("Error", $"Hiba történt a kép feltöltése során: {ex.Message}", "OK");
        //        return string.Empty;
        //    }
        //}

        //private async Task<bool> DeleteImageAsync(string imageURL)
        //{
        //    try
        //    {
        //        await _userService.DeleteImage(imageURL);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await Shell.Current.DisplayAlert("Error", $"Hiba történt a kép törlése során: {ex.Message}", "OK");
        //        return false;
        //    }

        //}

        //[RelayCommand]
        //async Task<bool> DeleteAsync()
        //{
        //    bool answer = await Shell.Current.DisplayAlert("Figyelem", "Biztosan törölni szeretnéd a tételt?", "Igen", "Nem");
        //    if (!answer) return false;

        //    IsBusy = true;
        //    try
        //    {
        //        bool itemDelete = await _userService.DeleteItemAsync(userItem.Id);
        //        if (itemDelete)
        //        {
        //            //SQL row is deleted
        //            if (!string.IsNullOrEmpty(userItem.Pictureurl))
        //            {
        //                await DeleteImageAsync(userItem.Pictureurl);
        //            }
        //            await Shell.Current.DisplayAlert("Siker", "A tétel sikeresen törölve lett.", "OK");
        //            await Shell.Current.GoToAsync("..");
        //            IsBusy = false;
        //            return true;
        //        }
        //        else
        //        {
        //            await Shell.Current.DisplayAlert("Error", "Hiba történt a tétel törlése során.", "OK");
        //            IsBusy = false;
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Shell.Current.DisplayAlert("Error", $"Hiba történt a tétel törlése során: {ex.Message}", "OK");
        //        IsBusy = false;
        //        return false;
        //    }
        //}

        //[RelayCommand]
        //async Task<bool> EditAsync()
        //{
        //    IsBusy = true;
        //    if (string.IsNullOrWhiteSpace(ItemName))
        //    {
        //        await Shell.Current.DisplayAlert("Error", "Nevet kötelező megadni", "OK");
        //        return false;
        //    }

        //    if (imageChanged)
        //    {
        //        if(!string.IsNullOrEmpty(userItem.Pictureurl))
        //        {
        //            oldImageUrl = userItem.Pictureurl;
        //        }
        //        if (HasImage)
        //        {
        //            //upload new image
        //            string imageURL = await UploadImageAsync();
        //            if (string.IsNullOrEmpty(imageURL))
        //            {
        //                IsBusy = false;
        //                return false;
        //            }
        //            userItem.Pictureurl = imageURL;
        //        }
        //        else
        //        {
        //            //no image
        //            userItem.Pictureurl = null;
        //        }
        //    }

        //    userItem.Name = ItemName;
        //    userItem.Description = ItemDescription;
        //    try
        //    {
        //        bool result = await _userService.UpdateItemAsync(userItem);
        //        if (result)
        //        {
        //            await Shell.Current.DisplayAlert("Siker", "A tétel sikeresen frissítve lett.", "OK");
        //            await Shell.Current.GoToAsync("..");
        //            //delete old image if exists
        //            if(!string.IsNullOrEmpty(oldImageUrl))
        //            {
        //                await DeleteImageAsync(oldImageUrl);

        //            }
        //            IsBusy = false;
        //            return true;
        //        }
        //        else
        //        {
        //            await Shell.Current.DisplayAlert("Error", "Hiba történt a tétel frissítése során.", "OK");
        //            IsBusy = false;
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Shell.Current.DisplayAlert("Error", $"Hiba történt a tétel frissítése során: {ex.Message}", "OK");
        //        IsBusy = false;
        //        return false;
        //    }
        //}
    }
}
