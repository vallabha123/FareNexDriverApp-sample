using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Models.CustomModels
{
    public partial class RouteModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string  _routeID;

        [ObservableProperty]
        private string _routeName;

        [ObservableProperty]
        private string _fareCategory;


    }
}
