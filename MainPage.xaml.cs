using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace selfieig
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private const int maxFrames = 10;
        private int frame = 0;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("Nabigated to");
            base.OnNavigatedTo(e);
            await StartCaptureAsync();
        }
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            await StartCaptureAsync(); 
        }

        private async Task StartCaptureAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Error Initializing camera: {ex.Message}");
            }

            try
            {
                Preview.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error starting capture: {ex.Message}");
            }
        }
        private async Task StopCaptureAsync()
        {
            if(mediaCapture != null)
            {
                await mediaCapture?.StartPreviewAsync();
            }
        }
        private async void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            LowLagPhotoCapture lowLagPhotoCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            CapturedPhoto capturedPhoto = await lowLagPhotoCapture.CaptureAsync();
            SoftwareBitmap softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;

            await lowLagPhotoCapture.FinishAsync();

            if(softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || 
                softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            SoftwareBitmapSource softwareBitmapSource = new SoftwareBitmapSource();
            await softwareBitmapSource.SetBitmapAsync(softwareBitmap);

            ((Image)(Pictures.Children[frame++ % maxFrames])).Source = softwareBitmapSource;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           
            for(int i = 0; i < maxFrames; i++)
            {
                Image image = new Image();
                image.Width = 100;
                image.Height = 100;
                image.PointerEntered += Image_PointerEntered;
                image.PointerExited += Image_PointerExited;

                Pictures.Children.Add(image);
            }
        }

        private void Image_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Playback.Source = null;
        }

        private void Image_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            if(image != null && image.Source != null)
            {
                Playback.Source = image.Source;
            }
           
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
