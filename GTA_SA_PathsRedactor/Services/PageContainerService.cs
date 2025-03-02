using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GTA_SA_PathsRedactor.Services;

public interface IPageContainerService<TPage>
{
    TPage? CurrentPage { get; }
    ICommand NavigateToPageCommand { get; }
    void AddPage(string name, TPage page);
    bool RemovePage(string name);
    void NavigateTo(string? name);
}

public sealed class PageContainerService<TPage> : ObservableObject, IPageContainerService<TPage>
{
    private readonly Dictionary<string, TPage> _pages = new();
    private TPage? _currentPage;
    
    private ICommand? _navigateToPageCommand;

    public TPage? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }
    
    public ICommand NavigateToPageCommand => _navigateToPageCommand
        ??= new RelayCommand<string>(NavigateTo, name => !string.IsNullOrWhiteSpace(name));

    public void AddPage(string name, TPage page)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty.", nameof(name));

        if (!_pages.TryAdd(name, page))
            throw new InvalidOperationException($"Page with name '{name}' already exists.");
        
        CurrentPage ??= page;
    }

    public bool RemovePage(string name)
    {
        var result = _pages.Remove(name, out var page);
        
        if (CurrentPage?.Equals(page) == true)
            CurrentPage = default;

        return result;
    }

    public void NavigateTo(string? name)
    {
        if (!_pages.TryGetValue(name!, out var page))
            throw new InvalidOperationException($"Page with name '{name}' does not exist.");
        
        CurrentPage = page;
    }
}