using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel; // for component-based programming
using System.Data; // for handling tabular data
using System.Drawing; // for graphical operations in Windows applications
using System.Windows.Forms; // classes for building graphical user interfaces (GUI)
using System.IO.Ports; // for serial communication with external hardware (Arduino)
using System.Web; // for web development
using System.Diagnostics; //for debugging, performance tracking
using System.Drawing.Drawing2D; // 2d effects (designer)
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Collections.Concurrent;


namespace VS_APP
{
    public partial class PickAndPlace : Form
    {
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timerR;
        private bool isConnecting = false;  // To track whether we're connecting or disconnecting
        bool isResetting = false; // to reset the bar
        private SerialPort serialPort = new SerialPort();
        private int currentX = 0;
        private int currentY = 0;
        private int currentZ = 0;
        private int currentR = 0;
        private int pcbOriginX = 0;
        private int pcbOriginY = 0;
        private int pcbOriginZ = 0;
        private int pcbOriginR = 0;
        private string csvFilePath = "";
        private List<string> csvLines = new List<string>();
        private int currentCsvLineIndex = 1;   
        private bool procedureMode = false;
        private bool waitingForCoordinates = false;
        private int pickOriginX = 0;
        private int pickOriginY = 0;
        private int pickOriginZ = 0;
        private int pickOriginR = 0;
        private bool visionResultReady = false;
        private bool visionNeedsCorrection = false;

        private double visionDxMm = 0;
        private double visionDyMm = 0;
        private double visionDthetaDeg = 0;

        private bool isDarkMode = false;
        private readonly Color Light_Background = Color.FromArgb(240, 240, 240);
        private readonly Color Light_Text = Color.Black;
        private readonly Color Light_Panel = Color.FromArgb(230, 230, 230);
        private readonly Color Light_Button = Color.FromArgb(220, 220, 220);

        private readonly Color Dark_Background = Color.FromArgb(30, 30, 30);
        private readonly Color Dark_Text = Color.Gainsboro;
        private readonly Color Dark_Panel = Color.FromArgb(45, 45, 45);
        private readonly Color Dark_Button = Color.FromArgb(60, 60, 60);
        private RecalibrationForm recalibrationWindow;
        private bool cameraCalibrationActive = false;


        private readonly SemaphoreSlim cameraLock = new SemaphoreSlim(1, 1);
        private int preferredCameraIndex = 1;
        private readonly int[] cameraFallbackOrder = new[] { 1, 0, 2, 3, 4, 5 };

        private const int PreferredCameraRetries = 3;
        private const int PreferredCameraRetryDelayMs = 1000;

        private string activeComponentId = null;
        private int activeExpectedAxis = 0; // 0 or 90

        private readonly SemaphoreSlim _serialWriteLock = new SemaphoreSlim(1, 1);
        private const int SerialWriteTimeoutMs = 1500;



        private enum ProcedureWaitMode
        {
            None = 0,
            WaitingPlacementCoords = 1,
            WaitingCorrectionCoords = 2
        }
        private ProcedureWaitMode procedureWaitMode = ProcedureWaitMode.None;

        private bool correctionSentForThisRequest = false;

        private int terminalLogLines = 0;
        private const int TerminalMaxLines = 10;

        private DateTime _lastPosUiUpdate = DateTime.MinValue; // throttle POS UI
        private int _visionRunning = 0;                        // prevent parallel vision runs
        
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private System.Windows.Forms.Timer _logFlushTimer;
        private const int LogFlushIntervalMs = 100;    
        private const int MaxFlushLinesPerTick = 30;   



        public PickAndPlace()
        {
            InitializeComponent();
            InitLogFlushTimer();
            this.BackColor = Color.FromArgb(240, 240, 240);
            timer1 = new System.Windows.Forms.Timer();
            timer2 = new System.Windows.Forms.Timer();
            timerR = new System.Windows.Forms.Timer();
            Console.WriteLine("The app is running!");
            findAvailableCOMPorts();
            serialPort.BaudRate = 19200;
            chooseBaudRates();
            chooseArrowMovementUnit();
            chooseSetPositionUnit();
            chooseRotationUnit();
            serialPort.DataReceived += SerialPort_DataReceived;
            //GetUserInputDistance();
            //SendMovementCommand();

            comboBoxCOMPorts.SelectedIndexChanged += comboBoxCOMPorts_SelectedIndexChanged;
            //comboBoxBaudRates.SelectedIndexChanged += comboBoxBaudRates_SelectedIndexChanged;
            comboBoxUnitChoice.SelectedIndexChanged += comboBoxUnitChoice_SelectedIndexChanged;
            comboBoxUnitChoice2.SelectedIndexChanged += comboBoxUnitChoice2_SelectedIndexChanged;
            comboBoxUnitChoice3.SelectedIndexChanged += comboBoxUnitChoice3_SelectedIndexChanged;
            comboBoxCOMPorts.DropDown += comboBoxCOMPorts_DropDown;
            
            //textBox12.KeyDown += textBox12_KeyDown;

            // Progress bar 1 settings
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;

            // Progress bar 2 settings
            progressBar2.Minimum = 0;
            progressBar2.Maximum = 100;
            progressBar2.Value = 0;

            // Timer for progress bar
            timer1.Interval = 100; // ms
            timer1.Tick += Timer1_Tick;

            // Timer for progress bar 2
            timer2.Interval = 100;
            timer2.Tick += Timer2_Tick;

            // Reset timer
            timerR.Interval = 5000; // 5 seconds
            timerR.Tick += ResetTimer_Tick;

            textBox7.Text = "0";   // X
            textBox8.Text = "0";   // Y
            textBox9.Text = "0";   // Z
            textBox10.Text = "0";  // R
        }
        private void InitLogFlushTimer()
        {
            _logFlushTimer = new System.Windows.Forms.Timer();
            _logFlushTimer.Interval = LogFlushIntervalMs;
            _logFlushTimer.Tick += (s, e) => FlushLogsToUi();
            _logFlushTimer.Start();
        }
        private class PlacementCommandResult
        {
            public string Command { get; set; }
            public string ComponentId { get; set; }
        }
        private PlacementCommandResult BuildNextPlacementCommandFromCsv()
        {
            while (currentCsvLineIndex < csvLines.Count)
            {
                int usedCsvIndex = currentCsvLineIndex;

                string rawLine = csvLines[currentCsvLineIndex].Trim();
                int lineNumber = currentCsvLineIndex + 1;
                currentCsvLineIndex++;

                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                string[] parts = rawLine.Split(new[] { ';' }, StringSplitOptions.None);
                if (parts.Length < 4)
                    continue;

                string name = parts[0].Trim();
                string xStr = parts[1].Trim();
                string yStr = parts[2].Trim();
                string rStr = parts[3].Trim();

                if (!double.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double x_mm) ||
                    !double.TryParse(yStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double y_mm) ||
                    !double.TryParse(rStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double r_deg))
                    continue;

                double r_norm = r_deg % 180.0;
                if (r_norm < 0) r_norm += 180.0;
                r_norm = (Math.Abs(r_norm - 90.0) < 45.0) ? 90.0 : 0.0;

                int expectedAxis = ((int)Math.Round(r_norm)) % 180;

                activeComponentId = name;
                activeExpectedAxis = expectedAxis;

                LogSafe($"PLACEMENT CSV index={usedCsvIndex} id={activeComponentId}, expectedAxis={activeExpectedAxis}");

                int x_steps = MmToStepsX(x_mm);
                int y_steps = MmToStepsY(y_mm);
                int r_steps = DegToStepsR(r_norm);

                int targetX = pcbOriginX - x_steps - 146; // 108 before (maybe 98?)
                int targetY = pcbOriginY + y_steps - 102; // before 97 
                int targetZ = pcbOriginZ;
                int targetR = pcbOriginR + r_steps;

                int moveX = targetX - pickOriginX;
                int moveY = targetY - pickOriginY;
                int moveZ = targetZ - pickOriginZ;
                int moveR = targetR - pickOriginR;

                string command = $"X{moveX} Y{moveY} R{moveR}";

                return new PlacementCommandResult
                {
                    Command = command,
                    ComponentId = name
                };
            }

            return null;
        }
        private async Task<bool> SendSerialLineAsync(string line, int timeoutMs = SerialWriteTimeoutMs)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            if (serialPort == null || !serialPort.IsOpen)
            {
                LogWithTimestamp("SERIAL ERROR: port not open (cannot send).");
                return false;
            }

            await _serialWriteLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var writeTask = Task.Run(() =>
                {
                    try
                    {
                        serialPort.WriteLine(line);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogSafe("SERIAL WRITE EXCEPTION: " + ex.Message);
                        return false;
                    }
                });

                var finished = await Task.WhenAny(writeTask, Task.Delay(timeoutMs)).ConfigureAwait(false);
                if (finished != writeTask)
                {
                    LogSafe($"SERIAL WRITE TIMEOUT ({timeoutMs}ms) line dropped: {line}");
                    return false;
                }

                return await writeTask.ConfigureAwait(false);
            }
            finally
            {
                _serialWriteLock.Release();
            }
        }
        private void EnqueueLog(string message)
        {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logQueue.Enqueue($"[{timestamp}] {message}");
        }
        private void LogWithTimestamp(string message)
        {
            
            EnqueueLog(message);
        }
        private void FlushLogsToUi()
        {
            if (terminalBox == null || terminalBox.IsDisposed) return;

            int flushed = 0;
            var sb = new StringBuilder();

            while (flushed < MaxFlushLinesPerTick && _logQueue.TryDequeue(out string line))
            {
                sb.AppendLine(line);
                flushed++;
                terminalLogLines++;

                if (terminalLogLines >= TerminalMaxLines)
                {
                    terminalBox.Clear();
                    terminalLogLines = 0;
                    terminalBox.AppendText($"[{DateTime.Now:HH:mm:ss}] --- terminal auto-cleared after {TerminalMaxLines} logs ---\r\n");
                    terminalLogLines = 1;
                }
            }

            if (sb.Length > 0)
            {
                terminalBox.AppendText(sb.ToString());
                terminalBox.SelectionStart = terminalBox.Text.Length;
                terminalBox.ScrollToCaret();
            }
        }
        void findAvailableCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames(); // finds available COM ports

            comboBoxCOMPorts.Items.Clear(); // Clear previous entries

            if (ports.Length > 0)
            {
                foreach (string port in ports)
                {
                    comboBoxCOMPorts.Items.Add(port); // Add each port to the ComboBox
                }

                foreach (string port in ports)
                {
                    // empty
                }
            }
            else
            {
                comboBoxCOMPorts.Items.Add("No available COM ports");
            }
        }
        private void comboBoxCOMPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPort = comboBoxCOMPorts.SelectedItem.ToString();

            if (selectedPort == "No available COM ports")
            {
                terminalBox.AppendText(">> No COM port selected <<" + Environment.NewLine);
                terminalBox.SelectionStart = terminalBox.Text.Length;
                terminalBox.ScrollToCaret();
            }
            else
            {
                serialPort.PortName = selectedPort;
                terminalBox.AppendText($">> COM port selected: {selectedPort} <<" + Environment.NewLine);
                terminalBox.SelectionStart = terminalBox.Text.Length;
                terminalBox.ScrollToCaret();
            }
        }
        private void comboBoxCOMPorts_DropDown(object sender, EventArgs e)
        {
            findAvailableCOMPorts();
        }
        void chooseBaudRates()
        {
            comboBoxBaudRates.Items.Clear();
            comboBoxBaudRates.Items.Add("19200");
            comboBoxBaudRates.SelectedIndex = 0;
        }
        private void comboBoxBaudRates_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.BaudRate = 19200;
        }
        void chooseArrowMovementUnit()
        {
            string[] units = { "mm", "steps" };
            comboBoxUnitChoice.Items.Clear();

            foreach (string unit in units)
            {
                comboBoxUnitChoice.Items.Add(unit);
            }

            comboBoxUnitChoice.SelectedIndex = 0; // default = "mm"
        }
        private void chooseSetPositionUnit()
        {
            string[] units = { "steps", "mm" };
            comboBoxUnitChoice2.Items.Clear();

            foreach (string unit in units)
            {
                comboBoxUnitChoice2.Items.Add(unit);
            }

            comboBoxUnitChoice2.SelectedIndex = 1; 
        }
        private void chooseRotationUnit()
        {
            string[] units = { "steps", "degrees" };
            comboBoxUnitChoice3.Items.Clear();

            foreach (string unit in units)
            {
                comboBoxUnitChoice3.Items.Add(unit);
            }

            comboBoxUnitChoice3.SelectedIndex = 1; 
        }
        private void comboBoxUnitChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUnit = comboBoxUnitChoice.SelectedItem.ToString();
            terminalBox.AppendText($">> Movement unit selected: {selectedUnit} <<" + Environment.NewLine);
            terminalBox.SelectionStart = terminalBox.Text.Length;
            terminalBox.ScrollToCaret();
        }
        private void comboBoxUnitChoice2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUnit = comboBoxUnitChoice2.SelectedItem.ToString();
            LogWithTimestamp($">> Set Position Unit selected: {selectedUnit} <<");
        }
        private void comboBoxUnitChoice3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string unit = comboBoxUnitChoice3.SelectedItem.ToString();
            LogWithTimestamp($">> Rotation Unit selected: {unit} <<");
        }
        private double GetUserInputDistance()
        {
            string selectedUnit = comboBoxUnitChoice.SelectedItem?.ToString() ?? "mm";

            if (!double.TryParse(textBox12.Text, out double value))
            {
                LogWithTimestamp("ERROR: Invalid input value. Please enter a number.");
                return -1; 
            }

            return value;
        }
        private void SendMovementCommand(string axis, double distance)
        {
            string selectedUnit = comboBoxUnitChoice.SelectedItem?.ToString() ?? "mm";

            if (cameraCalibrationActive)
            {
                if (axis != "X" && axis != "X-" && axis != "Y" && axis != "Y-")
                {
                    LogWithTimestamp("CALIBRATION MODE: Z and R are ignored.");
                    return;
                }

                int steps;

                if (selectedUnit == "mm")
                {
                    if (axis == "X" || axis == "X-")
                        steps = (int)Math.Round(distance * 26.57);    // X mm to steps
                    else
                        steps = (int)Math.Round(distance * 19.95);   // Y mm to steps
                }
                else
                {
                    steps = (int)Math.Round(distance); // already steps
                }

                if (axis == "X-" || axis == "Y-")
                    steps = -steps;

                string cmd = axis.StartsWith("X")
                    ? $"CX:{steps}"
                    : $"CY:{steps}";

                if (serialPort.IsOpen)
                {
                    serialPort.WriteLine(cmd);
                    LogWithTimestamp($">> CALIBRATION CMD: {cmd}");
                }
                else
                {
                    LogWithTimestamp("ERROR: Serial port not open");
                }

                return;
            }

            string command;
            if (selectedUnit == "mm")
                command = $"{axis}{distance}mm";
            else
                command = $"{axis}{distance}";

            if (serialPort.IsOpen)
            {
                serialPort.WriteLine(command);
                LogWithTimestamp($">> Sent command: {command}");
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void textBox12_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; 

                string selectedUnit = comboBoxUnitChoice.SelectedItem?.ToString() ?? "mm";
                string textValue = textBox12.Text;

                if (!double.TryParse(textValue, out double value))
                {
                    LogWithTimestamp("ERROR: Invalid input. Please enter a numeric value.");
                    return;
                }

                bool valid = true;
                if (selectedUnit == "steps" && (value < 1 || value > 10000))
                {
                    LogWithTimestamp("ERROR: Value out of range (1–10000 steps).");
                    valid = false;
                }
                else if (selectedUnit == "mm" && (value < 0.1 || value > 10000))
                {
                    LogWithTimestamp("ERROR: Value out of range (0.1–10000 mm).");
                    valid = false;
                }

                if (valid)
                {
                    LogWithTimestamp($">> Movement value set to {value} {selectedUnit} <<");
                }
            }
        }
        private string serialBuffer = "";
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadExisting();
                serialBuffer += data;

                const int MaxSerialBuffer = 50000;
                if (serialBuffer.Length > MaxSerialBuffer)
                    serialBuffer = serialBuffer.Substring(Math.Max(0, serialBuffer.Length - 20000));

                List<Action> uiActions = null;

                while (serialBuffer.Contains("\n"))
                {
                    int newlineIndex = serialBuffer.IndexOf('\n');
                    string line = serialBuffer.Substring(0, newlineIndex).Trim();
                    serialBuffer = serialBuffer.Substring(newlineIndex + 1);

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("POS"))
                    {
                        if ((DateTime.Now - _lastPosUiUpdate).TotalMilliseconds < 100)
                            continue;

                        _lastPosUiUpdate = DateTime.Now;

                        int xSteps = currentX;
                        int ySteps = currentY;
                        int zSteps = currentZ;

                        int ExtractSteps(string src, string axis)
                        {
                            string marker = axis + ":";
                            int idx = src.IndexOf(marker);
                            if (idx < 0) return axis == "X" ? xSteps :
                                                axis == "Y" ? ySteps :
                                                            zSteps;

                            int start = idx + marker.Length;
                            int end = src.IndexOf("steps", start, StringComparison.OrdinalIgnoreCase);
                            if (end < 0) return axis == "X" ? xSteps :
                                                axis == "Y" ? ySteps :
                                                            zSteps;

                            string numberText = src.Substring(start, end - start).Trim();
                            return int.TryParse(numberText, out int parsed)
                                ? parsed
                                : (axis == "X" ? xSteps : axis == "Y" ? ySteps : zSteps);
                        }

                        ySteps = ExtractSteps(line, "Y");
                        xSteps = ExtractSteps(line, "X");
                        zSteps = ExtractSteps(line, "Z");

                        currentX = xSteps;
                        currentY = ySteps;
                        currentZ = zSteps;

                        double xMM = xSteps / 26.57;
                        double yMM = ySteps / 19.95;
                        double zMM = zSteps / 199.045;

                        uiActions ??= new List<Action>();
                        uiActions.Add(() =>
                        {
                            textBox4.Text = $"{currentX} steps / {xMM:0.00} mm";
                            textBox5.Text = $"{currentY} steps / {yMM:0.00} mm";
                            textBox6.Text = $"{currentZ} steps / {zMM:0.00} mm";
                        });

                        continue;
                    }

                    CheckArduinoErrorMessage(line);

                    if (line.IndexOf("AWAITING_APPROVAL", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uiActions ??= new List<Action>();
                        uiActions.Add(() => HandleApprovalRequest());
                        continue;
                    }

                    if (line.Equals("ARRIVED_AT_INSPECTION_POSITION", StringComparison.OrdinalIgnoreCase))
                    {
                        uiActions ??= new List<Action>();
                        uiActions.Add(() =>
                        {
                            LogWithTimestamp("VISION: Inspection position reached. Starting vision pipeline");
                            _ = RunVisionPipelineAsync();
                        });
                        continue;
                    }

                    if (line.IndexOf("AWAITING_FOR_COORDINATES_CORRECTION", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.IndexOf("AWAITING_COORDINATES_FOR_CORRECTION", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uiActions ??= new List<Action>();
                        uiActions.Add(() =>
                        {
                            procedureWaitMode = ProcedureWaitMode.WaitingCorrectionCoords;
                            correctionSentForThisRequest = false;

                            LogWithTimestamp("Arduino requests CORRECTION coords (DELTA) waiting for vision output");
                            TrySendVisionCorrectionIfReady();
                        });
                        continue;
                    }

                    if (line.IndexOf("AWAITING_COORDINATES", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uiActions ??= new List<Action>();
                        uiActions.Add(() =>
                        {
                            procedureWaitMode = ProcedureWaitMode.WaitingPlacementCoords;
                            _ = HandleWaitingForCoordinatesAsync(); 
                        });
                        continue;
                    }

                }

                if (uiActions != null && uiActions.Count > 0)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        foreach (var a in uiActions) a();
                    }));
                }
            }
            catch
            {
            }
        }
        private string ExtractValue(string input, string key)
        {
            int start = input.IndexOf(key);
            if (start == -1) return null;

            start += key.Length;
            int end = input.IndexOf(' ', start);
            if (end == -1) end = input.Length;

            string value = input.Substring(start, end - start).Trim();
            return value;
        }   
        private void CheckArduinoErrorMessage(string line)
        {
            string[] validErrors =
            {
                "ERROR: Home the machine before starting procedure.",
                "ERROR: Pick reference not set! Use SETPICK first.",
                "ERROR: PCB reference not set! Use SETPCB first."
            };

            foreach (string error in validErrors)
            {
                if (line.Contains(error))
                {
                    LogSafe(error);
                    break;
                }
            }
        }
        private int MmToStepsX(double mm)
        {
            return (int)Math.Round(mm * 26.57);
        }
        private int MmToStepsY(double mm)
        {
            return (int)Math.Round(mm * 19.95);
        }
        private int MmToStepsZ(double mm)
        {
            return (int)Math.Round(mm * 199.045);
        }
        private int DegToStepsR(double degrees)
        {
            return (int)Math.Round(degrees * (2650.0 / 360.0));
        }
        private void button1_Click(object sender, EventArgs e)
        {

            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("HOME");
                LogWithTimestamp("Performing homing");        
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("S");
                LogWithTimestamp("Stopped all processes");
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (progressBar1.Value >= progressBar1.Maximum)
            {
                LogWithTimestamp("Already connected");
                return;
            }

            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.BaudRate = 19200;
                    serialPort.Open();

                    // Apply serial port settings for faster response
                    serialPort.NewLine = "\n";        // Arduino uses \n as line ending
                    serialPort.ReadTimeout = 500;     // stop waiting after 0.5 s
                    serialPort.DiscardInBuffer();     // clear old incoming data
                    serialPort.DiscardOutBuffer();    // clear old outgoing data
                    serialPort.BaseStream.Flush();    // ensure clean stream

                    LogWithTimestamp("Serial port opened");
                    LogWithTimestamp("Serial buffers cleared and ready");

                    // (Optional) Immediately request position to start updates faster
                    // serialPort.WriteLine("GETPOS");
                }
                catch (Exception ex)
                {
                    LogWithTimestamp($">> ERROR: {ex.Message}");
                    return; // Don’t continue if connection failed
                }
            }

            isConnecting = true;
            timer1.Start();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (progressBar1.Value <= progressBar1.Minimum)
            {
                LogWithTimestamp("Already disconnected");
                return;
            }

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    LogWithTimestamp("Serial port closed");
                }
                catch (Exception ex)
                {
                    LogWithTimestamp($">> ERROR: {ex.Message}");
                }
            }

            isConnecting = false;
            timer1.Start();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            terminalBox.Clear();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                LogWithTimestamp("Sending new coordinates");
                progressBar2.Value = 0;
                isResetting = false;
                timer2.Start();
                timerR.Stop();     
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("SETPICK");
                LogWithTimestamp("Picking spot has been set");

                pickOriginX = currentX;
                pickOriginY = currentY;
                pickOriginZ = currentZ;
                pickOriginR = currentR;

                LogWithTimestamp(
                    $"PICK ORIGIN SAVED X={pickOriginX}  Y={pickOriginY}  Z={pickOriginZ}  R={pickOriginR}"
                );
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("SETPCB");
                LogWithTimestamp("PCB origin place has been set");

                pcbOriginX = currentX;
                pcbOriginY = currentY;
                pcbOriginZ = currentZ;
                pcbOriginR = currentR;

                LogWithTimestamp($"PCB ORIGIN SAVED X={pcbOriginX}  Y={pcbOriginY}  Z={pcbOriginZ}  R={pcbOriginR}");
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            CSVBox.Clear();
            LoadCsvFileAllInOne();
        }
        private void button13_Click(object sender, EventArgs e)
        {
                if (!serialPort.IsOpen)
            {
                LogWithTimestamp("ERROR: Serial port not open");
                return;
            }

                if (csvLines == null || csvLines.Count <= 1)
            {
                LogWithTimestamp("ERROR: Load CSV/TXT file first!");
                return;
            }

            // Reset CSV reading
            currentCsvLineIndex = 1; // skip header line
            procedureMode = true;

            serialPort.WriteLine("PROCEDURE");
            LogWithTimestamp("Procedure started.");            
        }
        private void button14_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("ABORT"); 
                procedureMode = false; 
                LogWithTimestamp("Procedure aborted");    
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }            
        }
        private void button15_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("RESET");
                LogWithTimestamp("Resetting");
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button16_Click(object sender, EventArgs e) // Move up X+
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving up along X-axis");
                    SendMovementCommand("X", value);
                }     
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button17_Click(object sender, EventArgs e) // Move down X-
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving down along X-axis");
                    SendMovementCommand("X-", value);
                }    
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button18_Click(object sender, EventArgs e) // Move right Y+
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving right along Y-axis");
                    SendMovementCommand("Y", value);
                } 
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button19_Click(object sender, EventArgs e) // Move left Y-
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving left along Y-axis");
                    SendMovementCommand("Y-", value);
                }
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button20_Click(object sender, EventArgs e) // Move up Z+
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving up along X-axis");
                    SendMovementCommand("Z", value);
                }     
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button21_Click(object sender, EventArgs e) // Move down Z-
        {
            if (serialPort.IsOpen)
            {
                double value = GetUserInputDistance();
                if (value > 0)
                {
                    LogWithTimestamp("Moving down along X-axis");
                    SendMovementCommand("Z-", value);
                }    
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private void button22_Click(object sender, EventArgs e) // Move down Z-
        {
            if (serialPort.IsOpen)
            {
                  try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                    string projectRoot = Path.GetFullPath(
                        Path.Combine(baseDir, @"..\..\..\..")
                    );

                    string pythonExe = Path.Combine(
                        projectRoot,
                        @"vision\venv\Scripts\python.exe"
                    );

                    string scriptPath = Path.Combine(
                        projectRoot,
                        @"vision\live_view.py"
                    );

                    if (!File.Exists(pythonExe))
                    {
                        MessageBox.Show("python.exe not found:\n" + pythonExe);
                        return;
                    }

                    if (!File.Exists(scriptPath))
                    {
                        MessageBox.Show("live_view.py not found:\n" + scriptPath);
                        return;
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = $"\"{scriptPath}\" --device 1",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process.Start(psi);

                    LogWithTimestamp("CAMERA: Live view started (press Q or ESC to close)");
                }
                catch (Exception ex)
                {
                    LogWithTimestamp("CAMERA ERROR:");
                    LogWithTimestamp(ex.Message);
                }
            }
             else
             {
                 LogWithTimestamp("ERROR: Serial port not open");
             }
        }
        private void button23_Click(object sender, EventArgs e) 
        {
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("CALIBRATE_CAMERA");
                LogWithTimestamp("Calibration window opened");

                if (recalibrationWindow == null || recalibrationWindow.IsDisposed)
                {
                    recalibrationWindow = new RecalibrationForm(
                        serialPort,
                        onMoveToCamera: () =>
                        {
                            cameraCalibrationActive = true;
                            LogWithTimestamp("CAMERA CALIBRATION ACTIVE (CX/CY mode ON)");
                        },
                        onWindowClosed: () =>
                        {
                            cameraCalibrationActive = false;
                            LogWithTimestamp("CAMERA CALIBRATION OFF (normal X/Y/Z/R mode)");
                        }
                    );

                    recalibrationWindow.FormClosed += (s, args) =>
                    {
                        cameraCalibrationActive = false;
                        LogWithTimestamp("Calibration window closed; CX/CY mode OFF");
                        recalibrationWindow = null;
                    };

                    recalibrationWindow.Show(this);
                }
                else
                {
                    recalibrationWindow.BringToFront();
                    recalibrationWindow.Focus();
                }       
            }
            else
            {
                LogWithTimestamp("ERROR: Serial port not open");
            }
        }
        private async void TrySendVisionCorrectionIfReady()
        {
            if (!procedureMode)
                return;

            if (procedureWaitMode != ProcedureWaitMode.WaitingCorrectionCoords)
                return;

            if (correctionSentForThisRequest)
                return;

            if (!visionResultReady)
            {
                LogWithTimestamp("VISION: correction requested but visionResultReady=false (waiting)...");
                return;
            }

            if (!visionNeedsCorrection)
            {
                LogWithTimestamp("VISION: Arduino requested correction, but vision says CORRECT. (Not sending delta)");
                return;
            }

            int xSteps = MmToStepsX(visionDxMm);
            int ySteps = MmToStepsY(visionDyMm);
            int rSteps = DegToStepsR(visionDthetaDeg);

            string cmd = $"X{xSteps} Y{ySteps} R{rSteps}";

            bool ok = await SendSerialLineAsync(cmd);
            if (!ok)
            {
                LogWithTimestamp("VISION ERROR: Failed to send correction (timeout/serial issue).");
                return;
            }

            LogWithTimestamp($"VISION to Arduino (CORRECTION DELTA): {cmd}");

            correctionSentForThisRequest = true;
            procedureWaitMode = ProcedureWaitMode.None;
        }
        private void LogSafe(string message)
        {
            EnqueueLog(message);
        }
        private async Task<string> CaptureImageAsyncWithRetry(int attempts, int initialDelayMs)
        {
            int delay = initialDelayMs;

            for (int i = 1; i <= attempts; i++)
            {
                string result = await CaptureImageAsync(); 
                if (!string.IsNullOrWhiteSpace(result))
                    return result;

                LogWithTimestamp($"CAPTURE: attempt {i}/{attempts} failed. Retrying in {delay} ms...");
                await Task.Delay(delay);

                // backoff
                delay = Math.Min(delay * 2, 2000);
            }

            return null;
        }
        private async Task RunVisionPipelineAsync()
        {
            if (Interlocked.Exchange(ref _visionRunning, 1) == 1)
            {
                LogWithTimestamp("VISION: already running ignored.");
                return;
            }

            await cameraLock.WaitAsync();
            try
            {
                visionResultReady = false;
                visionNeedsCorrection = false;

                LogWithTimestamp("VISION: Starting vision pipeline...");
                await Task.Delay(200);

                string imagePath = await CaptureImageAsyncWithRetry(attempts: 3, initialDelayMs: 250);

                if (imagePath == null)
                {
                    LogWithTimestamp("VISION ERROR: Image capture failed after retries. Procedure will not continue.");
                    return;
                }

                bool ok = await AnalyzeImageAsync(imagePath);
                if (!ok)
                {
                    LogWithTimestamp("VISION ERROR: Analysis failed.");
                    return;
                }

                visionResultReady = true;

                if (!visionNeedsCorrection)
                {
                    await SendSerialLineAsync("CORRECT");
                    LogWithTimestamp("VISION to Arduino: CORRECT");
                }
                else
                {
                    await SendSerialLineAsync("INCORRECT");
                    LogWithTimestamp("VISION to Arduino: INCORRECT");
                }

                TrySendVisionCorrectionIfReady();
            }
            finally
            {
                cameraLock.Release();
                Interlocked.Exchange(ref _visionRunning, 0);
            }
        }
        private async Task<string> CaptureImageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));

                    string pythonExe = Path.Combine(projectRoot, @"vision\venv\Scripts\python.exe");
                    string scriptPath = Path.Combine(projectRoot, @"vision\saving_frame.py");
                    string capturesDir = Path.Combine(projectRoot, @"vision\captures");

                    if (!File.Exists(pythonExe))
                    {
                        LogSafe("CAPTURE ERROR: python.exe not found: " + pythonExe);
                        return null;
                    }

                    if (!File.Exists(scriptPath))
                    {
                        LogSafe("CAPTURE ERROR: saving_frame.py not found: " + scriptPath);
                        return null;
                    }

                    if (!Directory.Exists(capturesDir))
                    {
                        Directory.CreateDirectory(capturesDir);
                    }

                    const int preferredTries = 3;
                    const int preferredDelayMs = 1000;

                    int preferredIndex = preferredCameraIndex; 

                    for (int attempt = 1; attempt <= preferredTries; attempt++)
                    {
                        try
                        {
                            LogSafe($"CAPTURE: Trying preferred camera index {preferredIndex} (attempt {attempt}/{preferredTries})");

                            string result = TryCaptureWithIndex(pythonExe, scriptPath, capturesDir, preferredIndex);

                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                preferredCameraIndex = preferredIndex; 
                                LogSafe($"CAPTURE OK (preferred device {preferredIndex}): {result}");
                                return result;
                            }

                            if (attempt < preferredTries)
                            {
                                LogSafe($"CAPTURE: Preferred index {preferredIndex} failed, retrying in {preferredDelayMs} ms...");
                                Thread.Sleep(preferredDelayMs);
                            }
                        }
                        catch (Exception exOne)
                        {
                            LogSafe($"CAPTURE (preferred device {preferredIndex}) EXCEPTION: {exOne.Message}");

                            if (attempt < preferredTries)
                                Thread.Sleep(preferredDelayMs);
                        }
                    }

                    foreach (int camIndex in cameraFallbackOrder.Distinct())
                    {
                        if (camIndex == preferredIndex)
                            continue;

                        try
                        {
                            LogSafe($"CAPTURE: Trying fallback camera index {camIndex}");

                            string result = TryCaptureWithIndex(pythonExe, scriptPath, capturesDir, camIndex);

                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                preferredCameraIndex = camIndex; // remember the last working camera index
                                LogSafe($"CAPTURE OK (fallback device {camIndex}): {result}");
                                return result;
                            }
                        }
                        catch (Exception exFallback)
                        {
                            LogSafe($"CAPTURE (fallback device {camIndex}) EXCEPTION: {exFallback.Message}");
                        }
                    }

                    LogSafe("CAPTURE ERROR: No camera index worked (preferred + fallback).");
                    return null;
                }
                catch (Exception ex)
                {
                    LogSafe("CAPTURE EXCEPTION: " + ex.Message);
                    return null;
                }
            });
        }
        private string TryCaptureWithIndex(
            string pythonExe,
            string scriptPath,
            string capturesDir,
            int camIndex
        )
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" --device {camIndex} --outdir \"{capturesDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)
            };

            using (var proc = new Process { StartInfo = psi })
            {
                proc.Start();

                var outTask = proc.StandardOutput.ReadToEndAsync();
                var errTask = proc.StandardError.ReadToEndAsync();

                if (!proc.WaitForExit(15000))
                {
                    try { proc.Kill(true); } catch { }
                    LogSafe($"CAPTURE (device {camIndex}) TIMEOUT killed.");
                    return null;
                }

                string stdout = outTask.Result ?? "";
                string stderr = errTask.Result ?? "";

                LogSafe($"CAPTURE (device {camIndex}) stdout: " +
                        stdout.Replace("\r", "\\r").Replace("\n", "\\n"));

                if (!string.IsNullOrWhiteSpace(stderr))
                    LogSafe($"CAPTURE (device {camIndex}) stderr: " +
                            stderr.Replace("\r", "\\r").Replace("\n", "\\n"));

                if (proc.ExitCode != 0)
                {
                    LogSafe($"CAPTURE (device {camIndex}) ERROR: python exit code = {proc.ExitCode}");
                    return null;
                }

                string candidate = stdout
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().Trim('"'))
                    .LastOrDefault(l =>
                        l.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        l.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        l.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrWhiteSpace(candidate))
                    return null;

                if (!Path.IsPathRooted(candidate))
                    candidate = Path.Combine(capturesDir, candidate);

                return File.Exists(candidate) ? candidate : null;
            }
        }
        private async Task<bool> AnalyzeImageAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(activeComponentId))
                    {
                        LogSafe("VISION ERROR: activeComponentId is null (no active placement stored).");
                        return false;
                    }

                    int expectedAxis = activeExpectedAxis; // 0 or 90
                    string componentId = activeComponentId;

                    LogSafe($"VISION INPUT id={componentId}, expectedAxis={expectedAxis}, image={Path.GetFileName(imagePath)}");

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\.."));

                    string pythonExe = Path.Combine(projectRoot, @"vision\venv\Scripts\python.exe");
                    string scriptPath = Path.Combine(projectRoot, @"vision\inspect_resistor.py");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = $"\"{scriptPath}\" --id {componentId} --image \"{imagePath}\" --expected-axis {expectedAxis}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var proc = new Process { StartInfo = psi })
                    {
                        proc.Start();

                        var outTask = proc.StandardOutput.ReadToEndAsync();
                        var errTask = proc.StandardError.ReadToEndAsync();

                        if (!proc.WaitForExit(15000))
                        {
                            try { proc.Kill(true); } catch { }
                            LogSafe("VISION ERROR: Python timeout (killed).");
                            return false;
                        }

                        string output = outTask.Result ?? "";
                        string err = errTask.Result ?? "";

                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            LogSafe("VISION python stderr:");
                            LogSafe(err);
                        }

                        // Parse JSON result
                        using (JsonDocument doc = JsonDocument.Parse(output))
                        {
                            string status = doc.RootElement.GetProperty("status").GetString();

                            if (status == "NEEDS_CORRECTION")
                            {
                                visionNeedsCorrection = true;
                                visionDxMm = doc.RootElement.GetProperty("dx_mm").GetDouble();
                                visionDyMm = doc.RootElement.GetProperty("dy_mm").GetDouble();
                                visionDthetaDeg = doc.RootElement.GetProperty("dtheta_deg").GetDouble(); // -90..+90

                                LogSafe($"VISION: NEEDS CORRECTION X({visionDxMm}) Y({visionDyMm}) R({visionDthetaDeg})");
                            }
                            else
                            {
                                visionNeedsCorrection = false;
                                LogSafe("VISION: PLACEMENT CORRECT");
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    LogSafe("VISION EXCEPTION:");
                    LogSafe(ex.Message);
                    return false;
                }
            });
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (isConnecting)  
            {
                if (progressBar1.Value < progressBar1.Maximum)
                {
                    progressBar1.Value += 10;
                }
                else
                {
                    timer1.Stop(); // Stop when it's full
                    LogWithTimestamp("Connected to the machine");
                    LogWithTimestamp("Please home the machine first");
                }
            }
            else  
            {
                if (progressBar1.Value > progressBar1.Minimum)
                {
                    progressBar1.Value -= 10;
                }
                else
                {
                    timer1.Stop(); // Stop when it's empty
                    LogWithTimestamp("Disconnected from the machine");
                }
            }
        }
        private async void Timer2_Tick(object sender, EventArgs e)
        {
            if (!isResetting)
            {
                if (progressBar2.Value < progressBar2.Maximum)
                {
                    progressBar2.Value += 10;
                }
                else
                {
                    timer2.Stop();

                    string xStr = textBox7.Text.Trim();
                    string yStr = textBox8.Text.Trim();
                    string zStr = textBox9.Text.Trim();
                    string rStr = textBox10.Text.Trim();

                    if (!int.TryParse(xStr, out int x) ||
                        !int.TryParse(yStr, out int y) ||
                        !int.TryParse(zStr, out int z) ||
                        !int.TryParse(rStr, out int r))
                    {
                        LogWithTimestamp("ERROR: Enter only whole numbers in all coordinate fields!");
                        goto RESET_PHASE;
                    }

                    LogWithTimestamp("New coordinates sent");
                    LogWithTimestamp($">> X={x}, Y={y}, Z={z}, R={r} <<");

                    string unitXYZ = comboBoxUnitChoice2.SelectedItem?.ToString() ?? "steps";
                    string unitR = comboBoxUnitChoice3.SelectedItem?.ToString() ?? "steps";

                    int rToSend = r;
                    if (unitR == "degrees")
                        rToSend = DegToStepsR(r);

                    string command;

                    if (cameraCalibrationActive)
                    {
                        if (unitXYZ == "mm")
                            command = $"CX: {x}mm CY: {y}mm";
                        else
                            command = $"CX: {x} CY: {y}";

                        LogWithTimestamp("CALIBRATION MODE: sending CX/CY (Z & R ignored)");
                    }
                    else
                    {
                        if (unitXYZ == "mm")
                            command = $"X{x}mm Y{y}mm Z{z}mm R{rToSend}";
                        else
                            command = $"X{x} Y{y} Z{z} R{rToSend}";
                    }

                    if (serialPort.IsOpen)
                    {
                        serialPort.WriteLine(command);
                        LogWithTimestamp($"Sent to Arduino: {command}");
                    }
                    else
                    {
                        LogWithTimestamp("ERROR: Serial port not open");
                    }

                RESET_PHASE:
                    await Task.Delay(3000);
                    isResetting = true;
                    timer2.Start();
                }
            }
            else
            {
                if (progressBar2.Value > 0)
                {
                    progressBar2.Value -= 10;
                }
                else
                {
                    timer2.Stop();
                    isResetting = false;

                    textBox7.Text = "0";
                    textBox8.Text = "0";
                    textBox9.Text = "0";
                    textBox10.Text = "0";

                    LogWithTimestamp("Position controls reset to 0 after send.");
                }
            }
        }
        private void ResetTimer_Tick(object sender, EventArgs e)
        {
            progressBar2.Value = 0;
            timerR.Stop(); // Stop the timer so it doesn't keep firing
        }
        private void LoadCsvFileAllInOne()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open CSV/TXT File";
                dialog.Filter = "Text or CSV files|*.txt;*.csv";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    csvFilePath = dialog.FileName;

                    try
                    {
                        csvLines = File.ReadAllLines(csvFilePath).ToList();

                        currentCsvLineIndex = 1;

                        CSVBox.Text = string.Join(Environment.NewLine, csvLines);

                        LogWithTimestamp($"CSV file loaded: {csvFilePath}");
                        LogWithTimestamp($"Data lines found: {csvLines.Count - 1}");
                    }
                    catch (Exception ex)
                    {
                        LogWithTimestamp($"ERROR while loading CSV: {ex.Message}");
                    }
                }
                else
                {
                    LogWithTimestamp("CSV load cancelled.");
                }
            }
        }
        private bool HasNextValidCsvRow()
        {
            if (csvLines == null || csvLines.Count <= 1) 
                return false;

            for (int i = currentCsvLineIndex; i < csvLines.Count; i++)
            {
                string row = csvLines[i].Trim();
                if (string.IsNullOrWhiteSpace(row))
                    continue;

                string[] parts = row.Split(';');
                if (parts.Length >= 4 &&
                    !string.IsNullOrWhiteSpace(parts[1]) &&
                    !string.IsNullOrWhiteSpace(parts[2]) &&
                    !string.IsNullOrWhiteSpace(parts[3]))
                {
                    return true; // Found a valid row
                }
            }
            return false; // No valid rows left
        }
        private async void HandleApprovalRequest()
        {
            if (!procedureMode)
            {
                LogWithTimestamp("Received 'waiting for approval' but procedure mode is not active.");
                return;
            }

            if (!HasNextValidCsvRow())
            {
                LogWithTimestamp("No valid CSV rows left. Sending ABORT.");
                await SendSerialLineAsync("ABORT");
                procedureMode = false;
                procedureWaitMode = ProcedureWaitMode.None;
                return;
            }

            bool ok = await SendSerialLineAsync("APPROVE");
            if (ok)
            {
                LogWithTimestamp("Sent APPROVE to Arduino");

                waitingForCoordinates = true;
                procedureWaitMode = ProcedureWaitMode.WaitingPlacementCoords;

                // reset correction gate for next component cycle
                correctionSentForThisRequest = false;
            }
            else
            {
                LogWithTimestamp("ERROR: Failed to send APPROVE (timeout/serial issue).");
            }
        }
        private async Task HandleWaitingForCoordinatesAsync()
        {
            if (!procedureMode)
            {
                LogWithTimestamp("Received 'AWAITING_COORDINATES' but procedure mode is not active.");
                return;
            }

            if (procedureWaitMode != ProcedureWaitMode.WaitingPlacementCoords)
            {
                LogWithTimestamp($"Ignoring AWAITING_COORDINATES because waitMode={procedureWaitMode}");
                return;
            }

            if (!waitingForCoordinates)
            {
                LogWithTimestamp("Received 'AWAITING_COORDINATES' but APPROVE was not yet sent.");
                return;
            }

            if (csvLines == null || csvLines.Count <= 1)
            {
                LogWithTimestamp("ERROR: No CSV data loaded. Aborting procedure.");
                await SendSerialLineAsync("S");
                procedureMode = false;
                procedureWaitMode = ProcedureWaitMode.None;
                return;
            }

            PlacementCommandResult result = await Task.Run(() => BuildNextPlacementCommandFromCsv());

            if (result == null)
            {
                LogWithTimestamp("No more data lines in CSV. Procedure complete.");
                await SendSerialLineAsync("S");
                procedureMode = false;
                procedureWaitMode = ProcedureWaitMode.None;
                return;
            }

            bool ok = await SendSerialLineAsync(result.Command);
            if (ok)
            {
                LogWithTimestamp($"Sent RELATIVE move for {result.ComponentId}: {result.Command}");
                procedureWaitMode = ProcedureWaitMode.None;
            }
            else
            {
                LogWithTimestamp("ERROR: Failed to send placement coords (timeout/serial issue).");
            }
        }
        private void AddLabelToPanel(Panel panel, string labelText, int x, int y)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new System.Drawing.Point(x, y); // Position inside the panel
            label.Size = new System.Drawing.Size(500, 30); // Width and Height of label
            label.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold); // Optional styling
            panel.Controls.Add(label); // Add the label to the given panel
            label.BackColor = Color.Transparent; // Removes any rectangle background
        }
        // private void SendAbort()
        // {
        //     if (serialPort.IsOpen)
        //     {
        //         serialPort.WriteLine("S");
        //         LogWithTimestamp("Abort (S) command sent");
        //     }
        //     else
        //     {
        //         LogWithTimestamp("ERROR: Serial port not open (abort failed)");
        //     }
        // }
    }
// ******************** DESIGNER CLASS ******************** //
    public class RoundedPanel : Panel // for rounding the edges and adding the gradient to the panels
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphicsPath path = new GraphicsPath();
            int radius = 20; // Border radius

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);

            // Gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.LightGray, Color.Gray, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}
    