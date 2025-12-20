using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;

namespace Client.Tests
{
    public partial class AuthorizationTestPage : ComponentBase
    {
        [Inject] private HttpClient HttpClient { get; set; } = null!;
        [Inject] private IAuthService AuthService { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private IAlertService AlertService { get; set; } = null!;

        // Authentication state
        private bool _isCheckingAuth = false;
        private bool _isAuthenticated = false;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private int _userId = 0;

        // Test state
        private bool _isRunningAllTests = false;
        private List<TestResult> _testResults = new();

        // Custom endpoint test
        private string _customEndpoint = "api/test/admin";
        private string _customMethod = "GET";
        private bool _isTestingCustom = false;
        private TestResult? _customResult = null;

        // Policy-based endpoints (require specific permissions)
        private List<EndpointTest> _policyEndpoints = new()
        {
            new() { Name = "Admin Policy", Endpoint = "api/test/admin", Description = "Requires 'Admin' permission" },
            new() { Name = "Viewer Policy", Endpoint = "api/test/viewer", Description = "Requires 'Viewer' permission" },
            new() { Name = "ActiveUser Policy", Endpoint = "api/test/active-user", Description = "Requires 'ActiveUser' permission" },
        };

        // Permission-based endpoints (dynamic permission check)
        private List<EndpointTest> _permissionEndpoints = new()
        {
            new() { Name = "Admin Permission", Endpoint = "api/test/permission/Admin", Description = "Check if user has 'Admin' permission" },
            new() { Name = "Viewer Permission", Endpoint = "api/test/permission/Viewer", Description = "Check if user has 'Viewer' permission" },
            new() { Name = "ActiveUser Permission", Endpoint = "api/test/permission/ActiveUser", Description = "Check if user has 'ActiveUser' permission" },
        };

        protected override async Task OnInitializedAsync()
        {
            await CheckAuthenticationStatus();
        }

        private async Task CheckAuthenticationStatus()
        {
            _isCheckingAuth = true;
            StateHasChanged();

            try
            {
                var response = await AuthService.GetCurrentUserAsync();

                if (response?.Success == true && response.Data != null)
                {
                    _isAuthenticated = true;
                    _username = response.Data.Username ?? "Unknown";
                    _email = response.Data.Email ?? "Unknown";
                    _userId = response.Data.Id;
                    AlertService.ShowSuccess($"Authenticated as {_username}");
                }
                else
                {
                    _isAuthenticated = false;
                    _username = string.Empty;
                    _email = string.Empty;
                    _userId = 0;
                    AlertService.ShowWarning("Not authenticated - tests will likely return 401 Unauthorized");
                }
            }
            catch (Exception ex)
            {
                _isAuthenticated = false;
                AlertService.ShowError($"Error checking authentication: {ex.Message}");
            }
            finally
            {
                _isCheckingAuth = false;
                StateHasChanged();
            }
        }

        private async Task TestEndpoint(EndpointTest endpoint)
        {
            endpoint.IsTesting = true;
            endpoint.Result = null;
            endpoint.StatusCode = string.Empty;
            StateHasChanged();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Endpoint);
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await HttpClient.SendAsync(request);
                var statusCode = (int)response.StatusCode;
                var success = response.IsSuccessStatusCode;

                endpoint.Result = success;
                endpoint.StatusCode = $"{statusCode} {response.StatusCode}";

                // Add to results log
                var testResult = new TestResult
                {
                    Endpoint = endpoint.Endpoint,
                    Method = "GET",
                    StatusCode = $"{statusCode}",
                    Success = success,
                    Message = GetStatusMessage(response.StatusCode, endpoint.Name),
                    Timestamp = DateTime.Now
                };
                _testResults.Add(testResult);

                // Show alert based on result
                if (success)
                {
                    AlertService.ShowSuccess($"✓ {endpoint.Name}: Access granted ({statusCode})");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AlertService.ShowWarning($"✗ {endpoint.Name}: Unauthorized (401) - Not logged in or session expired");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    AlertService.ShowInfo($"✗ {endpoint.Name}: Forbidden (403) - Logged in but missing required permission");
                }
                else
                {
                    AlertService.ShowError($"✗ {endpoint.Name}: Error ({statusCode})");
                }
            }
            catch (Exception ex)
            {
                endpoint.Result = false;
                endpoint.StatusCode = "Error";
                
                _testResults.Add(new TestResult
                {
                    Endpoint = endpoint.Endpoint,
                    Method = "GET",
                    StatusCode = "Error",
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.Now
                });

                AlertService.ShowError($"Error testing {endpoint.Name}: {ex.Message}");
            }
            finally
            {
                endpoint.IsTesting = false;
                StateHasChanged();
            }
        }

        private async Task TestCustomEndpoint()
        {
            if (string.IsNullOrWhiteSpace(_customEndpoint))
                return;

            _isTestingCustom = true;
            _customResult = null;
            StateHasChanged();

            try
            {
                var method = _customMethod switch
                {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    _ => HttpMethod.Get
                };

                var request = new HttpRequestMessage(method, _customEndpoint);
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                
                var response = await HttpClient.SendAsync(request);
                var statusCode = (int)response.StatusCode;
                var success = response.IsSuccessStatusCode;

                _customResult = new TestResult
                {
                    Endpoint = _customEndpoint,
                    Method = _customMethod,
                    StatusCode = $"{statusCode}",
                    Success = success,
                    Message = GetStatusMessage(response.StatusCode, _customEndpoint),
                    Timestamp = DateTime.Now
                };
                _testResults.Add(_customResult);

                // Show alert
                if (success)
                {
                    AlertService.ShowSuccess($"✓ {_customMethod} {_customEndpoint}: Access granted ({statusCode})");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AlertService.ShowWarning($"✗ {_customMethod} {_customEndpoint}: Unauthorized (401)");
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    AlertService.ShowInfo($"✗ {_customMethod} {_customEndpoint}: Forbidden (403)");
                }
                else
                {
                    AlertService.ShowError($"✗ {_customMethod} {_customEndpoint}: Error ({statusCode})");
                }
            }
            catch (Exception ex)
            {
                _customResult = new TestResult
                {
                    Endpoint = _customEndpoint,
                    Method = _customMethod,
                    StatusCode = "Error",
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.Now
                };
                _testResults.Add(_customResult);

                AlertService.ShowError($"Error: {ex.Message}");
            }
            finally
            {
                _isTestingCustom = false;
                StateHasChanged();
            }
        }

        private async Task RunAllTests()
        {
            _isRunningAllTests = true;
            StateHasChanged();

            AlertService.ShowInfo("Running all authorization tests...");

            try
            {
                // Test policy endpoints
                foreach (var endpoint in _policyEndpoints)
                {
                    await TestEndpoint(endpoint);
                    await Task.Delay(100); // Small delay between tests
                }

                // Test permission endpoints
                foreach (var endpoint in _permissionEndpoints)
                {
                    await TestEndpoint(endpoint);
                    await Task.Delay(100);
                }

                // Summary
                var passed = _policyEndpoints.Count(e => e.Result == true) + _permissionEndpoints.Count(e => e.Result == true);
                var total = _policyEndpoints.Count + _permissionEndpoints.Count;

                if (passed == total)
                {
                    AlertService.ShowSuccess($"All tests passed! ({passed}/{total})");
                }
                else if (passed > 0)
                {
                    AlertService.ShowWarning($"Some tests passed: {passed}/{total}");
                }
                else
                {
                    if (_isAuthenticated)
                    {
                        AlertService.ShowError($"All tests failed ({passed}/{total}). Check if permissions are assigned to your roles.");
                    }
                    else
                    {
                        AlertService.ShowInfo($"All tests denied ({passed}/{total}). This is expected since you're not authenticated.");
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowError($"Error running tests: {ex.Message}");
            }
            finally
            {
                _isRunningAllTests = false;
                StateHasChanged();
            }
        }

        private void ClearResults()
        {
            _testResults.Clear();
            _customResult = null;

            foreach (var endpoint in _policyEndpoints)
            {
                endpoint.Result = null;
                endpoint.StatusCode = string.Empty;
            }

            foreach (var endpoint in _permissionEndpoints)
            {
                endpoint.Result = null;
                endpoint.StatusCode = string.Empty;
            }

            AlertService.ShowInfo("Results cleared");
            StateHasChanged();
        }

        private string GetStatusMessage(HttpStatusCode statusCode, string context)
        {
            return statusCode switch
            {
                HttpStatusCode.OK => $"Access granted to {context}",
                HttpStatusCode.Unauthorized => "Not authenticated - please log in",
                HttpStatusCode.Forbidden => "Authenticated but missing required permission",
                HttpStatusCode.NotFound => "Endpoint not found",
                HttpStatusCode.InternalServerError => "Server error",
                _ => $"Response: {statusCode}"
            };
        }

        private string GetStatusColorClass(string statusCode)
        {
            if (statusCode.StartsWith("2")) return "bg-success";
            if (statusCode == "401") return "bg-warning";
            if (statusCode == "403") return "bg-info";
            if (statusCode.StartsWith("4")) return "bg-danger";
            if (statusCode.StartsWith("5")) return "bg-danger";
            return "bg-secondary";
        }

        // Helper classes
        private class EndpointTest
        {
            public string Name { get; set; } = string.Empty;
            public string Endpoint { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsTesting { get; set; } = false;
            public bool? Result { get; set; } = null;
            public string StatusCode { get; set; } = string.Empty;
        }

        private class TestResult
        {
            public string Endpoint { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string StatusCode { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}
