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

		private void MHook_KeyDownEvent(int keyCode)
		{
			if (!Properties.Settings.Default.KeyboardHook) return;

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

		private void InjectionSelectItem(object sender, RoutedEventArgs e)
		{
			var items = ListBoxFilterItem.SelectedItems as IEnumerable;
			if (items == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;
			vm.InjectionItemInfo(items);
		}

		private void ListBoxBackpack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var backpack = (sender as ListBox)?.SelectedItem as Backpack;
			if (backpack == null) return;

			var vm = DataContext as ViewModel;
			if (vm == null) return;

			vm.BackpaktoBag(backpack);
		}
	}
}
