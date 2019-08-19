using System.ComponentModel;

namespace ChatClient
{
   class MainWindowModel : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      private void NotifyPropertyChanged(string propertyName)
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      // The member variable to hold the updated server address typed by the user.
      private string _mServerAddress;
      public string mServerAddress
      {
         get { return _mServerAddress; }
         set { if (_mServerAddress != value){ _mServerAddress = value; NotifyPropertyChanged("mServerAddress"); } }
      }
      // The member variable to hold the updated port number typed by the user.
      private string _mPortNumber;
      public string mPortNumber
      {
         get { return _mPortNumber; }
         set { if (_mPortNumber != value){ _mPortNumber = value; NotifyPropertyChanged("mPortNumber"); } }
      }
      // The member variable to hold the updated username typed by the user.
      private string _mUsername;
      public string mUsername
      {
         get { return _mUsername; }
         set { if (_mUsername != value) { _mUsername = value; NotifyPropertyChanged("mUsername"); } }
      }
      // The member variable to hold the updated username typed by the user.
      public string _mPassword;
      public string mPassword
      {
         get { return _mPassword; }
         set { if (_mPassword != value) { _mPassword = value; NotifyPropertyChanged("mPassword"); } }
      }
      // The member variable to hold the updated user message typed by the user.
      public string _mUserMessage;
      public string mUserMessage
      {
         get { return _mUserMessage; }
         set { if (_mUserMessage != value) { _mUserMessage = value; NotifyPropertyChanged("mUserMessage"); } }
      }
   }
}
