using Xamarin.Forms;

namespace Angle3DMonitor
{
    public partial class Angle3DMonitorPage : ContentPage
    {
        public Angle3DMonitorPage()
        {
            InitializeComponent();

            Title = " Angle 3D Monitor ";
        }

        void ViewTest_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new UrhoPage(null));
        }

        void PeripheralsSearch_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new SearchPage());
        }
    }
}
