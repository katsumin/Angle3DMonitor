using System;
using System.Collections.Generic;
using System.Threading;
using Plugin.BLE.Abstractions.Contracts;
using Urho.Forms;
using Xamarin.Forms;
using System.Diagnostics;
using Urho;

namespace Angle3DMonitor
{
    public partial class UrhoPage : ContentPage
    {
        ICharacteristic _characteristicRcv;
        ICharacteristic _characteristicSnd;
        Charts urhoApp;
        float rotate_x = 0;
        float rotate_y = 0;
        float rotate_z = 0;

        public UrhoPage()
        {
            InitializeComponent();

            setEvents();
        }

        public UrhoPage(ICharacteristic characteristic)
        {
            _characteristicRcv = characteristic;
            InitializeComponent();

            setEvents();

            if (_characteristicRcv != null)
            {
                Title = "Angle 3D Monitor(BLE mode)";
                rotationSliderX.IsVisible = false;
                rotationSliderY.IsVisible = false;
                rotationSliderZ.IsVisible = false;
                getCharasteristicSnd();
            } else {
                Title = "Angle 3D Monitor(TEST mode)";
            }
        }

        async private void getCharasteristicSnd() {
            _characteristicSnd = await _characteristicRcv.Service.GetCharacteristicAsync(Guid.Parse("482d5c81-607c-4e34-b42a-bff1f76607c0"));
        }

        private void setEvents()
        {
            // events
            rotationSliderX.ValueChanged += (s, e) =>
            {
                rotate_x = (float)e.NewValue;
                angleY.Text = string.Format("Y: {0, 6:F1}°", rotate_x);
                Rotate(rotate_x, rotate_y, rotate_z);
            };
            rotationSliderY.ValueChanged += (s, e) =>
            {
                rotate_y = (float)e.NewValue;
                angleX.Text = string.Format("X: {0, 6:F1}°", -rotate_y);
                Rotate(rotate_x, rotate_y, rotate_z);
            };
            rotationSliderZ.ValueChanged += (s, e) =>
            {
                rotate_z = (float)e.NewValue;
                Rotate(rotate_x, rotate_y, rotate_z);
            };

            if ( _characteristicRcv != null ) {
                _characteristicRcv.ValueUpdated += (s, e) =>
                {
                    string value = e.Characteristic.StringValue;
                    char[] separator = { ',', '=' };
                    string[] parsed = value.Split(separator);
                    if (parsed.Length == 4 && parsed[0] == "a")
                    {
                        // a=xxx,xxx,xxx
                        double x_axis = double.Parse(parsed[1].Trim()) / 1000.0;
                        double y_axis = double.Parse(parsed[2].Trim()) / 1000.0;
                        double z_axis = double.Parse(parsed[3].Trim()) / 1000.0;
                        double norm = Math.Sqrt(x_axis * x_axis + y_axis * y_axis + z_axis * z_axis);
                        x_axis /= norm;
                        y_axis /= norm;
                        float x = (float)(-Math.Asin(x_axis) / Math.PI * 180.0);
                        float y = (float)(-Math.Asin(y_axis) / Math.PI * 180.0);
                        Rotate(y, -x, 0.0F);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            angleX.Text = string.Format("X: {0, 6:F1}°", x);
                            angleY.Text = string.Format("Y: {0, 6:F1}°", y);
                        });
                    }
                };
            }
        }

        private void Rotate(float x, float y, float z)
        {
            //Debug.WriteLine(string.Format("x:{0}, y:{1}, z:{2}", x, y, z));
            urhoApp?.Rotate(x, y, z);
        }

        protected override void OnDisappearing()
        {
            _characteristicRcv?.StopUpdatesAsync();
            UrhoSurface.OnDestroy();
            base.OnDisappearing();
        }

        protected override async void OnAppearing()
        {
            if ( _characteristicRcv != null ){
                await _characteristicRcv.StartUpdatesAsync();
            }
            urhoApp = await urhoSurface.Show<Charts>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.LandscapeAndPortrait });
        }

        async void Reset_Clicked(object sender, System.EventArgs e)
        {
            if ( _characteristicSnd != null && _characteristicSnd.CanWrite ) {
                byte[] cmd = {(byte)'R', (byte)'S', (byte)'T'};
                await _characteristicSnd.WriteAsync(cmd);
            } else {
                rotationSliderX.Value = 0;
                rotationSliderY.Value = 0;
                rotationSliderZ.Value = 0;
            }
        }
    }
}
