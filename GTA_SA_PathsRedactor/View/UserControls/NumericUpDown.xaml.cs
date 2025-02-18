using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTA_SA_PathsRedactor.View.UserControls;

public enum SignType
{
    Minus,
    Plus,
}

public partial class NumericUpDown : UserControl
{
    private record ChangeValueData(int Milliseconds, int Delta);
    
    public static readonly DependencyProperty ValueProperty = 
        DependencyProperty.Register(nameof(Value), 
                                    typeof(double), 
                                    typeof(NumericUpDown), 
                                    new PropertyMetadata(0d));

    private static readonly Dictionary<int, ChangeValueData> ChangeValueMap_ = new()
    {
        { 0, new ChangeValueData(250, 1 ) },
        { 3, new ChangeValueData(200, 1 ) },
        { 7, new ChangeValueData(150, 1 ) },
        { 15, new ChangeValueData(150, 2 ) },
        { 18, new ChangeValueData(120, 5 ) },
        { 20, new ChangeValueData(100, 5 ) },
        { 30, new ChangeValueData(50, 10 ) },
    };
    
    private CancellationTokenSource? _cancellationTokenSource;
    
    public NumericUpDown()
    {
        InitializeComponent();
    }
    
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    private async Task ChangeValueInLoop(object? state)
    {
        if (_cancellationTokenSource is null)
            return;

        var sign = GetSignTypeAsInt((SignType)state!);
        var token = _cancellationTokenSource.Token;

        var key = 0;
        var delay = 0;
        var delta = 0;
        while (!token.IsCancellationRequested)
        {
            if (ChangeValueMap_.TryGetValue(key, out var changeValueData))
            {
                delay = changeValueData.Milliseconds;
                delta = changeValueData.Delta;
            }
            
            await Task.Delay(delay, token);
            
            if (token.IsCancellationRequested)
                return;

            Dispatcher.BeginInvoke((object arg) => Value += sign * (int)arg, delta);
            key++;
        }
    }
    
    private void NumericButtonOnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button { Tag: SignType signType } button)
            return;
        
        _cancellationTokenSource = new CancellationTokenSource();

        Value += GetSignTypeAsInt(signType);
        
        Task.Factory.StartNew(ChangeValueInLoop, button.Tag, _cancellationTokenSource.Token)
            .ConfigureAwait(true);
    }

    private void NumericButtonOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
    }
    
    private void ValueField_OnTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        
        var isNegativeStart = e.Text.StartsWith('-') && textBox.Text.Length == 0;

        if (!isNegativeStart && !double.TryParse(e.Text, out _))
        {
            e.Handled = true;
            SystemSounds.Beep.Play();
        }
    }

    private static int GetSignTypeAsInt(SignType signType)
    {
        return signType == SignType.Minus ? -1 : 1;
    }
}