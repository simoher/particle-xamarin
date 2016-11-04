﻿using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Particle;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.Generic;
using Akavache;
using EvolveApp.ViewModels;

namespace EvolveApp.ViewModels
{
	public class MyDevicesViewModel : BaseViewModel
	{
		public MyDevicesViewModel()
		{
			LastRefresh = DateTime.Now.ToString();
		}

		Command refreshCommand;
		public ICommand RefreshCommand
		{
			get
			{
				if (refreshCommand == null)
				{
					refreshCommand = new Command(HandleRefresh);
				}

				return refreshCommand;
			}
		}

		async void HandleRefresh(object parameter)
		{
			await GetDevicesAsync();
			Refreshing = false;
			LastRefresh = DateTime.Now.ToString();
		}

		public string lastRefresh;
		public string LastRefresh
		{
			get { return lastRefresh; }
			set
			{
				lastRefresh = "Last Updated: " + DateTime.Now.ToString();
				OnPropertyChanged("LastRefresh");
			}
		}

		public bool isRefreshing = false;
		public bool Refreshing
		{
			get { return isRefreshing; }
			internal set
			{
				isRefreshing = value;
				OnPropertyChanged("Refreshing");
			}
		}

		ObservableCollection<ParticleDevice> devices;
		public ObservableCollection<ParticleDevice> Devices
		{
			get { return devices; }
			internal set
			{
				if (value == devices)
					return;
				devices = value;
				OnPropertyChanged("Devices");
			}
		}

		DateTime devicesLastRefreshed;
		public DateTime DeviceListLastRefreshed
		{
			get { return devicesLastRefreshed; }
			internal set
			{
				if (value == devicesLastRefreshed)
					return;
				devicesLastRefreshed = value;
				OnPropertyChanged("DeviceListLastRefreshed");
			}
		}

		public async Task GetDevicesAsync()
		{
			Refreshing = true;
			var devices = await ParticleCloud.SharedInstance.GetDevicesAsync();
			Devices = new ObservableCollection<ParticleDevice>(devices);
			DeviceListLastRefreshed = DateTime.Now;
			Refreshing = false;
		}
	}
}