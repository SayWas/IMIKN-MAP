using IMIKN_MAP.ViewModels;
using IMIKN_MAP.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace IMIKN_MAP
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
