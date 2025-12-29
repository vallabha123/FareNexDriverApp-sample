using System.Threading.Tasks;

namespace FareNexDriverApp.Core.Interfaces.Core; 
public interface INavigationService
{
    Task NavigateBack(); 
    Task NavigateToMainPage();
    Task NavigateTo<T>(); 
}
