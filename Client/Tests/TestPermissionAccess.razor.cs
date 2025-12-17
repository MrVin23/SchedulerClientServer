using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Tests
{
    public partial class TestPermissionAccess : ComponentBase
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

        // Custom permission test
        private string _customPermission = string.Empty;
        private string _testedPermission = string.Empty;
        private bool _testingCustom = false;
        private bool? _customPermissionResult = null;

        // Predefined permissions
        private List<PermissionTest> _predefinedPermissions = new()
        {
            new() { Name = "Admin", Description = "Administrator level access", Color = Color.Error },
            new() { Name = "Viewer", Description = "View-only access", Color = Color.Info },
            new() { Name = "ActiveUser", Description = "Active user features", Color = Color.Success },
            new() { Name = "CanEditPosts", Description = "Permission to edit posts", Color = Color.Warning },
            new() { Name = "CanDeleteUsers", Description = "Permission to delete users", Color = Color.Error },
            new() { Name = "CanAccessAdminPanel", Description = "Access to admin panel", Color = Color.Secondary }
        };

        // Result message
        private string? _lastTestMessage = null;
        private Severity _lastTestSeverity = Severity.Info;
        private List<PermissionDetail>? _permissionDetails = null;

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

        private async Task TestCustomPermission()
        {
            if (string.IsNullOrWhiteSpace(_customPermission))
                return;

            _testingCustom = true;
            _testedPermission = _customPermission;
            _customPermissionResult = null;

            try
            {
                // IMPORTANT: Must include credentials to send the authentication cookie!
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/test/permission/{Uri.EscapeDataString(_customPermission)}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var response = await HttpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionApiResponse>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    _customPermissionResult = result?.Data?.HasAccess ?? false;
                    _lastTestMessage = result?.Data?.Message ?? "Permission test completed";
                    _lastTestSeverity = _customPermissionResult == true ? Severity.Success : Severity.Warning;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _customPermissionResult = false;
                    _lastTestMessage = "Unauthorized. Please log in again.";
                    _lastTestSeverity = Severity.Error;
                    _isAuthenticated = false;
                }
                else
                {
                    _customPermissionResult = false;
                    _lastTestMessage = $"Error: {response.StatusCode}";
                    _lastTestSeverity = Severity.Error;
                }
            }
            catch (Exception ex)
            {
                _customPermissionResult = false;
                _lastTestMessage = $"Error: {ex.Message}";
                _lastTestSeverity = Severity.Error;
                AlertService.ShowError($"Error testing permission: {ex.Message}");
            }
            finally
            {
                _testingCustom = false;
            }
        }

        private async Task TestPredefinedPermission(string permissionName)
        {
            var permission = _predefinedPermissions.FirstOrDefault(p => p.Name == permissionName);
            if (permission == null) return;

            permission.IsTesting = true;
            permission.Result = null;
            StateHasChanged();

            try
            {
                // IMPORTANT: Must include credentials to send the authentication cookie!
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/test/permission/{Uri.EscapeDataString(permissionName)}");
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                var response = await HttpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PermissionApiResponse>(content, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    permission.Result = result?.Data?.HasAccess ?? false;
                    _lastTestMessage = result?.Data?.Message ?? "Permission test completed";
                    _lastTestSeverity = permission.Result == true ? Severity.Success : Severity.Warning;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    permission.Result = false;
                    _lastTestMessage = "Unauthorized. Please log in again.";
                    _lastTestSeverity = Severity.Error;
                    _isAuthenticated = false;
                }
                else
                {
                    permission.Result = false;
                    _lastTestMessage = $"Error testing {permissionName}: {response.StatusCode}";
                    _lastTestSeverity = Severity.Error;
                }
            }
            catch (Exception ex)
            {
                permission.Result = false;
                AlertService.ShowError($"Error testing permission: {ex.Message}");
            }
            finally
            {
                permission.IsTesting = false;
                StateHasChanged();
            }
        }

        private async Task TestAllPermissions()
        {
            // Reset all results
            foreach (var permission in _predefinedPermissions)
            {
                permission.Result = null;
            }
            _permissionDetails = new List<PermissionDetail>();

            foreach (var permission in _predefinedPermissions)
            {
                await TestPredefinedPermission(permission.Name);
                
                _permissionDetails.Add(new PermissionDetail
                {
                    Permission = permission.Name,
                    HasAccess = permission.Result ?? false,
                    Message = permission.Result == true 
                        ? $"You have the {permission.Name} permission" 
                        : $"You do NOT have the {permission.Name} permission"
                });
            }

            // Summary message
            var passed = _predefinedPermissions.Count(p => p.Result == true);
            var total = _predefinedPermissions.Count;
            _lastTestMessage = $"Permission test complete: {passed}/{total} permissions granted";
            _lastTestSeverity = passed == total ? Severity.Success : (passed > 0 ? Severity.Warning : Severity.Info);
        }

        // Helper classes
        private class PermissionTest
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Color Color { get; set; } = Color.Default;
            public bool IsTesting { get; set; } = false;
            public bool? Result { get; set; } = null;
        }

        private class PermissionDetail
        {
            public string Permission { get; set; } = string.Empty;
            public bool HasAccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        // API Response classes
        private class PermissionApiResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public PermissionTestData? Data { get; set; }
        }

        private class PermissionTestData
        {
            public bool HasAccess { get; set; }
            public string? PermissionName { get; set; }
            public string? Message { get; set; }
            public int UserId { get; set; }
            public string? Username { get; set; }
        }
    }
}

