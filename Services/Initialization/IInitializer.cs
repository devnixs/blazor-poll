namespace Poll.Services.Abstractions;

public interface IInitializer
{
    Task OnInitialize();
}