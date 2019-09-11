//*********************************************************************************************************************
//
// File Name: ServerConnection.cs
//
// Description:
//    This class handles the connection to the server. It has the ability to connect to the server, disconnect from
//    the server, send messages to the server, and receive messages from the server.
//
// Revision History
//---------------------------------------------------------------------------------------------------------------------
// Matthew Yorke             | 07/31/2019 | This is the initial setup for the server with some functionality missing
//                           |            | or not complete. It has the basic ability to connect and disconnect as well
//                           |            | as send and receive messages.
// Matthew Yorke             | 08/02/2019 | Allow user the enter the host address and port number. Fixed bug where a
//                           |            | thread blocks on read due to no data coming in, preventing users from
//                           |            | logging out. Spelling Fixes.
//
//*********************************************************************************************************************

using System;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace ChatClient
{
   class ServerConnection
   {
      // The socket of the client, used to connect to the server.
      TcpClient mClientSocket;

      //***************************************************************************************************************
      //
      // Method: ServerConnection
      //
      // Description:
      //    Constructor this class. The member variables are set to default values.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public ServerConnection()
      {
         mClientSocket = null;
      }

      //***************************************************************************************************************
      //
      // Method: OpenConnection
      //
      // Description:
      //    Attempts to open a connection to the server. Any failures will result in a failure (false) return. No
      //    problems with connecting to the server will result in a successful (true) return.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public bool OpenConnection(String theServerAddress, int thePortNumber)
      {
         mClientSocket = new System.Net.Sockets.TcpClient();
         try
         {
            mClientSocket.Connect(theServerAddress, thePortNumber);
         }
         catch (SocketException e)
         {
            return false;
         }

         return true;
      }

      //***************************************************************************************************************
      //
      // Method: CloseConnection
      //
      // Description:
      //    Closes the connection to the server.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public void CloseConnection()
      {
         mClientSocket.Close();
      }

      //***************************************************************************************************************
      //
      // Method: SendMessageType
      //
      // Description:
      //    Send message type to server so it knows what kind of message it is receiving.
      //
      // Arguments:
      //    theMessageType
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public void SendMessageType(String theMessageType)
      {
         // The packet of correct size to be sent to the connected server.
         byte[] messagePacket = new byte[8192];
         // Stream to write to the connected server.
         NetworkStream serverStream = mClientSocket.GetStream();

         byte[] messageTypeInBytes = Encoding.ASCII.GetBytes(theMessageType);
         for (int i = 0; i < messageTypeInBytes.Length; ++i)
         {
            messagePacket[i] = messageTypeInBytes[i];
         }
         serverStream.Write(messagePacket, 0, 8192);
         serverStream.Flush();
      }

      //***************************************************************************************************************
      //
      // Method: WriteMessage
      //
      // Description:
      //    Creates a series of packets to be sent to the server that containing the byte representation of the
      //    message.
      //
      // Arguments:
      //    message - The message the user has sent to be sent to the server.
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public void WriteMessage(String message)
      {
         // The packet of correct size to be sent to the connected server.
         byte[] messagePacket = new byte[8192];
         // Stream to write to the connected server.
         NetworkStream serverStream = mClientSocket.GetStream();

         // Retrieve the entire message in a byte array..
         byte[] messageInBytes = Encoding.ASCII.GetBytes(message);

         // The current number of bytes remaining in the message to be sent through packets.
         int remainingBytesToSend = messageInBytes.Length;
         // Counts the packet number being sent.
         int packetCount = 1;
         int loopRemaingingBytesToSend = remainingBytesToSend;
         while (remainingBytesToSend > 0)
         {
            // Zero out the message packet.
            Array.Clear(messagePacket, 0, 8192);

            // Loop only within the packet range. If the message is bigger than the packet range, then separate packets
            // must be made up to the maximum range value.
            for (int i = 0; i < (remainingBytesToSend < 8192 ? loopRemaingingBytesToSend : 8192); ++i)
            {
               // Make sure to use packetCount to continue from the last position of the message since the last packet was made.
               messagePacket[i] = messageInBytes[i * packetCount];
               remainingBytesToSend--;
            }
            loopRemaingingBytesToSend = remainingBytesToSend;

            // Send the packet to the connected server and then flush the stream.
            serverStream.Write(messagePacket, 0, 8192);
            serverStream.Flush();

            // Increment to the next packet number to be made.
            packetCount++;
         }
      }

      //***************************************************************************************************************
      //
      // Method: ReceiveMessage
      //
      // Description:
      //    Waits for the server to send any message to the client and converts the byte representation into a string.
      //
      // Arguments:
      //    N/A
      //
      // Return:
      //    N/A
      //
      //***************************************************************************************************************
      public String ReceiveMessage()
      {
         // Used for the timeout
         Stopwatch timeout = new Stopwatch();
         // Used to determine if the server message has been received.
         bool haveReceivedMessage = false;
         // The message string received from the server.
         String receivedMessage = "";
         // The byte array of the message received from the server.
         byte[] resp = new byte[8192];
         // Used to check the socket stream for messages.
         NetworkStream serverStream = mClientSocket.GetStream();

         timeout.Start();
         // May take a moment for data to come in but also have a timeout timer for waiting.
         while (haveReceivedMessage == false && timeout.ElapsedMilliseconds < 1000)
         {
            // Make sure there is data available so the read function doesn't become blocked forever.
            if (serverStream.DataAvailable == true)
            {
               // Retrieve a message from the server as a representation in bytes as well as the number of bytes that were
               //  read
               int bytesRead = serverStream.Read(resp, 0, resp.Length);

               // Convert the byte representation of the message into a string and remove any remaining null terminators.
               receivedMessage = Encoding.ASCII.GetString(resp, 0, bytesRead).Trim('\0');

               // Mark the message has now been received.
               haveReceivedMessage = true;
            }
         }
         timeout.Stop();

         return receivedMessage;
      }
   }
}
