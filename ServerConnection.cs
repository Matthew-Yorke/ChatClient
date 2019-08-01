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
//
//*********************************************************************************************************************

using System;
using System.Text;
using System.Net.Sockets;

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
      public bool OpenConnection()
      {
         mClientSocket = new System.Net.Sockets.TcpClient();
         try
         {
            mClientSocket.Connect("192.168.56.1", 54000);
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
      // Method: RecieveMessage
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
      public String RecieveMessage()
      {
         String recievedMessage = "";
         byte[] resp = new byte[8192];
         NetworkStream serverStream = mClientSocket.GetStream();

         // Retrieve a message from the server as a representation in bytes as well as the number of bytes that were
         //  read
         int bytesRead = serverStream.Read(resp, 0, resp.Length);

         // Convert the byte representation of the message into a string and remove any remaining null terminators.
         recievedMessage = Encoding.ASCII.GetString(resp, 0, bytesRead).Trim('\0');

         return recievedMessage;
      }
   }
}
