using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace WpfCustomControlLibrary1
{
    [TemplatePart(Name = "Main", Type = typeof(Border))]
    [TemplatePart(Name = "body", Type = typeof(ContentControl))]
    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
            CommandManager.RegisterClassCommandBinding(typeof(CustomControl1), new CommandBinding(CustomControl1.CustomCommand, C_Command));
            EventManager.RegisterClassHandler(typeof(CustomControl1), Mouse.MouseDownEvent, new MouseButtonEventHandler(M_Down));
        }
        public static readonly DependencyProperty C_prt = DependencyProperty.Register("Color", typeof(Color), typeof(CustomControl1), new PropertyMetadata(Colors.AliceBlue));
        public Color Color
        {
            get
            {
                return (Color)this.GetValue(C_prt);
            }
            set
            {
                this.SetValue(C_prt, value);
            }
        }
        Border MB;
        ContentControl Body;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Template != null)
            {
                Border mainBorder = this.Template.FindName("Main", this) as Border;
                if (mainBorder != MB)
                {
                    //Firstly you have to unhook existing handler
                    if (MB != null)
                    {
                        MB.MouseEnter -= new MouseEventHandler(MB_MEnter);
                        MB.MouseLeave -= new MouseEventHandler(MB_MLeave);
                    }
                    MB = mainBorder;
                    if (MB != null)
                    {
                        // Now we have to Add a default basecolor
                        MB.Background = new LinearGradientBrush(this.Color, this.Color, .5);
                        MB.MouseEnter += new MouseEventHandler(MB_MEnter);
                        MB.MouseLeave += new MouseEventHandler(MB_MLeave);
                    }
                }
                Body = this.Template.FindName("body", this) as ContentControl;
            }
        }
        void MB_MLeave(object sender, MouseEventArgs e)
        {
            Border thisBorder = sender as Border;
            if (thisBorder != null)
            {
                thisBorder.Background = new SolidColorBrush(Colors.HotPink);
                if (Body != null)
                {
                    Run r = new Run("Mouse Has Been Left!");
                    r.Foreground = new SolidColorBrush(Colors.Yellow);
                    Body.Content = r;
                }
            }
        }
        void MB_MEnter(object sender, MouseEventArgs e)
        {
            Border thisBorder = sender as Border;
            if (thisBorder != null)
            {
                thisBorder.Background = new SolidColorBrush(Colors.Tomato);
                if (Body != null)
                {
                    Run r = new Run("Mouse Has Entered!");
                    r.Foreground = new SolidColorBrush(Colors.Silver);
                    Body.Content = r;
                }
            }
        }
        static void M_Down(object sender, MouseButtonEventArgs e)
        {
            CustomControl1 invoker = sender as CustomControl1;
            //Do handle event
            //Raise your event
            invoker.OnInvertCall();
            //Do Rest
        }
        public static readonly RoutedEvent InvertCallEvent = EventManager.RegisterRoutedEvent("InvertCall", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomControl1));
        public event RoutedEventHandler InvertCall
        {
            add { AddHandler(InvertCallEvent, value); }
            remove { RemoveHandler(InvertCallEvent, value); }
        }
        private void OnInvertCall()
        {
            RoutedEventArgs args = new RoutedEventArgs(InvertCallEvent);
            RaiseEvent(args);
        }
        static void C_Command(object sender, ExecutedRoutedEventArgs e)
        {
            //Need to first retrieve the control
            CustomControl1 invoker = sender as CustomControl1;
            //Do whatever you need
        }
        public static readonly ICommand CustomCommand = new RoutedUICommand("CustomCommand", "CustomCommand" typeof(CustomControl1), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.Enter), new MouseGesture(MouseAction.LeftClick) }));
    }
}