using System;
using System.Windows;

namespace GTA_SA_PathsRedactor.Services;

public enum QuestionResults
{
    Yes,
    No,
    Cancel
}

public sealed class NotificationService
{
    public void NotifyInformation(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    public void NotifyWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void NotifyError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    public QuestionResults NotifyQuestion(string message, string title)
    {
        return NotifyQuestionInternal(message, title, MessageBoxButton.YesNoCancel);
    }

    public QuestionResults NotifyQuestionYesNo(string message, string title)
    {
        return NotifyQuestionInternal(message, title, MessageBoxButton.YesNo);
    }
    
    private static QuestionResults NotifyQuestionInternal(string message, string title, MessageBoxButton messageBoxButton)
    {
        var result = MessageBox.Show(message, title, messageBoxButton, MessageBoxImage.Question);

        return result switch
        {
            MessageBoxResult.Cancel => QuestionResults.Cancel,
            MessageBoxResult.Yes => QuestionResults.Yes,
            MessageBoxResult.No => QuestionResults.No,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}