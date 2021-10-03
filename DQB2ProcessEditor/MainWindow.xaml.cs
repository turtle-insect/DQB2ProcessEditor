using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

		private void TextBoxItemFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.FilterItem();
		}

		private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var info = (sender as ListBox)?.SelectedItem as ItemInfo;
			if (info == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.InjectionItem(info.ID);
		}

		private void ButtonInjectionAllItem_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;
			
			vm.InjectionAllItem();
		}

		private void ButtonWriteItemCount_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.WriteInventoryItemCount();
		}

		private void ButtonWritePlayerJumpPower_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.WritePlayerJumpPower();
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
    }
}
