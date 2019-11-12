using NextCloudAPI;
using NextCloudAPI.Models;
using NextCloudAPI.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace ChatOnNextCloudTalk.Classes
{
    public class ChatAgent
    {
        #region FIELDS

        RoomBL RoomBL;
        ChatBL ChatBL;
        ContactBL ContactBL;
        internal AvatarBL AvatarBL;
        UserBL UserBL;
        private Room selectedRoom;
        private NotifyIcon notifyIcon;
        #endregion

        #region OBJECTS TO COMMUNICATE WITH CHATVM
        internal User CurrentUser { get; set; } = new User();
        internal List<Room> Rooms { get; set; } = new List<Room>();
        internal List<Chat> Chats { get; set; } = new List<Chat>();
        internal List<Contact> Contacts { get; set; } = new List<Contact>();
        internal Room SelectedRoom
        {
            get => selectedRoom;
            set
            {
                if (value != selectedRoom)
                {
                    selectedRoom = value;
                    if (value != null)
                        ChatsRequest();
                }
            }
        }
        internal UserInfo userInfo { get; set; }
        #endregion

        #region Events definition
        public event EventHandler RoomsRefresh;
        public event EventHandler ChatsRefresh;
        public event EventHandler ContactsRefresh;
        public event EventHandler UserInfoReady;
        void OnRoomsRefresh(EventArgs e)
        {
            RoomsRefresh?.Invoke(this, e);
        }
        void OnChatsRefresh(EventArgs e)
        {
            ChatsRefresh?.Invoke(this, e);
        }
        void OnContactsRefresh(EventArgs e)
        {
            ContactsRefresh?.Invoke(this, e);
        }
        void OnUserInfoReady(EventArgs e)
        {
            UserInfoReady?.Invoke(this, e);
        }
        #endregion

        #region Timers/Workers

        private BackgroundWorker RoomsWorker = new BackgroundWorker();                  //Worker che aggiorna le room
        private System.Timers.Timer RoomTimer10 = new System.Timers.Timer(10000);       //Timer per chiamare /room (10sec)
        private object TimerLock = new object();                                        //Oggetto per la non esecuzione continua del timer

        #endregion

        #region CONSTRUCTOR
        public ChatAgent(string username, string password, string url)
        {
            CurrentUser.userId = username;
            CurrentUser.displayName = username;
            CurrentUser.password = password;

            RequestsBL RequestsBL = new RequestsBL(CurrentUser, url);
            RoomBL = new RoomBL(RequestsBL);
            ChatBL = new ChatBL(RequestsBL);
            ContactBL = new ContactBL(RequestsBL);
            AvatarBL = new AvatarBL(RequestsBL);
            UserBL = new UserBL(RequestsBL);

            GetUserInfo();

            //loads data once at startup before starting worker and timer
            RoomRequest();

            RoomsWorker.DoWork += RoomsWorker_DoWork;
            RoomTimer10.Elapsed += RoomTimer10_Elapsed;
            RoomTimer10.Start();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.ncloudico;
            notifyIcon.Visible = true;

        }
        #endregion

        #region Timer/Worker execution
        private void RoomTimer10_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(TimerLock);
                if (lockTaken)
                    if (!RoomsWorker.IsBusy)
                        RoomsWorker.RunWorkerAsync();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(TimerLock);
            }
        }
        private void RoomsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RoomRequest();
        }
        #endregion

        #region Api calls
        async void GetUserInfo()
        {
            userInfo = await UserBL.GetUserInfo(CurrentUser.userId);
            userInfo.Avatar = await AvatarBL.GetAvatar(userInfo.id, 46);
            OnUserInfoReady(new EventArgs());
        }
        public async void RoomRequest()
        {
            List<Room> NewRooms = await RoomBL.GetRoomsForCurrentUser();

            if (!NewRooms.Equals(Rooms))
            {
                Rooms.Clear();

                foreach (Room r in NewRooms)
                {
                    if (r.name != CurrentUser.userId)
                        r.Avatar = await AvatarBL.GetAvatar(r.name, 46);
                    else
                        r.Avatar = Properties.Resources.ncloudbox;

                    if (r.Avatar == null)
                        r.Avatar = Properties.Resources.ncloudbox;

                    if (r.lastMessage.actorId != userInfo.id)
                        if (r.HasUnreadMessages)
                            notifyIcon.ShowBalloonTip(3000, "Nuovo messagio", r.displayName + ": " + r.lastMessage.message.ToString(), ToolTipIcon.None);

                    Rooms.Add(r);
                }

                OnRoomsRefresh(new EventArgs());
            }
        }

        public async void ChatsRequest()
        {
            if (SelectedRoom != null)
            {
                Chats.Clear();
                Chats = await ChatBL.GetChatsInRoom(SelectedRoom);
                OnChatsRefresh(new EventArgs());
            }
        }
        public async void SendChat(Room Room, Chat Chat)
        {
            await ChatBL.SendChat(Room, Chat);
            RoomRequest();
            ChatsRequest();
        }
        public async void SearchContacts(string textSearch)
        {
            List<Contact> NewContacts = await ContactBL.SearchContacts(textSearch);

            if (!NewContacts.Equals(Contacts))
            {
                Contacts.Clear();

                foreach (Contact c in NewContacts)
                {
                    c.Avatar = await AvatarBL.GetAvatar(c.id, 46);
                    Contacts.Add(c);
                }

                OnContactsRefresh(new EventArgs());
            }
        }
        public async void LoadOrCreateRoom(string contactId)
        {
            RoomOpen(await RoomBL.CreateRoom(Constants.ConversationTypes.OneToOne, contactId, string.Empty));
        }
        public async void RoomOpen(string token)
        {
            Rooms.Clear();
            Rooms = await RoomBL.GetRoomsForCurrentUser();
            SelectedRoom = Rooms.Find(r => r.token == token);
            OnRoomsRefresh(new EventArgs());
        }
        #endregion
    }
}
