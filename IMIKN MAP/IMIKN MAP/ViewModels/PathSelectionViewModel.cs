using IMIKN_MAP.Models;
using IMIKN_MAP.Views;
using IMIKN_MAP.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IMIKN_MAP.ViewModels
{
    class PathSelectionViewModel : BaseViewModel
    {
        public ObservableCollection<Dot> Dots { get; set; }
        public ObservableCollection<Dot> RawDots { get; set; }
        public Command<Dot> DotTapped { get; }
        public Command<SearchBar> TextChangedCommand { get; }
        public Command BuildPathCommand { get; }

        private string collectionViewText;
        private string startSearchBarText;
        private string destinationSearchBarText;

        private Graph Graph { get; set; }
        private Dot[] NavigationDots { get; set; }
        private bool IsStartDot { get; set; }
        public string CollectionViewText { get => collectionViewText; set => SetProperty(ref collectionViewText, value); }
        public string StartSearchBarText { get => startSearchBarText; set => SetProperty(ref startSearchBarText, value); }
        public string DestinationSearchBarText { get => destinationSearchBarText; set => SetProperty(ref destinationSearchBarText, value); }

        public PathSelectionViewModel()
        {
            CollectionViewText = "Выберите место назначения";
            NavigationDots = new Dot[2];
            NavigationDots[0] = new Dot("Мое местоположение", 0, 0, 0, new string[1] {""});
            RawDots = new ObservableCollection<Dot>();
            RawDots.Add(new Dot("qwrq", 1, 1, 1, new string[] {""}));
            RawDots.Add(new Dot("qqef", 1, 1, 1, new string[] { "" }));
            RawDots.Add(new Dot("qwegq", 1, 1, 1, new string[] { "" }));
            RawDots.Add(new Dot("aaaq", 1, 1, 1, new string[] { "" }));
            RawDots.Add(new Dot("wrhetjs", 1, 1, 1, new string[] { "" }));
            RawDots.Add(new Dot("qberw", 1, 1, 1, new string[] { "" }));
            RawDots.Add(new Dot("zvdsbg", 1, 1, 1, new string[] { "" }));
            Dots = new ObservableCollection<Dot>();
            TextChangedCommand = new Command<SearchBar>(TextChanged);
            DotTapped = new Command<Dot>(OnDotSelected);
            BuildPathCommand = new Command(BuildPath);
            //LoadDots();
        }

        private void LoadDots()
        {
            IsBusy = true;

            try
            {
                Dots.Clear();
                var rawdots = JArray.Parse((string)App.Current.Properties["RawDots"]);
                foreach (var dot in rawdots)
                    RawDots.Add(new Dot((string)dot["Id"], (double)dot["X"], (double)dot["Y"], (int)dot["Floor"], dot["LinkedId"].ToObject<string[]>(), (bool)dot["IsStairs"]));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        private void OnDotSelected(Dot dot)
        {
            if (dot == null)
                return;

            if (IsStartDot) { NavigationDots[0] = dot; StartSearchBarText = dot.Id; }
            else { NavigationDots[1] = dot; DestinationSearchBarText = dot.Id; }
            Dots.Clear();
            CollectionViewText = "";
        }

        private async void BuildPath()
        {
            if (NavigationDots[0] == null || NavigationDots[1] == null || NavigationDots[0] == NavigationDots[1])
            {
                if (Dots.Count != 0)
                    Dots.Clear();
                CollectionViewText = "Невозможно посторить маршрут";
                return;
            }
            //Dot[] path = Graph.GetPath(NavigationDots[0].Id, NavigationDots[1].Id);
            await Shell.Current.GoToAsync("..");
        }

        private void TextChanged(SearchBar searchBar)
        {
            IsStartDot = searchBar.Placeholder.Contains("местоположение") ? true : false;
            if (Dots.Count != 0) Dots.Clear();

            if (String.IsNullOrEmpty(searchBar.Text))
            {
                foreach (Dot dot in RawDots)
                    Dots.Add(dot);

                if (NavigationDots[0] != null && IsStartDot) NavigationDots[0] = null;
                if (NavigationDots[1] != null && !IsStartDot) NavigationDots[1] = null;
                if (IsStartDot) NavigationDots[0] = new Dot("Мое местоположение", 0, 0, 0, new string[1] { "" });
                return;
            }

            foreach (Dot dot in new ObservableCollection<Dot>(RawDots.Where(d => d.Id.Contains(searchBar.Text))))
                Dots.Add(dot);
            if (Dots.Count == 0) CollectionViewText = "Таких мест нет";

            if (NavigationDots[0] != null && NavigationDots[0].Id != searchBar.Text && IsStartDot) NavigationDots[0] = null;
            if (NavigationDots[1] != null && NavigationDots[1].Id != searchBar.Text && !IsStartDot) NavigationDots[1] = null;
        }
    }
}
