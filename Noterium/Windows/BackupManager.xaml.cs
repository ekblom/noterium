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
using Noterium.ViewModels;

namespace Noterium.Windows
{
    /// <summary>
    /// Interaction logic for BackupManager.xaml
    /// </summary>
    public partial class BackupManager
    {
        public BackupManagerViewModel Model => (BackupManagerViewModel) DataContext;

        public BackupManager()
        {
            InitializeComponent();
        }

        private void BackupTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Model.SelectedFileNode = e.NewValue as FileTreeNode;
        }
    }
}
