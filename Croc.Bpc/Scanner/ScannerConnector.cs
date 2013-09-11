using System; 

using System.Net; 

using System.Threading; 

using System.Collections.Specialized; 

using System.Net.Sockets; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core; 

using Croc.Core.Diagnostics; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Базовый подключатель сканера 

    /// </summary> 

    public abstract class ScannerConnector 

    { 

        /// <summary> 

        /// Логгер 

        /// </summary> 

        protected ILogger _logger; 

        /// <summary> 

        /// Адрес для широковещательной рассылки 

        /// </summary> 

        protected IPAddress _broadcastIPAddress; 

        /// <summary> 

        /// Обработчик событий о подключении 

        /// </summary> 

        protected IScannerConnectorEventHandler _connectorEventsHandler; 

        /// <summary> 

        /// Сканер 

        /// </summary> 

        protected IScanner _scanner; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public ScannerConnector() 

        { 

            // получим логгер подсистемы управления сканером 

            _logger = CoreApplication.Instance.GetSubsystemOrThrow<IScannerManager>().Logger; 

        } 

 

 

        #region IScannerConnector Members 

 

 

        /// <summary> 

        /// Инициализация 


        /// </summary> 

        /// <param name="broadcastIPAddress"></param> 

        /// <param name="settings"></param> 

        public void Init(string broadcastIPAddress, NameValueCollection settings) 

        { 

            _broadcastIPAddress = IPAddress.Parse(broadcastIPAddress); 

            InitInternal(settings); 

        } 

 

 

        /// <summary> 

        /// Подключиться к сканеру 

        /// </summary> 

        /// <param name="connectorEventsHandler"></param> 

        public virtual void Connect(IScannerConnectorEventHandler connectorEventsHandler) 

        { 

            CodeContract.Requires(connectorEventsHandler != null); 

 

 

            _connectorEventsHandler = connectorEventsHandler; 

 

 

            // подключается к сканеру 

            _scanner = ConnectToScanner(); 

 

 

            // оповестим принимающего события, о том что как бы произошло подключение к локальному сканеру 

            _connectorEventsHandler.WantToConnect(_scanner.SerialNumber, _scanner.IPAddress, _scanner.Status); 

 

 

            // запускаем поток широковещательной рассылки уведомлений о присутствие сканера 

            _stopBroadcastEvent.Reset(); 

            _broadcastThread = new Thread(Broadcast); 

            _broadcastThread.Start(); 

        } 

 

 

        public IScanner GetConnectedScanner(string scannerSerialNumber) 

        { 

            if (scannerSerialNumber != null && scannerSerialNumber != _scanner.SerialNumber) 

                throw new Exception("Несовпадение серийных номеров"); 

 

 

            return _scanner; 

        } 

 

 

        #endregion 

 

 


        #region Виртуальные методы 

 

 

        /// <summary> 

        /// Внутренняя инициализация 

        /// </summary> 

        /// <param name="settings"></param> 

        protected virtual void InitInternal(NameValueCollection settings) 

        { 

        } 

 

 

        /// <summary> 

        /// Подключиться к сканеру 

        /// </summary> 

        /// <returns></returns> 

        protected virtual IScanner ConnectToScanner() 

        { 

            return null; 

        } 

 

 

        #endregion 

 

 

        #region Рассылки уведомлений о присутствие сканера 

 

 

        /// <summary> 

        /// Порт сокета для широковещательной рассылки уведомлений 

        /// </summary> 

        private const int UDP_BROADCAST_PORT = 20044; 

        /// <summary> 

        /// Задержка между рассылкой уведомлений (6 сек) 

        /// </summary> 

        private const int TIMEOUT = 6000; 

        /// <summary> 

        /// Поток широковещательной рассылки уведомлений о присутствие сканера 

        /// </summary> 

        private Thread _broadcastThread; 

        /// <summary> 

        /// Событие остановки потока широковещательной рассылки уведомлений 

        /// </summary> 

        private ManualResetEvent _stopBroadcastEvent = new ManualResetEvent(false); 

 

 

        /// <summary> 

        /// Рассылка широковещательных уведомлений о присутствие данного сканера  

        /// в сети и прием таких сообщений от других сканеров 

        /// </summary> 


        private void Broadcast() 

        { 

            Socket sendUdpSocket = null; 

            Socket receiveUdpSocket = null; 

 

 

            try 

            { 

                // уведомление для рассылки 

                var noticeForSend = new ScannerBroadcastNotice(_scanner.SerialNumber, _scanner.Status); 

 

 

                // сокет для рассылки уведомления 

                sendUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified); 

                sendUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1); 

                var sendEndPoint = new IPEndPoint(_broadcastIPAddress, UDP_BROADCAST_PORT); 

 

 

                // сокет для приема уведомлений от других сканеров 

                receiveUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified); 

                EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, UDP_BROADCAST_PORT); 

                receiveUdpSocket.Bind(receiveEndPoint); 

 

 

                // отдельно отсылаем первое уведомление, чтобы при ошибке сделать запись в лог 

                try 

                { 

                    sendUdpSocket.SendTo(noticeForSend.Data, sendEndPoint); 

                   _logger.LogVerbose(Message.DebugVerbose, "Отправлено сообщение о присутствии сканера в сети"); 

                } 

                catch (Exception ex) 

                { 

                    _logger.LogException(Message.ScannerManagerException, ex, "Ошибка при отправке сообщения"); 

                } 

 

 

                while (true) 

                { 

                    try 

                    { 

                        // ждем уведомлений от других сканеров 

                        while (receiveUdpSocket.Poll(TIMEOUT, SelectMode.SelectRead)) 

                        { 

                            // получено уведомление 

                            var data = new byte[ScannerBroadcastNotice.DATA_LENGTH]; 

                            var i = receiveUdpSocket.ReceiveFrom(data, ref receiveEndPoint); 

 

 

                            if (_connectorEventsHandler != null) 

                            { 


                                var remoteAdress = ((IPEndPoint)receiveEndPoint).Address.ToString(); 

 

 

                                // если оповещение пришло не от самого себя 

                                if (!_scanner.IPAddress.Equals(remoteAdress)) 

                                { 

                                    var receivedData = new ScannerBroadcastNotice(data); 

 

 

                                    // сообщим о присутствии еще одного сканера в сети 

                                    _connectorEventsHandler.Connected( 

                                        receivedData.SerialNumber, 

                                        remoteAdress, 

                                        receivedData.Status); 

                                } 

                            } 

                        } 

                    } 

                    catch { } 

 

 

                    // Не перегружаем процессор 

                    if (_stopBroadcastEvent.WaitOne(TIMEOUT)) 

                        return; 

 

 

                    // отсылаем следующее уведомление 

                    try 

                    { 

                        sendUdpSocket.SendTo(noticeForSend.Data, sendEndPoint); 

                    } 

                    catch { } 

                } 

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

            if (_broadcastThread != null) 

            { 

                _stopBroadcastEvent.Set(); 

                if (!_broadcastThread.Join(1000)) 

                    _broadcastThread.Abort(); 

                _broadcastThread = null; 

            } 

 

 

            if (_scanner != null) 

            { 

                _scanner.SetIndicator(""); 

                _scanner.Dispose(); 

                _scanner = null; 

            } 

 

 

            GC.SuppressFinalize(this); 

        } 

 

 

        #endregion 

    } 

}


