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
    [TemplatePart(Name = c_PART_RedValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = c_PART_GreenValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = c_PART_BlueValueTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = c_PART_ResultColorBox, Type = typeof(ColorBox))]
    public class ColorPicker : ItemsControl
    {
        private const string c_PART_RedValueTextBox = "PART_RedValueTextBox";
        private const string c_PART_GreenValueTextBox = "PART_GreenValueTextBox";
        private const string c_PART_BlueValueTextBox = "PART_BlueValueTextBox";
        private const string c_PART_ResultColorBox = "PART_ResultColorBox";

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

        private static readonly Color[] s_defaultColors;

        private readonly ColorBox[] m_colorsBoxes;

        private bool m_textBoxChangedInternally;

        private TextBox? m_redValueTextBox;
        private TextBox? m_greenValueTextBox;
        private TextBox? m_blueValueTextBox;
        private ColorBox? m_resultColorBox;

        static ColorPicker()
        {
            SelectedColorProperty = DependencyProperty.Register("SelectedColor",
                                                                typeof(SolidColorBrush),
                                                                typeof(ColorPicker),
                                                                new FrameworkPropertyMetadata(
                                                                           new SolidColorBrush(Colors.Transparent),
                                                                           FrameworkPropertyMetadataOptions.AffectsRender,
                                                                           new PropertyChangedCallback(OnSelectedColorChanged))
                                                                );
            IsExpandedProperty = DependencyProperty.Register("IsExpanded",
                                                    typeof(bool),
                                                    typeof(ColorPicker),
                                                    new FrameworkPropertyMetadata(
                                                                false,
                                                                FrameworkPropertyMetadataOptions.AffectsRender,
                                                                new PropertyChangedCallback(OnIsExpandedProperyChanged),
                                                                new CoerceValueCallback(CoerceIsExpandedValue))
                                                    );
            ColorBoxTemplateProperty = DependencyProperty.Register("ColorBoxTemplate",
                                                          typeof(ControlTemplate),
                                                          typeof(ColorPicker),
                                                          new FrameworkPropertyMetadata(
                                                                     new ControlTemplate(),
                                                                     FrameworkPropertyMetadataOptions.AffectsRender,
                                                                     new PropertyChangedCallback(OnColorBoxTemplateChanged))
                                                          );

            IsEnabledProperty.OverrideMetadata(typeof(ColorPicker), new UIPropertyMetadata(new PropertyChangedCallback(OnEnbaledChanged)));

            ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceToolTipIsEnabled)));

            DropDownClosedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownClosed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));
            DropDownOpenedEvent = EventManager.RegisterRoutedEvent(nameof(DropDownOpened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorPicker));
            SelectedColorChagnedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedColorChagned), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<SolidColorBrush>), typeof(ColorPicker));
            ColorBoxTemplateChangedEvent = EventManager.RegisterRoutedEvent(nameof(ColorBoxTemplateChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ControlTemplate>), typeof(ColorPicker));

            s_defaultColors = GetDefaultColors();
        }

        public ColorPicker() : base()
        {
            m_colorsBoxes = new ColorBox[s_defaultColors.Length];
            m_textBoxChangedInternally = false;

            InitializaDefaultColors();
        }

        #region Dependecy properties implemetation

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public SolidColorBrush SelectedColor
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

        public new ColorBox[] Items { get { return m_colorsBoxes; } }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_redValueTextBox != null)
            {
                m_redValueTextBox.KeyDown -= TextBox_KeyDown;
                m_redValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (m_greenValueTextBox != null)
            {
                m_greenValueTextBox.KeyDown -= TextBox_KeyDown;
                m_greenValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (m_blueValueTextBox != null)
            {
                m_blueValueTextBox.KeyDown -= TextBox_KeyDown;
                m_blueValueTextBox.TextChanged -= TextBox_TextChanged;
            }

            if (m_resultColorBox != null)
                m_resultColorBox.MouseUp -= ColorBox_MouseUp;

            m_redValueTextBox = GetTemplateChild(c_PART_RedValueTextBox) as TextBox;
            m_greenValueTextBox = GetTemplateChild(c_PART_GreenValueTextBox) as TextBox;
            m_blueValueTextBox = GetTemplateChild(c_PART_BlueValueTextBox) as TextBox;
            m_resultColorBox = GetTemplateChild(c_PART_ResultColorBox) as ColorBox;

            if (m_redValueTextBox != null)
            {
                m_redValueTextBox.KeyDown += TextBox_KeyDown;
                m_redValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (m_greenValueTextBox != null)
            {
                m_greenValueTextBox.KeyDown += TextBox_KeyDown;
                m_greenValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (m_blueValueTextBox != null)
            {
                m_blueValueTextBox.KeyDown += TextBox_KeyDown;
                m_blueValueTextBox.TextChanged += TextBox_TextChanged;
            }

            if (m_resultColorBox != null)
            {
                m_resultColorBox.MouseUp += ColorBox_MouseUp;
                m_resultColorBox.Template = ColorBoxTemplate;
            }
        }

        private void InitializaDefaultColors()
        {
            base.Items.Clear();

            for (int i = 0; i < m_colorsBoxes.Length; i++)
            {
                var colorBox = new ColorBox(new SolidColorBrush(s_defaultColors[i]), ColorBoxTemplate);

                colorBox.MouseUp += ColorBox_MouseUp;
                colorBox.Height = 15;
                colorBox.Width = 15;

                m_colorsBoxes[i] = colorBox;
                base.Items.Add(colorBox);
            }
        }

        private void ChangeColorBoxesTemplate()
        {
            foreach (var colorBox in m_colorsBoxes)
            {
                colorBox.Template = ColorBoxTemplate;
            }

            if (m_resultColorBox != null)
                m_resultColorBox.Template = ColorBoxTemplate;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_textBoxChangedInternally)
                return;

            int value = 0;
            var textBox = sender as TextBox;
            var tag = textBox!.Tag?.ToString().ToUpper() ?? string.Empty;

            if (Int32.TryParse(textBox.Text, out value))
            {
                if (value > 255)
                {
                    value = 255;

                    m_textBoxChangedInternally = true;

                    textBox.Text = value.ToString();
                    textBox.CaretIndex = textBox.Text.Length;

                    m_textBoxChangedInternally = false;
                }

                var currentColor = m_resultColorBox.Color.Color;
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

                m_resultColorBox.Color = new SolidColorBrush(currentColor);
            }
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

        private void Parrent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOver)
                IsExpanded = false;
        }

        private static Color[] GetDefaultColors()
        {
            var colorsContainerType = typeof(Colors);
            var props = colorsContainerType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var result = new List<Color>();

            for (int i = 0; i < props.Length; i++)
            {
                var currentColor = (Color)props[i].GetValue(null);

                if (currentColor.A != 0)
                    result.Add(currentColor);
            }

            return result.OrderBy(color => Math.Sqrt(0.241 * color.R + 0.691 * color.G + 0.068 * color.B)).ToArray();
        }

        private static DependencyObject GetTopmostParrent(DependencyObject dependencyObject)
        {
            var parrent = LogicalTreeHelper.GetParent(dependencyObject);

            if (parrent != null)
                return GetTopmostParrent(parrent);
            else
                return dependencyObject;
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

        private static void OnEnbaledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = d as ColorPicker;

            colorPicker.IsExpanded = false;

            foreach (var colorBox in colorPicker.m_colorsBoxes)
            {
                colorBox.IsEnabled = (bool)e.NewValue;
            }
        }
        private static void OnIsExpandedProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            var colorPicker = (ColorPicker)d;

            var routedEventArgs = new RoutedEventArgs(newValue ? DropDownOpenedEvent : DropDownClosedEvent, colorPicker);

            colorPicker.RaiseEvent(routedEventArgs);
        }
        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (SolidColorBrush)e.NewValue;
            var oldValue = (SolidColorBrush)e.OldValue;
            var colorPicker = (ColorPicker)d;

            var routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<SolidColorBrush>(newValue, oldValue);
            routedPropertyChangedEventArgs.RoutedEvent = SelectedColorChagnedEvent;

            colorPicker.RaiseEvent(routedPropertyChangedEventArgs);
        }
        private static void OnColorBoxTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (ControlTemplate)e.NewValue;
            var oldValue = (ControlTemplate)e.OldValue;
            var colorPicker = d as ColorPicker;

            var routedPropertyChangedEventArgs = new RoutedPropertyChangedEventArgs<ControlTemplate>(newValue, oldValue);
            routedPropertyChangedEventArgs.RoutedEvent = SelectedColorChagnedEvent;

            colorPicker.RaiseEvent(routedPropertyChangedEventArgs);

            colorPicker.ChangeColorBoxesTemplate();
        }
    }

    public class ColorBox : Control
    {
        private static readonly SolidColorBrush defaultSolidColorBrush;

        public static readonly DependencyProperty ColorProperty;

        static ColorBox()
        {
            defaultSolidColorBrush = new SolidColorBrush(Colors.Transparent);

            ColorProperty = DependencyProperty.Register("Color",
                                               typeof(SolidColorBrush),
                                               typeof(ColorBox),
                                               new FrameworkPropertyMetadata(
                                                           defaultSolidColorBrush,
                                                           FrameworkPropertyMetadataOptions.AffectsRender)
                                               );
        }

        public ColorBox() : this(new SolidColorBrush(Colors.Transparent), null)
        { }
        public ColorBox(SolidColorBrush color) : this(color, null)
        { }
        public ColorBox(SolidColorBrush color, ControlTemplate? template)
        {
            Template = template;
            Color = color;
        }

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
    }

    //public class ColorPickerDropDown : Control
    //{
    //    private static readonly Color[] s_colors;
    //    private static readonly double s_colorBoxWidth;
    //    private static readonly double s_colorBoxHeight;
    //    private static readonly Thickness s_colorBoxMargin;

    //    internal struct Box
    //    {
    //        private double m_offsetLeft;
    //        private double m_offsetRight;
    //        private double m_height;
    //        private double m_width;

    //        private SolidColorBrush? m_fillColor;
    //        private Pen? m_pen;

    //        public Box(double height, double width, double offsetLeft, double offsetRight,
    //                   SolidColorBrush? fillColor, Pen? pen)
    //        {
    //            m_offsetLeft = offsetLeft;
    //            m_offsetRight = offsetRight;
    //            m_height = height;
    //            m_width = width;

    //            m_fillColor = fillColor;
    //            m_pen = pen;
    //        }

    //        public bool HitTest(double x, double y)
    //        {
    //            return m_offsetLeft < x && x < m_offsetLeft + m_width &&
    //                   m_offsetRight < y && y < m_offsetRight + m_height;
    //        }

    //        public void Draw(DrawingContext drawingContext)
    //        {
    //            drawingContext.DrawRectangle(m_fillColor, m_pen, new Rect(m_offsetLeft, m_offsetRight, m_width, m_height));
    //        }
    //    }

    //    private Box[] m_colorBoxes = null;

    //    static ColorPickerDropDown()
    //    {
    //        //s_colors = GetDefaultColors();

    //        s_colorBoxWidth = 15;
    //        s_colorBoxHeight = 15;

    //        s_colorBoxMargin = new Thickness(5, 5, 0, 0);
    //    }

    //    public void Render(DrawingContext drawingContext)
    //    {
    //        if (Height == 0 || Width == 0)
    //            return;

    //        var boxesInLineCapacity = (int)Math.Round((Width - s_colorBoxMargin.Left) / (s_colorBoxWidth + s_colorBoxMargin.Left), 0);
    //        var linesCapacity = s_colors.Length % boxesInLineCapacity == 0 ? s_colors.Length / boxesInLineCapacity : s_colors.Length / boxesInLineCapacity + 1;

    //        if (linesCapacity == 0)
    //            return;

    //        m_colorBoxes = new Box[s_colors.Length];

    //        var backgroundHeight = linesCapacity * (s_colorBoxHeight + s_colorBoxMargin.Top);

    //        var background = new LinearGradientSolidColorBrush(Colors.White, Color.FromRgb(40, 40, 40), 90);

    //        drawingContext.DrawRoundedRectangle(background, null, new Rect(0, Height - 8, Width, backgroundHeight + 20), 5, 5);

    //        for (int i = 0; i < linesCapacity; i++)
    //        {
    //            for (int j = 0; j < boxesInLineCapacity && i * boxesInLineCapacity + j < s_colors.Length; j++)
    //            {
    //                var brush = new SolidColorBrush();
    //                brush.Color = s_colors[i * boxesInLineCapacity + j];

    //                Box box = new Box(s_colorBoxHeight, s_colorBoxWidth,
    //                                  s_colorBoxMargin.Left / 2 + j * (s_colorBoxMargin.Left + s_colorBoxWidth),
    //                                  Height + 3 + i * (s_colorBoxMargin.Top + s_colorBoxHeight),
    //                                  brush, null);

    //                m_colorBoxes[i * boxesInLineCapacity + j] = box;

    //                box.Draw(drawingContext);

    //                //new Rect(s_colorBoxMargin.Left + j * (s_colorBoxMargin.Left + s_colorBoxWidth),
    //                //                                            Height + 3 + i * (s_colorBoxMargin.Top + s_colorBoxHeight),
    //                //                                            s_colorBoxHeight, s_colorBoxHeight))
    //            }
    //        }
    //    }

    //    protected override void OnRender(DrawingContext drawingContext)
    //    {
    //        base.OnRender(drawingContext);

    //        Render(drawingContext);
    //    }
    //}
}
