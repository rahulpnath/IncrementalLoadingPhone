using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using IncrementalLoadingPhone.Resources;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Collections;

namespace IncrementalLoadingPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Register for your api key here 
        // http://500px.com/settings/applications?from=developers
        private static string consumerKey = "YOUR_CONSUMER_KEY";
        private static int requestPerPage = 20;
        private int currentPage = 1;
        private bool isCurrentlyLoading = false;

        private ObservableCollection<Photo> Photos = new ObservableCollection<Photo>();

        private string datasourceUrl = "https://api.500px.com/v1/photos?feature=popular&consumer_key=" + consumerKey + "&rpp=" + requestPerPage.ToString() + "&page={0}";

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            photosList.ItemsSource = Photos;
            scrollStateListBox.ItemsSource = Photos;
            LoadDataFromSource();

        }

        private void LoadDataFromSource()
        {
            progressBar.IsVisible = true;
            isCurrentlyLoading = true;
            var query = string.Format(datasourceUrl, currentPage);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri(query));

        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            using (var reader = new MemoryStream(Encoding.Unicode.GetBytes(e.Result)))
            {
                var ser = new DataContractJsonSerializer(typeof(RootObject));
                RootObject obj = (RootObject)ser.ReadObject(reader);
                currentPage = obj.current_page + 1;
                if (obj != null)
                {
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            foreach (var photo in obj.photos)
                            {
                                Photos.Add(photo);
                            }
                            isCurrentlyLoading = false;
                            progressBar.IsVisible = false;
                        });
                }
            }
        }

        private void photosList_ItemRealized_1(object sender, ItemRealizationEventArgs e)
        {
            Photo photo = e.Container.Content as Photo;
            if (photo != null)
            {
                int offset = 2;
                // Only if there is no data that is currently getting loaded would be initiate the loading again
                if (!isCurrentlyLoading && Photos.Count - Photos.IndexOf(photo) <= offset)
                {
                    LoadDataFromSource();
                }
            }
        }

        private void myScrollViewer_Loaded_1(object sender, RoutedEventArgs e)
        {
            SetScrollViewer();
        }

        private void SetScrollViewer()
        {
            // Visual States are always on the first child of the control template 
            FrameworkElement element = VisualTreeHelper.GetChild(myScrollViewer, 0) as FrameworkElement;
            if (element != null)
            {
                VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");

                if (vgroup != null)
                {
                    vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                }
            }
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {

            }

            if (e.NewState.Name == "CompressionBottom")
            {
                if (!isCurrentlyLoading )
                {
                    LoadDataFromSource();
                }
            }
            if (e.NewState.Name == "NoVerticalCompression")
            {

            }
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}