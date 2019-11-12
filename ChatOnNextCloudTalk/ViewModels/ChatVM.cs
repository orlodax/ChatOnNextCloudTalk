using ChatOnNextCloudTalk.Classes;
using NextCloudAPI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatOnNextCloudTalk.ViewModels
{
    public class ChatVM : ObservableObject
    {
        #region FIELDS and PROPERTIES
        ChatAgent ChatAgent;
        ObservableCollection<Room> contatti = new ObservableCollection<Room>();
        ObservableCollection<Chat> messaggi = new ObservableCollection<Chat>();
        ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();
        Room selectedRoom;
        System.Drawing.Color NewBlue;
        System.Drawing.Color GrigioClear;
        private string messageText;
        private string searchText;
        private Visibility contactsPanelVIS = Visibility.Collapsed;
        private Visibility roomsPanelVIS = Visibility.Visible;
        private Contact selectedContact;
        private UserInfo userInfo;
        uint? RoomToSetAsRead = null;

        public ObservableCollection<Room> Rooms { get => contatti; set => SetField(ref contatti, value); }
        public ObservableCollection<Chat> Chats { get => messaggi; set => SetField(ref messaggi, value); }
        public ObservableCollection<Contact> Contacts { get => contacts; set => SetField(ref contacts, value); }
        public Room SelectedRoom { get => selectedRoom; set { SetField(ref selectedRoom, value); SelectedRoomChanged(); } }

        public string MessageText { get => messageText; set => SetField(ref messageText, value); }
        public string SearchText { get => searchText; set { SetField(ref searchText, value); SearchContacts(); } }

        public Visibility ContactsPanelVIS
        {
            get => contactsPanelVIS;
            set
            {
                SetField(ref contactsPanelVIS, value);
                if (value == Visibility.Visible)
                    RoomsPanelVIS = Visibility.Collapsed;
                else
                    RoomsPanelVIS = Visibility.Visible;
            }
        }
        public Visibility RoomsPanelVIS { get => roomsPanelVIS; set => SetField(ref roomsPanelVIS, value); }
        public Contact SelectedContact { get => selectedContact; set { SetField(ref selectedContact, value); if (value != null) LoadOrCreateRoom(); } }
        public UserInfo UserInfo { get => userInfo; set => SetField(ref userInfo, value); }

        public ICommand SendMessageCMD { get; private set; }
        public ICommand HideContactsCMD { get; private set; }
        public ICommand ShowMyDetailsCMD { get; private set; }

        #endregion

        #region CONSTRUCTOR

        public ChatVM(ChatAgent chatAgent)
        {
            ChatAgent = chatAgent;
            UserInfo = ChatAgent.userInfo;

            ChatAgent.RoomsRefresh += ChatAgent_RoomsRefresh;
            ChatAgent.ChatsRefresh += ChatAgent_ChatsRefresh;
            ChatAgent.ContactsRefresh += ChatAgent_ContactsRefresh;
            ChatAgent.UserInfoReady += ChatAgent_UserInfoReady;

            SolidColorBrush nb = (SolidColorBrush)Application.Current.Resources["NewBlue"];
            SolidColorBrush gc = (SolidColorBrush)Application.Current.Resources["GrigioClear"];
            NewBlue = System.Drawing.Color.FromArgb(nb.Color.A, nb.Color.R, nb.Color.G, nb.Color.B);
            GrigioClear = System.Drawing.Color.FromArgb(gc.Color.A, gc.Color.R, gc.Color.G, gc.Color.B);

            SendMessageCMD = new RelayCommand(SendMessage);
            HideContactsCMD = new RelayCommand(HideContacts);
        }
        #endregion

        #region CHATAGENTS EVENTS TO REFRESH UI ELEMENTS
        private void ChatAgent_UserInfoReady(object sender, EventArgs e)
        {
            UserInfo = ChatAgent.userInfo;
        }
        private void ChatAgent_RoomsRefresh(object sender, EventArgs e)
        {
            //Save selection before recreating list
            uint roomId = 0;
            if (SelectedRoom != null)
                roomId = SelectedRoom.id;

            //Carica le conversazioni, ordinandole per data di ultima attività (quindi i nuovi messaggi sono sempre in cima)
            ObservableCollection<Room> NewRooms = new ObservableCollection<Room>((ChatAgent.Rooms).OrderByDescending(r => r.lastActivity));

            if (!NewRooms.Equals(Rooms))        //THIS CHECK DOES NOT WORK. (always true)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Rooms.Clear();
                    foreach (Room r in NewRooms)
                        Rooms.Add(r);
                });

            if (roomId != 0)
                SelectedRoom = Rooms.First(r => r.id == roomId);

            if (RoomToSetAsRead != null)
            {
                Rooms.First(r => r.id == RoomToSetAsRead).HasUnreadMessages = false;    //un-news new messages sent from self
                RoomToSetAsRead = null;
            }
        }
        private void ChatAgent_ChatsRefresh(object sender, EventArgs e)
        {
            SelectedRoom = ChatAgent.SelectedRoom;
            //Carica i messaggi della conversazione selezionata in ordine inevrso (gli ultimi in basso)
            ObservableCollection<Chat> NewChats = new ObservableCollection<Chat>((ChatAgent.Chats).OrderBy(c => c.timestamp));

            if (!NewChats.Equals(Chats))        //THIS CHECK DOES NOT WORK. (always true)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats.Clear();
                    foreach (Chat c in NewChats)
                    {
                        if (c.actorId.Equals(UserInfo.displayname))
                        {
                            c.ColoreSfondo = NewBlue;
                            c.MessageAlignment = "right";
                        }
                        else
                        {
                            c.ColoreSfondo = GrigioClear;
                            c.MessageAlignment = "left";
                        }

                        Chats.Add(c);
                    }
                });
        }
        private void ChatAgent_ContactsRefresh(object sender, EventArgs e)
        {
            ObservableCollection<Contact> NewContacts = new ObservableCollection<Contact>(ChatAgent.Contacts);

            if (!NewContacts.Equals(Contacts))        //THIS CHECK DOES NOT WORK. (always true)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Contacts.Clear();
                    foreach (Contact c in NewContacts)
                        Contacts.Add(c);
                });
            if (Contacts.Count > 0)
                ContactsPanelVIS = Visibility.Visible;
        }
        #endregion

        #region COMMANDS and SETTERS
        void SelectedRoomChanged()
        {
            ChatAgent.SelectedRoom = SelectedRoom as Room;
        }
        void SearchContacts()
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
                ChatAgent.SearchContacts(SearchText);
        }
        void LoadOrCreateRoom()
        {
            ContactsPanelVIS = Visibility.Collapsed;

            ChatAgent.LoadOrCreateRoom(SelectedContact.id);
        }
        void SendMessage(object parameter)
        {
            if (SelectedRoom != null)
            {
                RoomToSetAsRead = SelectedRoom.id;  //save selection to un-new new messages from self
                Chat NewChat = new Chat()
                {
                    actorDisplayName = UserInfo.displayname,
                    message = MessageText,
                    //timestamp = 0,
                    token = SelectedRoom.token
                };
                ChatAgent.SendChat(SelectedRoom, NewChat);
                MessageText = string.Empty;
            }
        }
        void HideContacts(object parameter)
        {
            ContactsPanelVIS = Visibility.Collapsed;
            ChatAgent_RoomsRefresh(this, new EventArgs());  //altrimenti non so perché a volte si sdoppiano i contatti
        }
        #endregion
    }
}
