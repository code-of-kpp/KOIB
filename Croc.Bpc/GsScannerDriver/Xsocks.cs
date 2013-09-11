using System; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    public class ScannerSocket 
    { 
        private Socket s; 
        public ScannerSocket( Socket s ) 
        { 
            this.s = s; 
            SetBuffers( s ); 
        } 
        private void WaitForData( int milliSecondWait ) 
        { 
            if ( !s.Poll( milliSecondWait * 1000, SelectMode.SelectRead ) ) 
            { 
                throw new Exception( "Таймаут ожидания данных истек!" ); 
            } 
        } 
        private bool IsDataForRead( int milliSecondWait ) 
        { 
            return s.Poll( milliSecondWait * 1000, SelectMode.SelectRead ); 
        } 
        public static void SetBuffers( Socket s ) 
        { 
            s.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1024*128 ); 
            s.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024*128 ); 
            s.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1 ); 
        } 
        private byte[] Receive( int milliSecondWait, int size ) 
        { 
            byte[] buffer = new byte[ size ]; 
            int index = 0; 
            int received; 
            while ( size > 0 && IsDataForRead( milliSecondWait ) ) 
            { 
                received = s.Receive( buffer, index, size, SocketFlags.None ); 
                index += received; 
                size -= received; 
            } 
            return buffer; 
        } 
        public UserMessage GetCommand( int milliSecondWait ) 
        { 
            comlen cl = new comlen( Receive( milliSecondWait, 8 ) ); 
            UserMessage um = new UserMessage( (Command)cl.CommandID, Receive( milliSecondWait, cl.datalen ) ); 
            return um; 
        } 
        public void SendCommand( Command command, byte[] data ) 
        { 
            comlen cl; 
            cl.CommandID = (int)command; 
            if ( data != null ) 
            { 
                cl.datalen = data.Length; 
            } 
            else 
            { 
                cl.datalen = 0; 
            } 
            s.Send(new List<ArraySegment<byte>>() 
                           { 
                               new ArraySegment<byte>(cl.data()), 
                               new ArraySegment<byte>(data ?? new byte[0]) 
                           }); 
        } 
        public void SendCommand( Command command, short commandData ) 
        { 
            byte[] data = new byte[2]; 
            data[0] = (byte) (commandData & 0xFF); 
            data[0] = (byte) (commandData >> 8 ); 
            SendCommand( command, data ); 
        } 
        public void SendCommand( UserMessage message ) 
        { 
            SendCommand( message.Command, message.Data ); 
        } 
        public void SendCommand( Command command ) 
        { 
            SendCommand( command, null ); 
        } 
        public void Close() 
        { 
            s.Close(); 
        } 
    } 
}
