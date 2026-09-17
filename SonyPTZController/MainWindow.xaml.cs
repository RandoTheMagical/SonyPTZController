using System;
using System.Windows;
using System.Windows.Media;

namespace SonyPTZController
{
    public partial class MainWindow : Window
    {
        private readonly SerialConnection _serial;
        private readonly ViscaCamera _camera;

        public MainWindow()
        {
            InitializeComponent();

            _serial = new SerialConnection();
            _camera = new ViscaCamera(_serial);

            _serial.DataReceived += Serial_DataReceived;
            _serial.StatusChanged += Serial_StatusChanged;

            SetupBaudRates();
            RefreshPorts();

            SetDisconnectedState();
        }

        private void PanLeft_MouseDown(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  PAN LEFT");

            _camera.PanLeft();

            e.Handled = true;
        }


        private void PanRight_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  PAN RIGHT");

            _camera.PanRight();

            e.Handled = true;
        }


        private void TiltUp_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  TILT UP");

            _camera.TiltUp();

            e.Handled = true;
        }


        private void TiltDown_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  TILT DOWN");

            _camera.TiltDown();

            e.Handled = true;
        }


        private void PanTilt_MouseUp(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  PAN/TILT STOP");

            _camera.PanTiltStop();

            e.Handled = true;
        }

        private void SetupBaudRates()
        {
            BaudComboBox.Items.Add("9600");
            BaudComboBox.Items.Add("38400");

            BaudComboBox.SelectedIndex = 0;
        }


        private void RefreshPorts()
        {
            string currentPort =
                PortComboBox.SelectedItem as string ?? "";

            PortComboBox.Items.Clear();

            string[] ports =
                SerialConnection.GetAvailablePorts();

            foreach (string port in ports)
            {
                PortComboBox.Items.Add(port);
            }

            if (PortComboBox.Items.Contains(currentPort))
            {
                PortComboBox.SelectedItem = currentPort;
            }
            else if (PortComboBox.Items.Count > 0)
            {
                PortComboBox.SelectedIndex = 0;
            }

            Log(
                $"Found {ports.Length} serial port(s).");
        }


        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            RefreshPorts();
        }


        private void ConnectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Disconnect if already connected
            if (_serial.IsConnected)
            {
                _serial.Disconnect();

                SetDisconnectedState();

                return;
            }


            // Make sure a COM port is selected
            if (PortComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a COM port.",
                    "No COM Port",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            if (BaudComboBox.SelectedItem == null)
                return;


            string port =
                PortComboBox.SelectedItem.ToString()!;

            int baud =
                int.Parse(
                    BaudComboBox.SelectedItem.ToString()!);


            try
            {
                _serial.Connect(port, baud);

                ConnectButton.Content = "Disconnect";

                ConnectionIndicator.Fill =
                    Brushes.Green;

                ConnectionStatusText.Text =
                    "Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to open {port}.\n\n{ex.Message}",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                SetDisconnectedState();
            }
        }


        private void Serial_DataReceived(
            object? sender,
            string data)
        {
            Dispatcher.Invoke(() =>
            {
                Log($"RX  {data}");
            });
        }


        private void Serial_StatusChanged(
            object? sender,
            string status)
        {
            Dispatcher.Invoke(() =>
            {
                Log($"--- {status}");
            });
        }


        private void SetDisconnectedState()
        {
            ConnectButton.Content = "Connect";

            ConnectionIndicator.Fill =
                Brushes.Gray;

            ConnectionStatusText.Text =
                "Disconnected";
        }


        private void ClearLogButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }


        private void Log(string message)
        {
            LogTextBox.AppendText(
                $"{DateTime.Now:HH:mm:ss.fff}  {message}\r\n");

            LogTextBox.ScrollToEnd();
        }


        protected override void OnClosed(
            EventArgs e)
        {
            _serial.Disconnect();

            base.OnClosed(e);
        }

        private void TestButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (!_serial.IsConnected)
            {
                MessageBox.Show(
                    "Please connect to the camera first.",
                    "Not Connected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            byte[] command =
            {
        0x81,
        0x09,
        0x04,
        0x00,
        0xFF
    };

            Log("TX  81 09 04 00 FF");

            try
            {
                _serial.Send(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Send Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PanTiltStop_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  PAN/TILT STOP");

            _camera.PanTiltStop();
        }

        private void PanTiltSpeedSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
        {
            if (_camera == null)
                return;

            int speed = (int)Math.Round(e.NewValue);

            _camera.PanTiltSpeed = speed;

            if (PanTiltSpeedText != null)
            {
                PanTiltSpeedText.Text = speed.ToString();
            }
        }

        private void ZoomTele_MouseDown(
    object sender,
    System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  ZOOM TELE");

            _camera.ZoomTele();

            e.Handled = true;
        }


        private void ZoomWide_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  ZOOM WIDE");

            _camera.ZoomWide();

            e.Handled = true;
        }


        private void Zoom_MouseUp(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  ZOOM STOP");

            _camera.ZoomStop();

            e.Handled = true;
        }

        private void FocusAuto_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS AUTO");

            _camera.FocusAuto();
        }


        private void FocusOnePush_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS ONE PUSH");

            _camera.FocusOnePush();
        }


        private void FocusManual_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS MANUAL");

            _camera.FocusManual();
        }


        private void FocusNear_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS NEAR");

            _camera.FocusNear();

            e.Handled = true;
        }


        private void FocusFar_MouseDown(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS FAR");

            _camera.FocusFar();

            e.Handled = true;
        }


        private void Focus_MouseUp(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_serial.IsConnected)
                return;

            Log("TX  FOCUS STOP");

            _camera.FocusStop();

            e.Handled = true;
        }


    }

}