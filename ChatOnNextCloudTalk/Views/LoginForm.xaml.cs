using System.Windows;
using System.Windows.Forms;

namespace ChatOnNextCloudTalk.Views
{
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window
    {
        public LoginForm()
        {
            InitializeComponent();

            int PSBH = Screen.PrimaryScreen.Bounds.Height;
            int TaskBarHeight = PSBH - Screen.PrimaryScreen.WorkingArea.Height;

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - TaskBarHeight;
        }

        //mvvm and security violations
        public System.Security.SecureString Password
        {
            get
            {
                return passwordBox.SecurePassword;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
