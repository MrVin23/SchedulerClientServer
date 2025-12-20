using Client.Dtos;
using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private IAuthService AuthService { get; set; } = null!;
        [Inject] private ISecureStorageService SecureStorage { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private IAlertService AlertService { get; set; } = null!;

        protected LoginResponse? currentUser;
        private bool isLoading = true;
        private bool isVerifying = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserData();
        }

        private async Task LoadUserData()
        {
            try
            {
                // First, check if we have stored user data (indicates successful login)
                currentUser = await SecureStorage.GetAsync<LoginResponse>("currentUser");
                
                if (currentUser != null)
                {
                    // We have stored data from successful login, show spinner and verify cookie
                    isLoading = false;
                    isVerifying = true;
                    StateHasChanged();
                    
                    // Verify cookie is available with retry mechanism
                    await VerifyCookieWithRetry();
                }
                else
                {
                    // No stored data, try to verify with server (might be page refresh)
                    await VerifyAuthenticationDirectly();
                }
            }
            catch (Exception ex)
            {
                // If we have stored data, use it even if verification fails
                if (currentUser == null)
                {
                    AlertService.ShowError($"Error loading user data: {ex.Message}");
                    Navigation.NavigateTo("/login");
                }
            }
            finally
            {
                isLoading = false;
                isVerifying = false;
            }
        }

        private async Task VerifyCookieWithRetry()
        {
            const int maxRetries = 2;
            const int retryDelayMs = 200;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var userResponse = await AuthService.GetCurrentUserAsync();
                    
                    if (userResponse?.Success == true && userResponse.Data != null)
                    {
                        // Cookie is valid, update with fresh data from server
                        currentUser = userResponse.Data;
                        await SecureStorage.SetAsync("currentUser", currentUser);
                        isVerifying = false;
                        StateHasChanged();
                        return; // Success, exit retry loop
                    }
                }
                catch
                {
                    // Ignore exceptions during retry, continue to next attempt
                }
                
                // If not the last attempt, wait before retrying
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(retryDelayMs);
                }
            }
            
            // All retries exhausted - cookie might not be ready yet
            // But we have stored data from successful login, so continue with it
            isVerifying = false;
            StateHasChanged();
        }

        private async Task VerifyAuthenticationDirectly()
        {
            try
            {
                var userResponse = await AuthService.GetCurrentUserAsync();
                
                if (userResponse?.Success == true && userResponse.Data != null)
                {
                    // User is authenticated, store and use the data
                    currentUser = userResponse.Data;
                    await SecureStorage.SetAsync("currentUser", currentUser);
                }
                else
                {
                    // Not authenticated, redirect to login
                    Navigation.NavigateTo("/login");
                }
            }
            catch
            {
                // Error verifying, redirect to login
                Navigation.NavigateTo("/login");
            }
        }

        protected async Task HandleLogout()
        {
            try
            {
                // Call server logout endpoint to clear the cookie
                await AuthService.LogoutAsync();
                
                // Remove stored user data
                await SecureStorage.RemoveAsync("currentUser");
                
                AlertService.ShowSuccess("Logged out successfully");
                Navigation.NavigateTo("/login");
            }
            catch (Exception ex)
            {
                // Even if server logout fails, clear local data
                await SecureStorage.RemoveAsync("currentUser");
                AlertService.ShowError($"Error during logout: {ex.Message}");
                Navigation.NavigateTo("/login");
            }
        }
    }
}
