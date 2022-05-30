using IMIKN_MAP.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace IMIKN_MAP.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}