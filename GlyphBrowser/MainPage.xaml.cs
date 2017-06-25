using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GlyphBrowser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FontFamily _defaultFontFamily;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _defaultFontFamily = GlyphText.FontFamily;
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                b.IsEnabled = false;

                switch (b.Name)
                {
                    case "ParseBtn":
                        ParseSetText(ParseText.Text);
                        break;
                    case "ClearBtn":
                        ResetGlyphText(_defaultFontFamily);
                        break;
                    case "SaveBtn":
                        await SaveGlyphTextAsImageAsync();
                        break;
                    default:
                        break;
                }

                b.IsEnabled = true;
            }
        }

        private async Task SaveGlyphTextAsImageAsync()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(GlyphText);

            uint width = (uint)renderTargetBitmap.PixelWidth;
            uint height = (uint)renderTargetBitmap.PixelHeight;
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            byte[] bytes = pixels.ToArray();

            var displayInformation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
            float dpiX = displayInformation.LogicalDpi;
            float dpiY = displayInformation.LogicalDpi;

            StorageFolder pictureFolder = KnownFolders.PicturesLibrary;

            StorageFile file = await pictureFolder.CreateFileAsync("Glyph Image.png", CreationCollisionOption.GenerateUniqueName);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, width, height, dpiX, dpiY, bytes);

                await encoder.FlushAsync();
            }
        }

        private void ParseSetText(string text)
        {
            string characters = Helpers.ParseUnicodeString(text);
            AddCharacters(characters);
        }

        private void GlyphText_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void GlyphText_DropAsync(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;

                    await LoadGlyphTextFromFileAsync(storageFile);
                }
            }
        }

        private async Task LoadGlyphTextFromFileAsync(StorageFile file)
        {
            if (file != null)
            {
                string text = await Helpers.LoadUnicodeTextFromFile(file);

                if (String.IsNullOrEmpty(text) == false)
                {
                    string fontFamilyName = Helpers.GetFontFamilyName(file);

                    SetText(text, fontFamilyName);
                }
            }
        }

        private void SetText(string text, string fontFamilyName)
        {
            ResetGlyphText(new FontFamily(fontFamilyName));

            AddCharacters(text);
        }

        private void ResetGlyphText(FontFamily fontFamily)
        {
            GlyphText.Text = String.Empty;
            GlyphText.FontFamily = fontFamily;
        }

        private void AddCharacters(string str)
        {
            GlyphText.Text += str;
            GlyphText.Text += Environment.NewLine;
        }
    }
}
