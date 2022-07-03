using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DQB2ProcessEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
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

		private void Window_Closed(object sender, EventArgs e)
		{
			mHook.UnHook();
			Properties.Settings.Default.Save();
		}

		private void ListBoxFilterItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			InjectionSelectItem();
		}

		private void ListBoxFilterItemContextMenu_Click(object sender, RoutedEventArgs e)
		{
			InjectionSelectItem();
		}

		private void ListBoxBackpack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var backpack = (sender as ListBox)?.SelectedItem as Backpack;
			if (backpack == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.BackpaktoBag(backpack);
		}

		private void MHook_KeyDownEvent(int keyCode)
		{
			if (!Properties.Settings.Default.KeyboardHook) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.KeyboardAction(keyCode);
		}

		private void InjectionSelectItem()
		{
			var items = ListBoxFilterItem.SelectedItems as IList;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItemInfo(items);
		}
	}
}
