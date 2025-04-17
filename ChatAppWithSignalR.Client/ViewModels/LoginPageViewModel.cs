using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ChatAppWithSignalR.Client.ViewModels
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method for property change notification
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly ServiceProvider _serviceProvider;

        public LoginPageViewModel(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Default values (for development/testing only)
            UserName = "wanda";
            Password = "Abc12345";
            IsProcessing = false;

            LoginCommand = new Command(async () => await ExecuteLoginCommand());
        }

        private async Task ExecuteLoginCommand()
        {
            if (IsProcessing ||
                string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            IsProcessing = true;
            try
            {
                await Login();
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task Login()
        {
            try
            {
                var request = new AuthenticateRequest
                {
                    LoginId = UserName,
                    Password = Password,
                };

                var response = await _serviceProvider.Authenticate(request);
                if (response?.StatusCode == 200)
                {
                    await Shell.Current.GoToAsync($"ListChatPage?userId={response.Id}");
                }
                else
                {
                    await AppShell.Current.DisplayAlert("ChatApp", response?.StatusMessage ?? "Unknown error", "OK");
                }
            }
            catch (Exception ex)
            {
                await AppShell.Current.DisplayAlert("ChatApp", ex.Message, "OK");
            }
        }

        // Backing fields
        private string userName;
        private string password;
        private bool isProcessing;

        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsProcessing
        {
            get => isProcessing;
            set
            {
                if (isProcessing != value)
                {
                    isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }
    }
}