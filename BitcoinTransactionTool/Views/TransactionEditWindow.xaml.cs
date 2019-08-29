﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BitcoinTransactionTool.Views
{
    /// <summary>
    /// Interaction logic for TransactionEditWindow.xaml
    /// </summary>
    public partial class TransactionEditWindow : Window
    {
        public TransactionEditWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            try {
                Window.GetWindow(this)?.DragMove();
            }
            catch {
                //Do Nothing
            }
            e.Handled = false;
        }
    }
}
