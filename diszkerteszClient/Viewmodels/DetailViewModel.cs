using CommunityToolkit.Mvvm.ComponentModel;
using diszkerteszClient.Models;
using diszkerteszClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Viewmodels
{
    [QueryProperty("FullPlant", "fullPlant")]
    public partial class DetailViewModel : BaseViewModel
    {
        [ObservableProperty]
        private FullPlant fullPlant;
        public DetailViewModel()
        { 
        }

        partial void OnFullPlantChanged(FullPlant value)
        {
            Title = value?.Namel!;
        }
    }

}
