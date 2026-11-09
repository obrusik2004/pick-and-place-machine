// Code responsible for the recalibration window

using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Drawing;

namespace VS_APP
{
    public class RecalibrationForm : Form
    {
        private SerialPort _serial;

        private readonly Action _onMoveToCamera;
        private readonly Action _onWindowClosed;

        public RecalibrationForm(SerialPort serialPort, Action onMoveToCamera, Action onWindowClosed)
        {
            _serial = serialPort;
            _onMoveToCamera = onMoveToCamera;
            _onWindowClosed = onWindowClosed;

            this.Text = "Recalibration";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(720, 460);
            this.MaximizeBox = false;

            this.FormClosed += (s, e) => _onWindowClosed?.Invoke();

            BuildUI();
        }

        private void BuildUI()
        {
            Font titleFont = new Font("Segoe UI", 18, FontStyle.Bold);
            Font buttonFont = new Font("Segoe UI", 14, FontStyle.Bold);

            Label title = new Label
            {
                Text = "Recalibration",
                Font = titleFont,
                AutoSize = true,
                Location = new Point(220, 20)
            };

            Button btnCalibrate = CreateButton(
                "CALIBRATE",
                120,
                195,
                buttonFont,
                () => Send("CALIBRATE")
            );

            Button btnRecalibrate = CreateButton(
                "RECALIBRATE",
                120,
                300,
                buttonFont,
                () => Send("RECALIBRATE")
            );

            Button btnMoveToCamera = CreateButton(
                "MOVE TO CAMERA",
                120,
                90,
                buttonFont,
                () =>
                {
                    Send("MOVE_TO_CAMERA");
                    _onMoveToCamera?.Invoke();
                }
            );

            Controls.AddRange(new Control[]
            {
                title,
                btnCalibrate,
                btnRecalibrate,
                btnMoveToCamera
            });
        }

        private Button CreateButton(
            string text,
            int x,
            int y,
            Font font,
            Action onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Font = font,
                Size = new Size(480, 90),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat
            };

            btn.FlatAppearance.BorderSize = 2;
            btn.Click += (s, e) => onClick();

            return btn;
        }

        private void Send(string cmd)
        {
            try
            {
                _serial.WriteLine(cmd);
            }
            catch
            {
                
            }
        }
    }
}
