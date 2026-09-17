using System;

namespace SonyPTZController
{
    public class ViscaCamera
    {
        private readonly SerialConnection _serial;
        public int PanTiltSpeed { get; set; } = 8;

        public ViscaCamera(SerialConnection serial)
        {
            _serial = serial;
        }

        // ---------------------------------------------------------
        // POWER
        // ---------------------------------------------------------

        public void PowerOn()
        {
            Send(0x81, 0x01, 0x04, 0x00, 0x02, 0xFF);
        }

        public void PowerOff()
        {
            Send(0x81, 0x01, 0x04, 0x00, 0x03, 0xFF);
        }

        public void PowerInquiry()
        {
            Send(0x81, 0x09, 0x04, 0x00, 0xFF);
        }


        // ---------------------------------------------------------
        // AUTO FOCUS
        // ---------------------------------------------------------

        public void FocusAuto()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x38,
                0x02,
                0xFF);
        }


        public void FocusManual()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x38,
                0x03,
                0xFF);
        }


        public void FocusOnePush()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x18,
                0x01,
                0xFF);
        }
        public void FocusNear()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x08,
                0x02,
                0xFF);
        }


        public void FocusFar()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x08,
                0x03,
                0xFF);
        }


        public void FocusStop()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x08,
                0x00,
                0xFF);
        }

        // ---------------------------------------------------------
        // ZOOM
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // ZOOM
        // ---------------------------------------------------------

        public void ZoomStop()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x07,
                0x00,
                0xFF);
        }


        public void ZoomTele()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x07,
                0x20,
                0xFF);
        }


        public void ZoomWide()
        {
            Send(
                0x81,
                0x01,
                0x04,
                0x07,
                0x30,
                0xFF);
        }

        // ---------------------------------------------------------
        // PAN / TILT
        // ---------------------------------------------------------

        public void PanTiltStop()
        {
            Send(
                0x81,
                0x01,
                0x06,
                0x01,
                0x18,
                0x18,
                0x03,
                0x03,
                0xFF);
        }


        public void PanLeft()
        {
            int speed = ClampSpeed(PanTiltSpeed);

            Send(
                0x81,
                0x01,
                0x06,
                0x01,
                (byte)speed,
                0x18,
                0x01,
                0x03,
                0xFF);
        }


        public void PanRight()
        {
            int speed = ClampSpeed(PanTiltSpeed);

            Send(
                0x81,
                0x01,
                0x06,
                0x01,
                (byte)speed,
                0x18,
                0x02,
                0x03,
                0xFF);
        }


        public void TiltUp()
        {
            int speed = ClampSpeed(PanTiltSpeed);

            Send(
                0x81,
                0x01,
                0x06,
                0x01,
                0x18,
                (byte)speed,
                0x03,
                0x01,
                0xFF);
        }


        public void TiltDown()
        {
            int speed = ClampSpeed(PanTiltSpeed);

            Send(
                0x81,
                0x01,
                0x06,
                0x01,
                0x18,
                (byte)speed,
                0x03,
                0x02,
                0xFF);
        }


        // ---------------------------------------------------------
        // INTERNAL HELPERS
        // ---------------------------------------------------------

        private void Send(params byte[] command)
        {
            _serial.Send(command);
        }


        private int ClampSpeed(int speed)
        {
            if (speed < 1)
                return 1;

            if (speed > 24)
                return 24;

            return speed;
        }
    }
}