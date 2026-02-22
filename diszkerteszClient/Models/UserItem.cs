using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public partial class UserItem : ObservableObject
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        
        [ObservableProperty]
        private string hungarianName;
        [ObservableProperty]
        private string latinName;

        [ObservableProperty]
        private string? description;

        [ObservableProperty]
        private string? pictureurl;

        [ObservableProperty]
        private PlantTips plantTips;

    }
}
