using ChatOnNextCloudTalk.ViewModels;
using ChatOnNextCloudTalk.Views;
using System;
using System.Threading;
using System.Windows;

namespace ChatOnNextCloudTalk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetLanguageDictionary();

            LoginForm loginForm = new LoginForm();
            loginForm.DataContext = new LoginFormVM();
            loginForm.Show();
        }

        static private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    dict.Source = new Uri("..\\ResourceDictionaries\\English.xaml", UriKind.Relative);
                    break;
                case "it-IT":
                    dict.Source = new Uri("..\\ResourceDictionaries\\Italian.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\ResourceDictionaries\\English.xaml", UriKind.Relative);
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
