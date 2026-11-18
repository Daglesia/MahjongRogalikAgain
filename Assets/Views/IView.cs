using System;

public interface IView
{
    bool IsOpen { get; }

    /// Fades in and shows the view. onClosed fires after the closing fade has finished.
    void Open(RunState state, Action onClosed = null);

    /// Fades out and hides the view.
    void Close();

    /// Applies the result of the view to the run's deck (add / remove / replace cards).
    void UpdateDeck(RunState state);
}