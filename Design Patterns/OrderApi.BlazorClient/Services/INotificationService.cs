namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: ISP][SRP] Exposes only application notification operations.</summary>
public interface INotificationService
{
    /// <summary>[ISP] Displays a success notification.</summary>
    void ShowSuccess(string message);

    /// <summary>[ISP] Displays an informational notification.</summary>
    void ShowInfo(string message);

    /// <summary>[ISP] Displays a warning notification.</summary>
    void ShowWarning(string message);

    /// <summary>[ISP] Displays an error notification.</summary>
    void ShowError(string message);
}