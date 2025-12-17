using Client.Dtos;
using Client.Interfaces.Authorisation;
using Client.Services.Authorisation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] private IAuthService AuthService { get; set; } = null!;
        [Inject] private ISecureStorageService SecureStorage { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private ISnackbar Snackbar { get; set; } = null!;
        [Inject] private TokenRefreshService TokenRefreshService { get; set; } = null!;

        private LoginRequest loginRequest = new();
        private bool isLoading = false;
        private string errorMessage = string.Empty;
        private MudForm? form;
        private bool isValid;
        private string[] errors = Array.Empty<string>();
        private bool showPassword = false;

        protected override async Task OnInitializedAsync()
        {
            // Check if user is already logged in by calling /api/auth/me
            // This verifies the cookie is still valid
            var currentUserResponse = await AuthService.GetCurrentUserAsync();
            
            if (currentUserResponse?.Success == true && currentUserResponse.Data != null)
            {
                // User is already authenticated, store user data and redirect
                await SecureStorage.SetAsync("currentUser", currentUserResponse.Data);
                Navigation.NavigateTo("/home");
            }
        }

        private async Task HandleLogin()
        {
            errorMessage = string.Empty;
            
            if (!isValid)
            {
                errorMessage = "Please fix validation errors";
                return;
            }

            isLoading = true;
            try
            {
                var response = await AuthService.LoginAsync(loginRequest);

                if (response?.Success == true && response.Data != null)
                {
                    var loginData = response.Data;
                    
                    // Store the login response data for UI purposes
                    // Authentication is handled automatically via HTTP-only cookie
                    await SecureStorage.SetAsync("currentUser", loginData);
                    
                    // Start token refresh monitoring
                    TokenRefreshService.StartMonitoring();
                    
                    Snackbar.Add("Login successful!", Severity.Success);
                    Navigation.NavigateTo("/home");
                }
                else
                {
                    errorMessage = response?.Message ?? "Login failed. Please check your credentials.";
                    Snackbar.Add(errorMessage, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"An error occurred: {ex.Message}";
                Snackbar.Add(errorMessage, Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        private void TogglePasswordVisibility()
        {
            showPassword = !showPassword;
        }
    }
}
