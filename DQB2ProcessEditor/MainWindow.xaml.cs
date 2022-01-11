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
		private KeyboardHook mHook = new KeyboardHook();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			mHook.KeyDownEvent += MHook_KeyDownEvent;
			mHook.Hook();
		}

		private void MHook_KeyDownEvent(int keyCode)
		{
			if (Properties.Settings.Default.KeyboardHook)
			{
				var vm = DataContext as ViewModel;
				if (vm == null) return;

				switch (keyCode)
				{
					case 112:   // F1
						vm.WriteInventoryItemCount();
						break;

					case 113:   // F2
						vm.WriteBagItemCount();
						break;

					case 121:   // F10
						if (Info.GetInstance().ItemTemplate.Count > 0)
						{
							vm.InjectionItem(Info.GetInstance().ItemTemplate[0].Items);
						}
						break;

					case 122:   // F11
						if (Info.GetInstance().ItemTemplate.Count > 1)
						{
							vm.InjectionItem(Info.GetInstance().ItemTemplate[1].Items);
						}
						break;

					default:
						break;
				}
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			mHook.UnHook();
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
			var items = dc as IEnumerable;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItemInfo(items);
		}

		private void InjectionSelectItem(object sender, RoutedEventArgs e)
		{
			var items = ListBoxFilterItem.SelectedItems as IEnumerable;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItemInfo(items);
		}

		private void ButtonImportTemplateItem_Click(object sender, RoutedEventArgs e)
		{
			var dc = (sender as FrameworkElement)?.DataContext;
			var items = dc as List<Item>;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItem(items);
		}

		private void ButtonWriteItemCount_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.WriteItemCount();
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

		private void ButtonImportBluePrintMemoryItem_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			int index = 0;
			int.TryParse((sender as Button)?.DataContext.ToString(), out index);

			vm.ImportBluePrintItem(index);
		}

		private void ButtonImportBluePrintFileItem_Click(object sender, RoutedEventArgs e)
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

			int index = 0;
			int.TryParse((sender as Button)?.DataContext.ToString(), out index);

			vm.ImportBluePrint(dlg.FileName, index);
		}

		private void ButtonExportBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			var dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == false) return;

			int index = 0;
			int.TryParse((sender as Button)?.DataContext.ToString(), out index);

			vm.ExportBluePrint(dlg.FileName, index);
		}

		private void ButtonClearBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			int index = 0;
			int.TryParse((sender as Button)?.DataContext.ToString(), out index);

			vm.ClearBluePrint(index);
		}

		private void ButtonClearAllBluePrint_Click(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			for (int index = 0; index < 4; index++)
			{
				vm.ClearBluePrint(index);
			}
		}

		private void ButtonReloadInfo_Click(object sender, RoutedEventArgs e)
        {
			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.LoadInfo();
		}
	}
}
