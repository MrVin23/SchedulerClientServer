using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MudBlazor;
using System.Net.Http.Json;

namespace Client.Tests
{
    public partial class TestRoleAccess : ComponentBase
    {
        [Inject] private IAuthService AuthService { get; set; } = null!;
        [Inject] private IUserInfoService UserInfoService { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private IAlertService AlertService { get; set; } = null!;
        [Inject] private HttpClient HttpClient { get; set; } = null!;

        // Authentication state
        private bool _isLoading = true;
        private bool _isAuthenticated = false;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private int _userId = 0;

        // Test states
        private bool _testingAdmin = false;
        private bool _testingViewer = false;
        private bool _testingActiveUser = false;

        // Test results
        private bool? _adminResult = null;
        private bool? _viewerResult = null;
        private bool? _activeUserResult = null;

        // Result message
        private string? _lastTestMessage = null;
        private Severity _lastTestSeverity = Severity.Info;

        protected override async Task OnInitializedAsync()
        {
            await CheckAuthenticationState();
        }

        private async Task CheckAuthenticationState()
        {
            _isLoading = true;

            try
            {
                var response = await AuthService.GetCurrentUserAsync();

                if (response?.Success == true && response.Data != null)
                {
                    _isAuthenticated = true;
                    _username = response.Data.Username ?? "Unknown";
                    _email = response.Data.Email ?? "Unknown";
                    _userId = response.Data.Id;
                }
                else
                {
                    _isAuthenticated = false;
                }
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                AlertService.ShowError($"Error checking authentication: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void NavigateToLogin()
        {
            Navigation.NavigateTo("/login");
        }

        private async Task TestRoleEndpoint(string endpoint)
        {
            // Set loading state
            switch (endpoint)
            {
                case "admin":
                    _testingAdmin = true;
                    break;
                case "viewer":
                    _testingViewer = true;
                    break;
                case "active-user":
                    _testingActiveUser = true;
                    break;
            }

            try
            {
                // IMPORTANT: Must include credentials to send the authentication cookie!
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/test/{endpoint}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var response = await HttpClient.SendAsync(request);
                var success = response.IsSuccessStatusCode;

                // Update result
                switch (endpoint)
                {
                    case "admin":
                        _adminResult = success;
                        break;
                    case "viewer":
                        _viewerResult = success;
                        break;
                    case "active-user":
                        _activeUserResult = success;
                        break;
                }

                // Update message
                var roleName = endpoint switch
                {
                    "admin" => "Administrator",
                    "viewer" => "User (Viewer)",
                    "active-user" => "VerifiedUser (ActiveUser)",
                    _ => endpoint
                };

                if (success)
                {
                    _lastTestMessage = $"✓ Access granted to {roleName} endpoint!";
                    _lastTestSeverity = Severity.Success;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _lastTestMessage = $"✗ Access denied to {roleName} endpoint. You don't have the required permission.";
                    _lastTestSeverity = Severity.Warning;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _lastTestMessage = $"✗ Unauthorized. Please log in again.";
                    _lastTestSeverity = Severity.Error;
                    _isAuthenticated = false;
                }
                else
                {
                    _lastTestMessage = $"✗ Error testing {roleName}: {response.StatusCode}";
                    _lastTestSeverity = Severity.Error;
                }
            }
            catch (Exception ex)
            {
                _lastTestMessage = $"Error: {ex.Message}";
                _lastTestSeverity = Severity.Error;
                AlertService.ShowError($"Error testing endpoint: {ex.Message}");
            }
            finally
            {
                // Clear loading state
                switch (endpoint)
                {
                    case "admin":
                        _testingAdmin = false;
                        break;
                    case "viewer":
                        _testingViewer = false;
                        break;
                    case "active-user":
                        _testingActiveUser = false;
                        break;
                }
            }
        }

        private async Task TestAllRoles()
        {
            // Reset results
            _adminResult = null;
            _viewerResult = null;
            _activeUserResult = null;

            await TestRoleEndpoint("admin");
            await TestRoleEndpoint("viewer");
            await TestRoleEndpoint("active-user");

            // Summary message
            var passed = (_adminResult == true ? 1 : 0) + (_viewerResult == true ? 1 : 0) + (_activeUserResult == true ? 1 : 0);
            _lastTestMessage = $"Test complete: {passed}/3 role tests passed";
            _lastTestSeverity = passed == 3 ? Severity.Success : (passed > 0 ? Severity.Warning : Severity.Error);
        }
    }
}

