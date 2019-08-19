//*********************************************************************************************************************
//
// File Name: MainWindow.xaml.cs
//
// Description:
//    The main window for the client chat. This handle the ability to connect to the server and send basic messages as
//    well as use a thread to receive any incoming messages to be displayed in chat.
//
// Revision History
//---------------------------------------------------------------------------------------------------------------------
// Matthew Yorke             | 07/31/2019 | Initial client is made without any databased information. There are still
//                           |            | many implementation features to add, but basic sending and receiving (on
//                           |            | a local host) are available.
// Matthew Yorke             | 08/02/2019 | Allow user the enter the host address and port number. Prevent users from
//                           |            | sending blank messages. Spelling fixes.
//
//*********************************************************************************************************************

using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace ChatClient
{
   public partial class MainWindow
   {
      private readonly MainWindowModel mModel = new MainWindowModel();

      // The server connection object that handles the connection, disconnect, and sending of messages to the server.
      private ServerConnection mServerConnection;
      // The thread that handles receiving of messages from the server.
      private Thread mMessageThread;
      // A boolean to track when the client is and isn't connected to the server.
      private bool mConnected;

      //***************************************************************************************************************
      //
      // Method: MainWindow
      //
      // Description:
      //    Set member variables to default values and then initialize the components of the window.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public MainWindow()
      {
         mServerConnection = null;
         mMessageThread = null;
         mConnected = false;
         InitializeComponent();
         DataContext = mModel;
      }

      //***************************************************************************************************************
      //
      // Method: LoginButtonCallback
      //
      // Description:
      //    Attempts to establish a connection to the server. If the connection is successfully established, then
      //    allow the user to type and send messages. Additionally the ability to logout will be available. On a
      //    failure to connect to the server an error message will be produced.
      //
      // Arguments:
      //    theSender         - The control that sent this callback command. This is unused, but needed for the
      //                        callback method structure.
      //    theEventArguments - Additional details about the events that triggered the callback. This is unused, but
      //                        needed for the callback method structure.
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void LoginButtonCallback(object theSender, RoutedEventArgs theEventArguments)
      {
         if (mModel.mServerAddress == "")
         {
            MessageBox.Show("The hostname has not been entered.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
         }
         else if (mModel.mPortNumber == "")
         {
            MessageBox.Show("The port number has not been entered.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
         }
         else
         {
            mServerConnection = new ServerConnection();

            bool connectionSuccesful = mServerConnection.OpenConnection(mModel.mServerAddress, int.Parse(mModel.mPortNumber));

            if (connectionSuccesful == true)
            {
               mServerConnection.SendMessageType("connection");
               String conenctionParameters = mModel.mUsername + "," + mModel.mPassword;
               mServerConnection.WriteMessage(conenctionParameters);
               String reply = mServerConnection.ReceiveMessage();

               if (reply == "ACK")
               { 
                  mConnected = true;
                  mMessageThread = new Thread(MessageReceiveThread);
                  mMessageThread.Start();

                  this.ServerAddress.IsEnabled = false;
                  this.PortNumber.IsEnabled = false;
                  this.LoginButton.IsEnabled = false;
                  this.LogoutButton.IsEnabled = true;
                  this.UserMessage.IsEnabled = true;
                  this.SendMessageButton.IsEnabled = true;
               }
               else
               {
                  connectionSuccesful = false;
                  mServerConnection = null;
                  MessageBox.Show("Username or password are invalid.", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
               }
            }
            else
            {
               MessageBox.Show("Failed to connect to the server.", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      //***************************************************************************************************************
      //
      // Method: LogoutButtonCallback
      //
      // Description:
      //    Ends the connection to the server, allowing the receive thread to finish before completely disconnecting.
      //    After the disconnect, the only enabled button will be the Login button.
      //
      // Arguments:
      //    theSender         - The control that sent this callback command. This is unused, but needed for the
      //                        callback method structure.
      //    theEventArguments - Additional details about the events that triggered the callback. This is unused, but
      //                        needed for the callback method structure.
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void LogoutButtonCallback(object theSender, RoutedEventArgs theEventArguments)
      {
         mConnected = false;
         mMessageThread.Join();
         mServerConnection.CloseConnection();
         
         this.ServerAddress.IsEnabled = true;
         this.PortNumber.IsEnabled = true;
         this.LoginButton.IsEnabled = true;
         this.LogoutButton.IsEnabled = false;
         this.UserMessage.IsEnabled = false;
         this.SendMessageButton.IsEnabled = false;
      }

      //***************************************************************************************************************
      //
      // Method: SendMessageButtonCallback
      //
      // Description:
      //    Retrieves the message from the User Message text box control and sends it through the socket stream to the
      //    server. Additionally, the User Message text box is set back to being blank for the user to type their next
      //    message.
      //
      // Arguments:
      //    theSender         - The control that sent this callback command. This is unused, but needed for the
      //                        callback method structure.
      //    theEventArguments - Additional details about the events that triggered the callback. This is unused, but
      //                        needed for the callback method structure.
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void SendMessageButtonCallback(object theSender, RoutedEventArgs theEventArguments)
      {
         if(mModel.mUserMessage != "")
         {
            mServerConnection.WriteMessage("message");
            mServerConnection.WriteMessage(mModel.mUserMessage);
            mModel.mUserMessage = "";
         }
      }

      //***************************************************************************************************************
      //
      // Method: UserMessageKeyPressed
      //
      // Description:
      //    Check any key presses for the User Message (text box) component. This method checks to see if the Enter key
      //    has been pressed so to call the function to send the message without having to press the Send button
      //    itself.
      //
      // Arguments:
      //    theSender         - The control that sent this callback command. This is unused, but needed for the
      //                        callback method structure.
      //    theEventArguments - Additional details about the events that triggered the callback. This is used in this
      //                        method to determine which key was pressed down.
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void UserMessageKeyPressed(object theSender, KeyEventArgs theEventArguments)
      {
         if(theEventArguments.Key == Key.Enter)
         {
            SendMessageButtonCallback(null, null);
         }
      }

      //***************************************************************************************************************
      //
      // Method: MessageReceiveThread
      //
      // Description:
      //    While there is still a connection to the server, continuously check for any messages being received from
      //    the server and append it to the clients chatbox.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void MessageReceiveThread()
      {
         while (mConnected == true)
         {
            String receivedMessage = mServerConnection.ReceiveMessage();

            if (String.IsNullOrWhiteSpace(receivedMessage) == false)
            {
               this.Dispatcher.Invoke(() => {AppendMessageToChat(receivedMessage + "\r\n");});
            }
         }
      }

      //***************************************************************************************************************
      //
      // Method: AppendMessageToChat
      //
      // Description:
      //    Method for the message receiving thread to call as to append the new message to the chatbox. This also
      //    makes sure to automatically scroll the chatbox to the bottom (newest messages) as it overflows the normal
      //    chatbox area.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      private void AppendMessageToChat(String theMessage)
      {
         // Append the received message to the chatbox.
         ChatBox.AppendText(theMessage);

         // Get the component that is currently focused.
         UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

         // Set focus to the chatbox and make sure to scroll to the end of the chatbox.
         ChatBox.Focus();
         ChatBox.CaretIndex = ChatBox.Text.Length;
         ChatBox.ScrollToEnd();

         // Reset the focus to the previous focused component.
         elementWithFocus.Focus();
      }
   }
}
