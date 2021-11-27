using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace DQB2ProcessEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.Save();
		}

		private void FilterItem(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.FilterItem();
		}

		private void InjectionItem(object sender, RoutedEventArgs e)
		{
			var dc = (sender as FrameworkElement)?.DataContext;
			IEnumerable items = dc as IEnumerable;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItem(items);
		}

		private void InjectionSelectItem(object sender, RoutedEventArgs e)
		{
			var items = ListBoxFilterItem.SelectedItems as IEnumerable;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItem(items);
		}

		private void ButtonWriteItemCount_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.WriteInventoryItemCount();
		}

		private void ListBoxBackpack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var backpack = (sender as ListBox)?.SelectedItem as Backpack;
			if (backpack == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.BackpaktoBag(backpack);
		}

		private void ButtonClearInventory_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.ClearItem(ProcessMemory.CarryType.eInventory);
		}

		private void ButtonClearBag_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.ClearItem(ProcessMemory.CarryType.eBag);
		}

		private void ButtonClearBagPage_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.ClearItem();
		}

        private void ButtonImportBluePrintItem_Click(object sender, RoutedEventArgs e)
        {
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog() == false) return;

			vm.ImportBluePrintItem(dlg.FileName);
		}

		private void ButtonImportBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			var dlg = new OpenFileDialog();
			if (dlg.ShowDialog() == false) return;

			vm.ImportBluePrint(dlg.FileName);
		}

		private void ButtonExportBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			var dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == false) return;

			vm.ExportBluePrint(dlg.FileName);
		}

		private void ButtonClearBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.ClearBluePrint();
		}

		private void ButtonReloadInfo_Click(object sender, RoutedEventArgs e)
        {
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.LoadInfo();
		}
	}
}
