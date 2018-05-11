using Localization.Extensions;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Localization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ILanguageService _languageService;
        public MainWindow()
        {
            _languageService = new DefaultLanguageService(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture);
            InitializeComponent();
        }

        private void SwitchLanguageButton_Click(object sender, RoutedEventArgs e)
        {

            // Set new culture.
            if (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en")
                _languageService.UiCulture = new CultureInfo("de");
            else
                _languageService.UiCulture = new CultureInfo("en");

            // Update all bindings.
            StringResExtension.UpdateStrings();
        }
    }
}
