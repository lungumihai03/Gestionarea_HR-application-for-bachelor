using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;
using Gestionarea_HR;

namespace Gestionarea_HR
{
    public sealed partial class AdminPanelPage : Page
    {
        private string userRights;

        public AdminPanelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string userRights)
            {
                this.userRights = userRights;
                AdjustMenuForUser();
            }
        }

        private void AdjustMenuForUser()
        {
            if (userRights == "Admin")
            {
            }
            else if (userRights == "Accountant-Salary")
            {
                HideMenuItem("CreateUserPage");
                HideMenuItem("MyHRPage");
            }
            else if (userRights == "Recruiter")
            {
                HideMenuItem("CreateUserPage");
                HideMenuItem("SalaryPage");
            }
            else if (userRights == "Director")
            {
                HideMenuItem("CreateUserPage");
            }
            else
            {
                Application.Current.Exit();
            }
        }

        private void HideMenuItem(string tag)
        {
            var item = NavigationMenu.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => i.Tag.ToString() == tag);
            if (item != null) item.Visibility = Visibility.Collapsed;
        }
     

        private void NavigationMenu_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "CreateUserPage":
                       ContentFrame.Navigate(typeof(UserManagementPage));
                        break;
                    case "Logout":
                        Frame.Navigate(typeof(LoginPage));
                        break;
                    case "Exit":
                        Application.Current.Exit();
                        break;
                    case "HirePage":
                        ContentFrame.Navigate(typeof(EmployeePage));
                        break;
                    case "ViewEmployeePage":
                        ContentFrame.Navigate(typeof(ViewEmployeesPage));
                        break;
                    case "LeavePage":
                        ContentFrame.Navigate(typeof(Concedii));
                        break;
                    case "FisaAngajat":
                        ContentFrame.Navigate(typeof(FisaAngajat));
                        break;
                    case "Settings":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "CalculSalarial": 
                        ContentFrame.Navigate(typeof(Salariu));
                        break;
                    case "Fluturasi":
                        ContentFrame.Navigate(typeof(ButterflyPage));
                        break;
                    case "RaportSalarial":
                        ContentFrame.Navigate(typeof(SalReportsPage));
                        break;
                }
            }
        }
    }
}
