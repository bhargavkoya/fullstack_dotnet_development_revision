using MudBlazor;

namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: Decorator][SRP][DIP] Wraps MudBlazor snackbar notifications with consistent formatting.</summary>
public sealed class NotificationService : INotificationService
{
    private readonly ISnackbar _snackbar;

    public NotificationService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    /// <inheritdoc />
    public void ShowSuccess(string message) => Show(message, Severity.Success);

    /// <inheritdoc />
    public void ShowInfo(string message) => Show(message, Severity.Info);

    /// <inheritdoc />
    public void ShowWarning(string message) => Show(message, Severity.Warning);

    /// <inheritdoc />
    public void ShowError(string message) => Show(message, Severity.Error);

    private void Show(string message, Severity severity)
    {
        var normalizedMessage = string.IsNullOrWhiteSpace(message)
            ? "An unexpected notification was triggered."
            : message.Trim();

        _snackbar.Add(
            normalizedMessage,
            severity,
            options =>
            {
                options.VisibleStateDuration = 4000;
                options.ShowCloseIcon = true;
                options.HideIcon = false;
            });
    }
}