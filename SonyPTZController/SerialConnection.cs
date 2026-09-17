using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SonyPTZController
{
    public class SerialConnection
    {
        private SerialPort? _serialPort;

        // VISCA messages end with FF.
        private readonly List<byte> _receiveBuffer = new();

        public bool IsConnected =>
            _serialPort != null && _serialPort.IsOpen;

        public event EventHandler<string>? DataReceived;
        public event EventHandler<string>? DataSent;
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

                // Sony VISCA expects DTR high.
                DtrEnable = true,

                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                _serialPort.Open();

                _receiveBuffer.Clear();

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

                _receiveBuffer.Clear();

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

            string hex = BitConverter
                .ToString(data)
                .Replace("-", " ");

            DataSent?.Invoke(
                this,
                $"TX  {hex}");
        }

        private void SerialPort_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            int bytesToRead = _serialPort.BytesToRead;

            if (bytesToRead <= 0)
                return;

            byte[] buffer = new byte[bytesToRead];

            int bytesRead = _serialPort.Read(
                buffer,
                0,
                buffer.Length);

            for (int i = 0; i < bytesRead; i++)
            {
                _receiveBuffer.Add(buffer[i]);

                // VISCA packets terminate with FF.
                if (buffer[i] == 0xFF)
                {
                    ProcessPacket();
                }
            }
        }

        private void ProcessPacket()
        {
            if (_receiveBuffer.Count == 0)
                return;

            byte[] packet = _receiveBuffer.ToArray();

            _receiveBuffer.Clear();

            string hex = BitConverter
                .ToString(packet)
                .Replace("-", " ");

            DataReceived?.Invoke(
                this,
                $"RX  {hex}");

            InterpretViscaResponse(packet);
        }

        private void InterpretViscaResponse(byte[] packet)
        {
            if (packet.Length < 3)
                return;

            // VISCA response packets normally begin with 90.
            if (packet[0] != 0x90)
                return;

            // ACK:
            // 90 4y FF
            if (packet[1] >= 0x40 && packet[1] <= 0x4F)
            {
                int socket = packet[1] & 0x0F;

                StatusChanged?.Invoke(
                    this,
                    $"VISCA: Command acknowledged (socket {socket}).");

                return;
            }

            // Inquiry response.
            // For example:
            // 90 50 02 FF
            //
            // Inquiry responses have additional data bytes.
            if (packet[1] >= 0x50 &&
                packet[1] <= 0x5F &&
                packet.Length > 3)
            {
                StatusChanged?.Invoke(
                    this,
                    "VISCA: Inquiry response received.");

                return;
            }

            // Completion:
            // 90 5y FF
            if (packet[1] >= 0x50 &&
                packet[1] <= 0x5F)
            {
                int socket = packet[1] & 0x0F;

                StatusChanged?.Invoke(
                    this,
                    $"VISCA: Command completed (socket {socket}).");

                return;
            }

            // Error:
            // 90 60 xx FF
            // 90 61 xx FF
            if (packet[1] == 0x60 || packet[1] == 0x61)
            {
                if (packet.Length >= 4)
                {
                    string description =
                        GetViscaErrorDescription(packet[2]);

                    StatusChanged?.Invoke(
                        this,
                        $"VISCA ERROR: {description} " +
                        $"(code {packet[2]:X2}).");
                }

                return;
            }

            // Inquiry response:
            // 90 50 ... FF
            if (packet[1] >= 0x50 && packet[1] <= 0x5F)
            {
                StatusChanged?.Invoke(
                    this,
                    "VISCA: Inquiry response received.");

                return;
            }
        }

        private string GetViscaErrorDescription(byte errorCode)
        {
            return errorCode switch
            {
                0x01 => "Message length error",
                0x02 => "Syntax error",
                0x03 => "Command buffer full",
                0x04 => "Command cancelled",
                0x05 => "No socket",
                0x41 => "Command not executable",
                _ => "Unknown error"
            };
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}