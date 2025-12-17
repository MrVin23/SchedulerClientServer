using Microsoft.AspNetCore.Components;

namespace Client.Pages
{
    public partial class NotAuthorised : ComponentBase
    {
        [Inject] private NavigationManager Navigation { get; set; } = null!;

        private void NavigateToLogin()
        {
            Navigation.NavigateTo("/login");
        }
    }
}

