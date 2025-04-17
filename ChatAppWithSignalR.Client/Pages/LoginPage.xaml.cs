using Microsoft.Maui.Controls;
using ChatAppWithSignalR.Client.ViewModels;

namespace ChatAppWithSignalR.Client.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}