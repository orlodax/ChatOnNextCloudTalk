using ChatOnNextCloudTalk.Classes;
using ChatOnNextCloudTalk.Views;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Input;

namespace ChatOnNextCloudTalk.ViewModels
{
    public class LoginFormVM
    {
        #region PROPERTIES

        public string URL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ICommand LoginCommand { get; private set; }

        #endregion

        #region CONSTRUCTOR

        public LoginFormVM()
        {
            LoginCommand = new RelayCommand(Login);
        }

        #endregion

        #region COMMANDS

        void Login(object parameter)
        {
            var loginForm = (LoginForm)parameter;
           
            //mvvm and security violation
            var secureString = loginForm.Password;
            Password = ConvertToUnsecureString(secureString);


            ChatAgent chatAgent = new ChatAgent(Username, Password, URL);

            ChatView chatView = new ChatView() { DataContext = new ChatVM(chatAgent) };

            chatView.Show();

            loginForm.Hide();
        }

        #endregion

        #region OTHER

        private string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
            {
                return string.Empty;
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        #endregion
    }
}
