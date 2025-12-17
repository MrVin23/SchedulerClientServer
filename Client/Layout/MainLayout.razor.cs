using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Client.Services.Authorisation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Client.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject] private IUserInfoService UserInfoService { get; set; } = null!;
        [Inject] private IAuthService AuthService { get; set; } = null!;
        [Inject] private ISecureStorageService SecureStorage { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private IAlertService AlertService { get; set; } = null!;
        [Inject] private TokenRefreshService TokenRefreshService { get; set; } = null!;

        private bool _drawerOpen = true;
        private string _welcomeMessage = "Welcome User";
        protected string _username = string.Empty;

        // Alert state
        private bool _alertVisible = false;
        private string _alertMessage = string.Empty;
        private Severity _alertSeverity = Severity.Info;
        private Variant _alertVariant = Variant.Filled;

        protected override async Task OnInitializedAsync()
        {
            await LoadWelcomeMessage();
            Navigation.LocationChanged += OnLocationChanged;
            
            // Subscribe to alert service events
            AlertService.OnAlert += HandleAlertTriggered;
            AlertService.OnClear += HandleAlertCleared;
        }

        protected override async Task OnParametersSetAsync()
        {
            // Refresh welcome message when parameters change (e.g., after navigation)
            await LoadWelcomeMessage();
        }

        private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            // Refresh welcome message when navigation occurs (e.g., after login)
            await LoadWelcomeMessage();
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadWelcomeMessage()
        {
            var firstName = await UserInfoService.GetFirstNameAsync();
            var lastName = await UserInfoService.GetLastNameAsync();
            _username = await UserInfoService.GetUsernameAsync() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                _welcomeMessage = $"Welcome {firstName} {lastName}";
            }
            else if (!string.IsNullOrWhiteSpace(firstName))
            {
                _welcomeMessage = $"Welcome {firstName}";
            }
            else
            {
                _welcomeMessage = "Welcome User";
            }

            StateHasChanged();
        }

        private void HandleAccount()
        {
            // Placeholder for account navigation - no action for now
        }

        private void HandleLogin()
        {
            Navigation.NavigateTo("/login");
        }

        private async Task HandleLogout()
        {
            try
            {
                // Stop token refresh monitoring
                TokenRefreshService.StopMonitoring();
                
                // Call server logout endpoint to clear the cookie
                await AuthService.LogoutAsync();
                
                // Remove stored user data
                await SecureStorage.RemoveAsync("currentUser");
                
                // Clear username
                _username = string.Empty;
                _welcomeMessage = "Welcome User";
                
                Snackbar.Add("Logged out successfully", Severity.Success);
                Navigation.NavigateTo("/login");
            }
            catch (Exception ex)
            {
                // Even if server logout fails, clear local data and stop monitoring
                TokenRefreshService.StopMonitoring();
                await SecureStorage.RemoveAsync("currentUser");
                _username = string.Empty;
                _welcomeMessage = "Welcome User";
                
                Snackbar.Add($"Error during logout: {ex.Message}", Severity.Error);
                Navigation.NavigateTo("/login");
            }
        }

        private void ToggleDrawer()
        {
            _drawerOpen = !_drawerOpen;
        }

        private string WelcomeMessage => _welcomeMessage;

        private void HandleAlertTriggered(AlertMessage alert)
        {
            _alertMessage = alert.Message;
            _alertSeverity = alert.Severity;
            _alertVariant = alert.Variant;
            _alertVisible = true;
            InvokeAsync(StateHasChanged);
        }

        private void HandleAlertCleared()
        {
            _alertVisible = false;
            InvokeAsync(StateHasChanged);
        }

        private void HandleAlertClosed()
        {
            _alertVisible = false;
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
            AlertService.OnAlert -= HandleAlertTriggered;
            AlertService.OnClear -= HandleAlertCleared;
        }
    }
}