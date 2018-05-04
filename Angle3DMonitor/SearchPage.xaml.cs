using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Xamarin.Forms;
using Urho.Resources;
            
namespace Angle3DMonitor
{
    public partial class SearchPage : ContentPage
    {
        IBluetoothLE _BluetoothLe;
        IAdapter _Adapter;
        private ObservableCollection<IDevice> _deviceList = new ObservableCollection<IDevice>();

        public SearchPage()
        {
            InitializeComponent();

            ListView1.ItemsSource = _deviceList;
            _BluetoothLe = Plugin.BLE.CrossBluetoothLE.Current;
            _Adapter = _BluetoothLe.Adapter;
            _Adapter.ScanTimeout = 5000;
            _Adapter.DeviceDiscovered += (s, e) =>
            {
                var device = e.Device;
                _deviceList.Add(device);
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _deviceList.Clear();
            var devices = _Adapter.ConnectedDevices;
            foreach (var device in devices)
            {
                await _Adapter.DisconnectDeviceAsync(device);
            }
            if (_BluetoothLe.State != BluetoothState.Off)
            {
                connectingIndicator.IsRunning = true;
                await _Adapter.StartScanningForDevicesAsync(null, dev => {
                    return (dev.Name == null) ? false : dev.Name.Equals("Angle Meter");
                });
                connectingIndicator.IsRunning = false;
            }
        }

        async void ListView1_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var device = e.SelectedItem as IDevice;
            if (device == null) {
                return;
            }

            try
            {
                connectingIndicator.IsRunning = true;
                await _Adapter.ConnectToDeviceAsync(device);
                connectingIndicator.IsRunning = false;
                if (device.State == DeviceState.Connected)
                {
                    var service = await device.GetServiceAsync(Guid.Parse("bc6da0e6-3cbe-11e8-b467-0ed5f89f718b"));
                    var characteristic = await service.GetCharacteristicAsync(Guid.Parse("cd0a93d6-07c3-4c1a-b16d-b38694b2a715"));
                    if (characteristic.CanRead)
                    {
                        await characteristic.ReadAsync();
                    }
                    await Navigation.PushAsync(new UrhoPage(characteristic));
                }
            }
            catch (DeviceConnectionException ex)
            {
                Debug.WriteLine(ex.StackTrace);
                await DisplayAlert("Angle 3D Monitor", "Do Not Connect", "OK");
            }
            finally
            {
                ListView1.SelectedItem = null;
            }
        }
    }
}
