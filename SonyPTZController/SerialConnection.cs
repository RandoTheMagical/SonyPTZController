using System;
using System.IO.Ports;

namespace SonyPTZController
{
    public class SerialConnection
    {
        private SerialPort? _serialPort;

        public bool IsConnected =>
            _serialPort != null && _serialPort.IsOpen;

        public event EventHandler<string>? DataReceived;
        public event EventHandler<string>? StatusChanged;

        public void Connect(string portName, int baudRate)
        {
            if (IsConnected)
                Disconnect();

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,

                // Sony VISCA expects the controller's DTR
                // output to be high.
                DtrEnable = true,

                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                _serialPort.Open();

                StatusChanged?.Invoke(
                    this,
                    $"Connected to {portName} at {baudRate} baud.");
            }
            catch
            {
                _serialPort.Dispose();
                _serialPort = null;
                throw;
            }
        }

        public void Disconnect()
        {
            if (_serialPort == null)
                return;

            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            finally
            {
                _serialPort.Dispose();
                _serialPort = null;

                StatusChanged?.Invoke(
                    this,
                    "Disconnected.");
            }
        }

        public void Send(byte[] data)
        {
            if (!IsConnected || _serialPort == null)
                throw new InvalidOperationException(
                    "Serial port is not connected.");

            _serialPort.Write(data, 0, data.Length);
        }

        private void SerialPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null)
                return;

            int bytesToRead = _serialPort.BytesToRead;

            if (bytesToRead <= 0)
                return;

            byte[] buffer = new byte[bytesToRead];

            _serialPort.Read(
                buffer,
                0,
                buffer.Length);

            string hex = BitConverter
                .ToString(buffer)
                .Replace("-", " ");

            DataReceived?.Invoke(this, hex);
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}