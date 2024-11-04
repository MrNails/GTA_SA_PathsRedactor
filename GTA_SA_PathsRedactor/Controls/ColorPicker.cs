using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace GTA_SA_PathsRedactor.Controls
{
    [TemplatePart(Name = PART_RedValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_GreenValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_BlueValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_ResultColorBox, Type = typeof(ColorBox))]
    public class ColorPicker : ItemsControl
    {
        private const string PART_RedValueTextBox = "PART_RedValueTextBox";
        private const string PART_GreenValueTextBox = "PART_GreenValueTextBox";
        private const string PART_BlueValueTextBox = "PART_BlueValueTextBox";
        private const string PART_ResultColorBox = "PART_ResultColorBox";

        #region Dependecy properties definition

        public static readonly DependencyProperty IsExpandedProperty;
        public static readonly DependencyProperty SelectedColorProperty;
        public static readonly DependencyProperty ColorBoxTemplateProperty;

        #endregion

        #region Routed events definition

        public static readonly RoutedEvent DropDownOpenedEvent;
        public static readonly RoutedEvent DropDownClosedEvent;
        public static readonly RoutedEvent SelectedColorChagnedEvent;
        public static readonly RoutedEvent ColorBoxTemplateChangedEvent;

        #endregion

        private static readonly Color[] DefaultColors;

        private readonly ColorBox[] _colorsBoxes;

        private bool _textBoxChangedInternally;

        private TextBox? _redValueTextBox;
        private TextBox? _greenValueTextBox;
        private TextBox? _blueValueTextBox;
        private ColorBox? _resultColorBox;

        static ColorPicker()
        {
            SelectedColorProperty = DependencyProperty.Register(nameof(SelectedColor),
                                                                typeof(SolidColorBrush),
                                                                typeof(ColorPicker),
                                                                new FrameworkPropertyMetadata(
                                                                           new SolidColorBrush(Colors.Transparent),
                                                                           FrameworkPropertyMetadataOptions.AffectsRender,
                                                                           OnSelectedColorChanged)
                                                                );
            IsExpandedProperty = DependencyProperty.Register(nameof(IsExpanded),
                                                    typeof(bool),
                                                    typeof(ColorPicker),
                                                    new FrameworkPropertyMetadata(
                                                                false,
                                                                FrameworkPropertyMetadataOptions.AffectsRender,
                                                                OnIsExpandedPropertyChanged,
                                                                CoerceIsExpandedValue)
                                                    );
            ColorBoxTemplateProperty = DependencyProperty.Register(nameof(ColorBoxTemplate),
                                                          typeof(ControlTemplate),
                                                          typeof(ColorPicker),
                                                          new FrameworkPropertyMetadata(
                                                                     new ControlTemplate(),
                                                                     FrameworkPropertyMetadataOptions.AffectsRender,
                                                                     OnColorBoxTemplateChanged)
                                                          );

            IsEnabledProperty.OverrideMetadata(typeof(ColorPicker), new UIPropertyMetadata(OnEnabledChanged));

            ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(null, CoerceToolTipIsEnabled));

            DropDownClosedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownClosed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));
            DropDownOpenedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownOpened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));
            SelectedColorChagnedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedColorChagned), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(ColorPicker));
            ColorBoxTemplateChangedEvent = EventManager.RegisterRoutedEvent(nameof(ColorBoxTemplateChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ControlTemplate>), typeof(ColorPicker));

            DefaultColors = GetDefaultColors();
        }

        public ColorPicker()
        {
            _colorsBoxes = new ColorBox[DefaultColors.Length];
            _textBoxChangedInternally = false;

            InitializeDefaultColors();
        }

        #region Dependecy properties implemetation

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public Brush SelectedColor
        {
            get { return (SolidColorBrush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public ControlTemplate ColorBoxTemplate
        {
            get { return (ControlTemplate)GetValue(ColorBoxTemplateProperty); }
            set { SetValue(ColorBoxTemplateProperty, value); }
        }

        #endregion

        #region Routed events impementation

        public event RoutedEventHandler DropDownOpened
        {
            add { AddHandler(DropDownOpenedEvent, value); }
            remove { RemoveHandler(DropDownOpenedEvent, value); }
        }
        public event RoutedEventHandler DropDownClosed
        {
            add { AddHandler(DropDownClosedEvent, value); }
            remove { RemoveHandler(DropDownClosedEvent, value); }
        }

        public event RoutedPropertyChangedEventHandler<SolidColorBrush> SelectedColorChagned
        {
            add { AddHandler(SelectedColorChagnedEvent, value); }
            remove { RemoveHandler(SelectedColorChagnedEvent, value); }
        }
        public event RoutedPropertyChangedEventHandler<ControlTemplate> ColorBoxTemplateChanged
        {
            add { AddHandler(ColorBoxTemplateChangedEvent, value); }
            remove { RemoveHandler(ColorBoxTemplateChangedEvent, value); }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_redValueTextBox != null)
            {
                _redValueTextBox.KeyDown -= TextBox_KeyDown;
                _redValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (_greenValueTextBox != null)
            {
                _greenValueTextBox.KeyDown -= TextBox_KeyDown;
                _greenValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (_blueValueTextBox != null)
            {
                _blueValueTextBox.KeyDown -= TextBox_KeyDown;
                _blueValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (_resultColorBox != null)
                _resultColorBox.MouseUp -= ColorBox_MouseUp;

            _redValueTextBox = GetTemplateChild(PART_RedValueTextBox) as TextBox;
            _greenValueTextBox = GetTemplateChild(PART_GreenValueTextBox) as TextBox;
            _blueValueTextBox = GetTemplateChild(PART_BlueValueTextBox) as TextBox;
            _resultColorBox = GetTemplateChild(PART_ResultColorBox) as ColorBox;

            if (_redValueTextBox != null)
            {
                _redValueTextBox.KeyDown += TextBox_KeyDown;
                _redValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (_greenValueTextBox != null)
            {
                _greenValueTextBox.KeyDown += TextBox_KeyDown;
                _greenValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (_blueValueTextBox != null)
            {
                _blueValueTextBox.KeyDown += TextBox_KeyDown;
                _blueValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (_resultColorBox != null)
            {
                _resultColorBox.MouseUp += ColorBox_MouseUp;
                _resultColorBox.Template = ColorBoxTemplate;
            }
        }

        private void InitializeDefaultColors()
        {
            base.Items.Clear();

            for (int i = 0; i < _colorsBoxes.Length; i++)
            {
                var colorBox = new ColorBox { Color = new SolidColorBrush(DefaultColors[i]), Template = ColorBoxTemplate};

                colorBox.MouseUp += ColorBox_MouseUp;
                colorBox.Height = 15;
                colorBox.Width = 15;

                _colorsBoxes[i] = colorBox;
                base.Items.Add(colorBox);
            }
        }

        private void ChangeColorBoxesTemplate()
        {
            foreach (var colorBox in _colorsBoxes)
            {
                colorBox.Template = ColorBoxTemplate;
            }

            if (_resultColorBox != null)
                _resultColorBox.Template = ColorBoxTemplate;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textBoxChangedInternally ||
                sender is not TextBox textBox ||
                !int.TryParse(textBox.Text, out var value))
                return;
            
            var tag = textBox.Tag?.ToString()?.ToUpper() ?? string.Empty;
            if (value > 255)
            {
                value = 255;

                _textBoxChangedInternally = true;

                textBox.Text = value.ToString();
                textBox.CaretIndex = textBox.Text.Length;

                _textBoxChangedInternally = false;
            }
            
            if (_resultColorBox?.Color is not SolidColorBrush currentColorBrush)
                return;

            var currentColor = currentColorBrush.Color;
            currentColor.A = 255;

            switch (tag)
            {
                case "R":
                    currentColor.R = (byte)value;
                    break;
                case "G":
                    currentColor.G = (byte)value;
                    break;
                case "B":
                    currentColor.B = (byte)value;
                    break;
                default:
                    return;
            }
            _resultColorBox.Color = new SolidColorBrush(currentColor);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key < Key.D0 || e.Key > Key.D9)
            {
                e.Handled = true;
            }
        }

        private void ColorBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var colorBox = (ColorBox)sender;

            SelectedColor = colorBox.Color;
        }

        private static Color[] GetDefaultColors()
        {
            var colorsContainerType = typeof(Colors);
            var props = colorsContainerType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var result = new List<Color>();

            for (int i = 0; i < props.Length; i++)
            {
                var currentColor = (Color)props[i].GetValue(null)!;

                if (currentColor.A != 0)
                    result.Add(currentColor);
            }

            return result.OrderBy(color => Math.Sqrt(0.241 * color.R + 0.691 * color.G + 0.068 * color.B)).ToArray();
        }

        private static object CoerceToolTipIsEnabled(DependencyObject d, object value)
        {
            var cp = (ColorPicker)d;
            return cp.IsExpanded ? false : value;
        }

        private static object CoerceIsExpandedValue(DependencyObject d, object baseValue)
        {
            var colorPicker = (ColorPicker)d;

            if (!colorPicker.IsEnabled)
                return false;

            return baseValue;
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            colorPicker.IsExpanded = false;

            if (e.NewValue is not bool newValue) 
                return;
            
            foreach (var colorBox in colorPicker._colorsBoxes)
            {
                colorBox.IsEnabled = newValue;
            }
        }
        private static void OnIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            var colorPicker = (ColorPicker)d;

            var routedEventArgs = new RoutedEventArgs(newValue ? DropDownOpenedEvent : DropDownClosedEvent, colorPicker);

            colorPicker.RaiseEvent(routedEventArgs);
        }
        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as Brush;
            var oldValue = e.OldValue as Brush;
            var colorPicker = (ColorPicker)d;

            var routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<Brush?>(newValue, oldValue);
            routedPropertyChangedEventArgs.RoutedEvent = SelectedColorChagnedEvent;

            colorPicker.RaiseEvent(routedPropertyChangedEventArgs);
        }
        private static void OnColorBoxTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as ControlTemplate;
            var oldValue = e.OldValue as ControlTemplate;
            
            var colorPicker = (ColorPicker)d;

            var routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<ControlTemplate?>(newValue, oldValue)
            {
                RoutedEvent = SelectedColorChagnedEvent
            };

            colorPicker.RaiseEvent(routedPropertyChangedEventArgs);
            colorPicker.ChangeColorBoxesTemplate();
        }
    }

    public sealed class ColorBox : Control
    {
        public static readonly DependencyProperty ColorProperty = 
            DependencyProperty.Register(nameof(Color),
                                        typeof(SolidColorBrush),
                                        typeof(ColorBox),
                                        new FrameworkPropertyMetadata(
                                            Brushes.Black,
                                            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Color
        {
            get => (SolidColorBrush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
    }
}
