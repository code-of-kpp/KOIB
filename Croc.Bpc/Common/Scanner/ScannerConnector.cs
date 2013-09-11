using System; 
using System.Collections.Specialized; 
using System.Net; 
using System.Net.Sockets; 
using System.Runtime.Remoting; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Scanner 
{ 
    public abstract class ScannerConnector 
    { 
        protected ILogger _logger; 
        protected IScannerConnectorEventHandler _connectorEventsHandler; 
        protected IScanner _scanner; 
        public ScannerConnector() 
        { 
            _logger = CoreApplication.Instance.GetSubsystemOrThrow<IScannerManager>().Logger; 
        } 
        #region IScannerConnector Members 
        public void Init(string broadcastIpAddress, NameValueCollection settings) 
        { 
            _broadcastIpAddress = IPAddress.Parse(broadcastIpAddress); 
            InitInternal(settings); 
        } 
        public virtual void Connect(IScannerConnectorEventHandler connectorEventsHandler) 
        { 
            CodeContract.Requires(connectorEventsHandler != null); 
            _connectorEventsHandler = connectorEventsHandler; 
            _scanner = ConnectToScanner(); 
            _connectorEventsHandler.WantToConnect(_scanner.SerialNumber, _scanner.IPAddress, _scanner.Status); 
            StartBroadcasting(); 
        } 
        public IScanner GetConnectedScanner(string scannerSerialNumber) 
        { 
            if (scannerSerialNumber != null && scannerSerialNumber != _scanner.SerialNumber) 
                throw new Exception("Несовпадение серийных номеров"); 
            return _scanner; 
        } 
        #endregion 
        #region Виртуальные методы 
        protected virtual void InitInternal(NameValueCollection settings) 
        { 
        } 
        protected virtual IScanner ConnectToScanner() 
        { 
            return null; 
        } 
        #endregion 
        #region Рассылки уведомлений о присутствие сканера 
        private const int UDP_BROADCAST_PORT = 20044; 
        private const int READ_TIMEOUT = 1000000; 
        private const int SEND_TIMEOUT = 3000; 
        protected IPAddress _broadcastIpAddress; 
        private Thread _broadcastThread; 
        private readonly ManualResetEvent _stopBroadcastEvent = new ManualResetEvent(false); 
        private void StartBroadcasting() 
        { 
            _stopBroadcastEvent.Reset(); 
            _broadcastThread = ThreadUtils.StartBackgroundThread(Broadcast); 
        } 
        private void StopBroadcasting() 
        { 
            if (_broadcastThread == null)  
                return; 
            _stopBroadcastEvent.Set(); 
            if (!_broadcastThread.Join(1000)) 
                _broadcastThread.SafeAbort(); 
            _broadcastThread = null; 
        } 
        public void RestartBroadcasting() 
        { 
            StopBroadcasting(); 
            StartBroadcasting(); 
        } 
        private void Broadcast() 
        { 
            Socket sendUdpSocket = null; 
            Socket receiveUdpSocket = null; 
            try 
            { 
                var noticeForSend = new ScannerBroadcastNotice(_scanner.SerialNumber, _scanner.Status); 
                sendUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified); 
                sendUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1); 
                var sendEndPoint = new IPEndPoint(_broadcastIpAddress, UDP_BROADCAST_PORT); 
                receiveUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified) 
                                       { 
                                           Blocking = false // режим неблокированной работы 
                                       }; 
                EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, UDP_BROADCAST_PORT); 
                receiveUdpSocket.Bind(receiveEndPoint); 
                try 
                { 
                    sendUdpSocket.SendTo(noticeForSend.Data, sendEndPoint); 
                    _logger.LogVerbose(Message.ScannerBrodcastMessageSended); 
                } 
                catch (Exception ex) 
                { 
                    if (ex is RemotingException || ex is SocketException) 
                        _logger.LogInfo(Message.ScannerManagerSendingFirstMessageError, ex.Message); 
                    else 
                        _logger.LogError(Message.ScannerManagerSendingFirstMessageError, ex); 
                } 
                while (true) 
                { 
                    try 
                    { 
                        while (receiveUdpSocket.Poll(READ_TIMEOUT, SelectMode.SelectRead)) 
                        { 
                            var data = new byte[ScannerBroadcastNotice.DATA_LENGTH]; 
                            receiveUdpSocket.ReceiveFrom(data, ref receiveEndPoint); 
                            if (_connectorEventsHandler == null) 
                                break; 
                            var remoteAdress = ((IPEndPoint) receiveEndPoint).Address.ToString(); 
                            if (string.CompareOrdinal(_scanner.IPAddress, remoteAdress) == 0) 
                                continue; 
                            var receivedData = new ScannerBroadcastNotice(data); 
                            _connectorEventsHandler.Connected( 
                                receivedData.SerialNumber, 
                                remoteAdress); 
                        } 
                    } 
                    catch (ThreadAbortException) 
                    { 
                        throw; 
                    } 
                    catch (Exception ex) 
                    { 
                        if (ex is RemotingException || ex is SocketException) 
                            _logger.LogInfo(Message.ScannerManagerPollMessageError, ex.Message); 
                        else 
                            _logger.LogError(Message.ScannerManagerPollMessageError, ex); 
                    } 
                    if (_stopBroadcastEvent.WaitOne(SEND_TIMEOUT)) 
                        return; 
                    try 
                    { 
                        sendUdpSocket.SendTo(noticeForSend.Data, sendEndPoint); 
                        _logger.LogVerbose(Message.ScannerBrodcastMessageSended); 
                    } 
                    catch (Exception ex) 
                    { 
                        if (ex is RemotingException || ex is SocketException) 
                            _logger.LogInfo(Message.ScannerManagerSendingMessageError, ex.Message); 
                        else 
                            _logger.LogError(Message.ScannerManagerSendingMessageError, ex, ex.Message); 
                    } 
                } 
            } 
            catch (ThreadAbortException) 
            { 
                _logger.LogVerbose(Message.ScannerBrodcastMessagingStopped); 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(Message.ScannerManagerBroadcastError, ex); 
            } 
            finally 
            { 
                if (receiveUdpSocket != null) 
                    receiveUdpSocket.Close(); 
                if (sendUdpSocket != null) 
                    sendUdpSocket.Close(); 
            } 
        } 
        #endregion 
        #region IDisposable Members 
        public void Dispose() 
        { 
            StopBroadcasting(); 
            if (_scanner != null) 
            { 
                _scanner.SetIndicator("Выключение..."); 
                _scanner.Dispose(); 
                _scanner = null; 
            } 
            GC.SuppressFinalize(this); 
        } 
        #endregion 
    } 
}
