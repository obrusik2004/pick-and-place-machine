namespace VS_APP;

partial class PickAndPlace
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button button1; 
    private System.Windows.Forms.Button button2; 
    //private System.Windows.Forms.Button button3;     
    private System.Windows.Forms.Button button4;     
    private System.Windows.Forms.Button button5;  
    private System.Windows.Forms.Button button6;     
    private System.Windows.Forms.Button button7;
    private System.Windows.Forms.Button button8;
    private System.Windows.Forms.Button button9;    
    private System.Windows.Forms.Button button12;     
    private System.Windows.Forms.Button button13;     
    private System.Windows.Forms.Button button14;
    private System.Windows.Forms.Button button15;   
    private System.Windows.Forms.Button button16;
    private System.Windows.Forms.Button button17;   
    private System.Windows.Forms.Button button18;   
    private System.Windows.Forms.Button button19;   
    private System.Windows.Forms.Button button20;   
    private System.Windows.Forms.Button button21;  
    private System.Windows.Forms.Button button22;    
    private System.Windows.Forms.Button button23;
    private RoundedPanel panelSection1;
    private RoundedPanel panelSection2;
    private RoundedPanel panelSection3;
    private RoundedPanel panelSection4;
    private RoundedPanel panelSection5;
    private RoundedPanel panelSection6;
    private RoundedPanel panelSection7;
    private RoundedPanel panelSection8;
    private System.Windows.Forms.ComboBox comboBoxCOMPorts;
    private System.Windows.Forms.ComboBox comboBoxBaudRates;
    private System.Windows.Forms.ComboBox comboBoxUnitChoice;
    private System.Windows.Forms.ComboBox comboBoxUnitChoice2;
    private System.Windows.Forms.ComboBox comboBoxUnitChoice3;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.ProgressBar progressBar2;
    private RichTextBox terminalBox;
    private RichTextBox CSVBox;
    private System.Windows.Forms.TextBox textBox7;
    private System.Windows.Forms.TextBox textBox8;
    private System.Windows.Forms.TextBox textBox9;
    private System.Windows.Forms.TextBox textBox10;
    private System.Windows.Forms.TextBox textBox11;
    private System.Windows.Forms.TextBox textBox12;
    private System.Windows.Forms.TextBox textBox4;
    private System.Windows.Forms.TextBox textBox5;
    private System.Windows.Forms.TextBox textBox6;
    private System.Windows.Forms.CheckBox checkBoxDarkMode;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        //  button 1 (HOME)
        this.button1 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button1.Text = "HOME";
        this.button1.Font = new System.Drawing.Font("Arial", 36);  // Set font family and size
        this.button1.Location = new System.Drawing.Point(1615, 1290);
        this.button1.Size = new System.Drawing.Size(635, 240);
        this.button1.BackColor = System.Drawing.Color.Green; // Background color
        this.button1.ForeColor = System.Drawing.Color.White; // Text color
        this.button1.Click += new System.EventHandler(this.button1_Click);
        this.Controls.Add(this.button1);

        this.checkBoxDarkMode = new System.Windows.Forms.CheckBox();
        // =======================
        // CHECKBOX DARK MODE
        // =======================
        // this.checkBoxDarkMode.AutoSize = true;
        // this.checkBoxDarkMode.Name = "checkBoxDarkMode";
        // this.checkBoxDarkMode.Text = "Dark Mode";
        // this.checkBoxDarkMode.Font = new System.Drawing.Font("Segoe UI", 10F);
        // this.checkBoxDarkMode.Location = new System.Drawing.Point(20, 20); // adjust if needed
        // this.checkBoxDarkMode.TabIndex = 999;
        // this.checkBoxDarkMode.Checked = false;
        // this.checkBoxDarkMode.UseVisualStyleBackColor = true;
        // this.checkBoxDarkMode.CheckedChanged += new System.EventHandler(this.checkBoxDarkMode_CheckedChanged);
        


        //  button 2 (STOP)
        this.button2 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button2.Text = "STOP";
        this.button2.Font = new System.Drawing.Font("Arial", 36);  // Set font family and size
        this.button2.Location = new System.Drawing.Point(2270, 1290);
        this.button2.Size = new System.Drawing.Size(635, 500);
        this.button2.BackColor = System.Drawing.Color.Red; // Background color
        this.button2.ForeColor = System.Drawing.Color.White; // Text color
        this.button2.Click += new System.EventHandler(this.button2_Click);
        this.Controls.Add(this.button2);

        //  button 15 (RESET)
        this.button15 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button15.Text = "RESET";
        this.button15.Font = new System.Drawing.Font("Arial", 36);  // Set font family and size
        this.button15.Location = new System.Drawing.Point(1615, 1550);
        this.button15.Size = new System.Drawing.Size(635, 240);
        this.button15.BackColor = System.Drawing.Color.Purple; // Background color
        this.button15.ForeColor = System.Drawing.Color.White; // Text color
        this.button15.Click += new System.EventHandler(this.button15_Click);
        this.Controls.Add(this.button15);

        // //  button 3 (CSV WINDOW)
        // this.button3 = new System.Windows.Forms.Button();
        // this.SuspendLayout();
        // this.button3.Text = "CSV Window";
        // this.button3.Font = new System.Drawing.Font("Arial", 36);  // Set font family and size
        // this.button3.Location = new System.Drawing.Point(20, 1560);
        // this.button3.Size = new System.Drawing.Size(1500, 280);
        // this.button3.BackColor = System.Drawing.Color.DimGray; // Background color
        // this.button3.ForeColor = System.Drawing.Color.White; // Text color
        // this.button3.Click += new System.EventHandler(this.button3_Click);
        // this.Controls.Add(this.button3);

        // ComboBox COM Ports
        this.comboBoxCOMPorts = new System.Windows.Forms.ComboBox();
        this.comboBoxCOMPorts.FormattingEnabled = true;
        this.comboBoxCOMPorts.Location = new System.Drawing.Point(100, 180); // Position of this combobox
        this.comboBoxCOMPorts.Name = "comboBoxCOMPorts";
        this.comboBoxCOMPorts.Size = new System.Drawing.Size(280, 40);
        this.comboBoxCOMPorts.TabIndex = 0;

        // ComboBox Baudrates
        this.comboBoxBaudRates = new System.Windows.Forms.ComboBox();
        this.comboBoxBaudRates.FormattingEnabled = true;
        this.comboBoxBaudRates.Location = new System.Drawing.Point(400, 180); // Position of this combobox
        this.comboBoxBaudRates.Name = "comboBoxBaudRates";
        this.comboBoxBaudRates.Size = new System.Drawing.Size(280, 40);
        this.comboBoxBaudRates.TabIndex = 0;

        // ComboBox Unit Choice 
        this.comboBoxUnitChoice = new System.Windows.Forms.ComboBox();
        this.comboBoxUnitChoice.FormattingEnabled = true;
        this.comboBoxUnitChoice.Location = new System.Drawing.Point(2340, 670); // Position of this combobox 2340
        this.comboBoxUnitChoice.Name = "comboBoxUnitChoice";
        this.comboBoxUnitChoice.Size = new System.Drawing.Size(280, 40);
        this.comboBoxUnitChoice.TabIndex = 0;

        // ComboBox Unit Choice 2
        this.comboBoxUnitChoice2 = new System.Windows.Forms.ComboBox();
        this.comboBoxUnitChoice2.FormattingEnabled = true;
        this.comboBoxUnitChoice2.Location = new System.Drawing.Point(1614, 470); // Position of this combobox
        this.comboBoxUnitChoice2.Name = "comboBoxUnitChoice2";
        this.comboBoxUnitChoice2.Size = new System.Drawing.Size(280, 40);
        this.comboBoxUnitChoice2.TabIndex = 0;

        // ComboBox Unit Choice 3
        this.comboBoxUnitChoice3 = new System.Windows.Forms.ComboBox();
        this.comboBoxUnitChoice3.FormattingEnabled = true;
        this.comboBoxUnitChoice3.Location = new System.Drawing.Point(2604, 470); // Position of this combobox 2604
        this.comboBoxUnitChoice3.Name = "comboBoxUnitChoice3";
        this.comboBoxUnitChoice3.Size = new System.Drawing.Size(280, 40);
        this.comboBoxUnitChoice3.TabIndex = 0;

        //  button 4 (CONNECT)
        this.button4 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button4.Text = "CONNECT";
        this.button4.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button4.Location = new System.Drawing.Point(100, 320);
        this.button4.Size = new System.Drawing.Size(280, 100);
        this.button4.Click += new System.EventHandler(this.button4_Click);
        this.Controls.Add(this.button4);
        
        // Progress Bar
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.SuspendLayout();
        this.progressBar1.Location = new System.Drawing.Point(100, 240);
        this.progressBar1.Size = new System.Drawing.Size(580, 60);
        this.Controls.Add(this.progressBar1);

        //  button 5 (DISCONNECT)
        this.button5 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button5.Text = "DISCONNECT";
        this.button5.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button5.Location = new System.Drawing.Point(400, 320);
        this.button5.Size = new System.Drawing.Size(280, 100);
        this.button5.Click += new System.EventHandler(this.button5_Click);
        this.Controls.Add(this.button5);

        // Panel 1 (LEFT UP)
        this.panelSection1 = new RoundedPanel ();
        this.panelSection1.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection1.Location = new System.Drawing.Point(20, 20); // Position of the first panel
        this.panelSection1.Name = "panelSection1";
        this.panelSection1.Size = new System.Drawing.Size(740 , 540); // Size of the first panel
        this.panelSection1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection1, "CONNECTION", 10, 10); // Label settings
        
        // Textbox Baudrate
        this.textBox11 = new TextBox();
        this.textBox11.Location = new Point(378, 158);
        this.textBox11.Size = new Size(280, 40);
        this.textBox11.ReadOnly = true;
        this.textBox11.Text = "19200"; 
        this.textBox11.Enabled = false;
        this.panelSection1.Controls.Add(this.textBox11);

        // Label 1
        Label label1 = new Label();
        label1.Text = "COM PORT";
        label1.ForeColor = Color.Black; // Text color
        label1.BackColor = Color.Transparent; // Set the background to transparent
        label1.Location = new Point(75, 120); // Set the location
        label1.AutoSize = true; // Automatically adjusts the size to the text
        label1.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection1.Controls.Add(label1);
        // Label 2
        Label label2 = new Label();
        label2.Text = "BAUDRATE";
        label2.ForeColor = Color.Black; // Text color
        label2.BackColor = Color.Transparent; // Set the background to transparent
        label2.Location = new Point(375, 120); // Set the location
        label2.AutoSize = true; // Automatically adjusts the size to the text
        label2.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection1.Controls.Add(label2);

        // Panel 2 (LEFT MIDDLE)
        this.panelSection2 = new RoundedPanel ();
        this.panelSection2.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection2.Location = new System.Drawing.Point(20, 580); // Position of the first panel
        this.panelSection2.Name = "panelSection2";
        this.panelSection2.Size = new System.Drawing.Size(740, 540); // Size of the first panel
        this.panelSection2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection2, "CURRENT INFORMATION", 10, 10); // Label settings
        // Label 3
        Label label3 = new Label();
        label3.Text = "MOVEMENT RANGE";
        label3.ForeColor = Color.Black; // Text color
        label3.BackColor = Color.Transparent; // Set the background to transparent
        label3.Location = new Point(70, 100); // Set the location
        label3.AutoSize = true; // Automatically adjusts the size to the text
        label3.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection2.Controls.Add(label3);
        // Label 4
        Label label4 = new Label();
        label4.Text = "CURRENT POSITION";
        label4.ForeColor = Color.Black; // Text color
        label4.BackColor = Color.Transparent; // Set the background to transparent
        label4.Location = new Point(375, 100); // Set the location
        label4.AutoSize = true; // Automatically adjusts the size to the text
        label4.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection2.Controls.Add(label4);

        // Textbox 1
        TextBox textBox1 = new TextBox();
        textBox1.Location = new Point(75, 200);
        textBox1.Size = new Size(280, 40);
        textBox1.ReadOnly = true; // Only for display not to write
        textBox1.Text = "-8767 steps / -324 mm"; 
        textBox1.Enabled = false;
        this.panelSection2.Controls.Add(textBox1);
        // Label for Textbox 1
        Label label6 = new Label();
        label6.Text = "RANGE X";
        label6.ForeColor = Color.Black; // Text color
        label6.BackColor = Color.Transparent; // Set the background to transparent
        label6.Location = new Point(70, 160); // Set the location
        label6.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label6);

        // Textbox 2
        TextBox textBox2 = new TextBox();
        textBox2.Location = new Point(75, 300);
        textBox2.Size = new Size(280, 40);
        textBox2.ReadOnly = true; // Only for display not to write
        textBox2.Text = "-5166 steps / -247,77 mm"; 
        textBox2.Enabled = false;
        this.panelSection2.Controls.Add(textBox2);
        // Label for Textbox 2
        Label label7 = new Label();
        label7.Text = "RANGE Y";
        label7.ForeColor = Color.Black; // Text color
        label7.BackColor = Color.Transparent; // Set the background to transparent
        label7.Location = new Point(70, 260); // Set the location
        label7.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label7);

        // Textbox 3
        TextBox textBox3 = new TextBox();
        textBox3.Location = new Point(75, 400);
        textBox3.Size = new Size(280, 40);
        textBox3.ReadOnly = true; // Only for display not to write
        textBox3.Text = "11000 steps / 55,27 mm"; 
        textBox3.Enabled = false;
        this.panelSection2.Controls.Add(textBox3);
        // Label for Textbox 3
        Label label8 = new Label();
        label8.Text = "RANGE Z";
        label8.ForeColor = Color.Black; // Text color
        label8.BackColor = Color.Transparent; // Set the background to transparent
        label8.Location = new Point(70, 360); // Set the location
        label8.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label8);

        // Textbox 4
        this.textBox4 = new TextBox();
        this.textBox4.Location = new Point(380, 200);
        this.textBox4.Size = new Size(280, 40);
        this.textBox4.ReadOnly = true; // Only for display not to write
        this.panelSection2.Controls.Add(this.textBox4);
        // Label for Textbox 4
        Label label9 = new Label();
        label9.Text = "POSITION X";
        label9.ForeColor = Color.Black; // Text color
        label9.BackColor = Color.Transparent; // Set the background to transparent
        label9.Location = new Point(375, 160); // Set the location
        label9.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label9);

        // Textbox 5
        this.textBox5 = new TextBox();
        this.textBox5.Location = new Point(380, 300);
        this.textBox5.Size = new Size(280, 40);
        this.textBox5.ReadOnly = true; // Only for display not to write
        this.panelSection2.Controls.Add(this.textBox5);
        // Label for Textbox 5
        Label label10 = new Label();
        label10.Text = "POSITION Y";
        label10.ForeColor = Color.Black; // Text color
        label10.BackColor = Color.Transparent; // Set the background to transparent
        label10.Location = new Point(375, 260); // Set the location
        label10.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label10);

        // Textbox 6
        this.textBox6 = new TextBox();
        this.textBox6.Location = new Point(380, 400);
        this.textBox6.Size = new Size(280, 40);
        this.textBox6.ReadOnly = true; // Only for display not to write
        this.panelSection2.Controls.Add(this.textBox6);
        // Label for Textbox 6
        Label label11 = new Label();
        label11.Text = "POSITION Z";
        label11.ForeColor = Color.Black; // Text color
        label11.BackColor = Color.Transparent; // Set the background to transparent
        label11.Location = new Point(375, 360); // Set the location
        label11.AutoSize = true; // Automatically adjusts the size to the text
        this.panelSection2.Controls.Add(label11);

        // Panel 3 (MIDDLE)
        this.panelSection3 = new RoundedPanel ();
        this.panelSection3.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection3.Location = new System.Drawing.Point(780, 20); // Position of the first panel
        this.panelSection3.Name = "panelSection3";
        this.panelSection3.Size = new System.Drawing.Size(740, 1100); // Size of the first panel
        this.panelSection3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection3, "MESSAGES", 10, 10); // Label settings
        // Label 5
        Label label5 = new Label();
        label5.Text = "TERMINAL:";
        label5.ForeColor = Color.Black; // Text color
        label5.BackColor = Color.Transparent; // Set the background to transparent
        label5.Location = new Point(75, 80); // Set the location
        label5.AutoSize = true; // Automatically adjusts the size to the text
        label5.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection3.Controls.Add(label5);
        // Terminal
        this.terminalBox = new RichTextBox();
        terminalBox.ReadOnly = true;
        terminalBox.BackColor = Color.White;
        terminalBox.ForeColor = Color.Black;
        terminalBox.Font = new Font("Consolas", 10);
        terminalBox.Location = new Point(75, 120);
        terminalBox.Size = new Size(590, 800);
        this.panelSection3.Controls.Add(terminalBox);
        //  button 6 (RESET TERMINAL)
        this.button6 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button6.Text = "RESET TERMINAL";
        this.button6.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button6.Location = new System.Drawing.Point(857, 960);
        this.button6.Size = new System.Drawing.Size(285, 100);// earlier 590
        this.button6.Click += new System.EventHandler(this.button6_Click);
        this.Controls.Add(this.button6);
        //  button 6 (OPEN CAMERA VIEW)
        this.button22 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button22.Text = "OPEN CAM VIEW";
        this.button22.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button22.Location = new System.Drawing.Point(1162, 960);
        this.button22.Size = new System.Drawing.Size(285, 100);// earlier 590
        this.button22.Click += new System.EventHandler(this.button22_Click);
        this.Controls.Add(this.button22);

        // Panel 5 (LEFT DOWN)
        this.panelSection5 = new RoundedPanel();
        this.panelSection5.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection5.Location = new System.Drawing.Point(20, 1140); // Position of the first panel
        this.panelSection5.Name = "panelSection5";
        this.panelSection5.Size = new System.Drawing.Size(1500, 700); // Size of the first panel, before it was 1820
        this.panelSection5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection5, "CSV WINDOW", 10, 10); // Label settings
        // Label 21
        // Label label21 = new Label();
        // label21.Text = "CSV Options:";
        // label21.ForeColor = Color.Black; // Text color
        // label21.BackColor = Color.Transparent; // Set the background to transparent
        // label21.Location = new Point(75, 100); // Set the location
        // label21.AutoSize = true; // Automatically adjusts the size to the text
        // label21.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        // this.panelSection5.Controls.Add(label21);
        //  button 12 (OPEN CSV FILE)
        this.button12 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button12.Text = "LOAD CSV FILE";
        this.button12.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button12.Location = new System.Drawing.Point(100, 1290);
        this.button12.Size = new System.Drawing.Size(275, 153);
        this.button12.Click += new System.EventHandler(this.button12_Click); //change
        this.Controls.Add(this.button12);
        //  button 13 (LOAD CSV FILE)
        this.button13 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button13.Text = "START PROCEDURE";
        this.button13.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button13.Location = new System.Drawing.Point(100, 1463);
        this.button13.Size = new System.Drawing.Size(275, 153);
        this.button13.Click += new System.EventHandler(this.button13_Click); //change
        this.Controls.Add(this.button13);
        //  button 14 (START PROCEDURE)
        this.button14 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button14.Text = "ABORT";
        this.button14.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button14.Location = new System.Drawing.Point(100, 1636);
        this.button14.Size = new System.Drawing.Size(275, 153);
        this.button14.Click += new System.EventHandler(this.button14_Click); //change
        this.Controls.Add(this.button14);
        // Label 20
        Label label20 = new Label();
        label20.Text = "CSV FILE DISPLAY:";
        label20.ForeColor = Color.Black; // Text color
        label20.BackColor = Color.Transparent; // Set the background to transparent
        label20.Location = new Point(375, 100); // Set the location
        label20.AutoSize = true; // Automatically adjusts the size to the text
        label20.Font = new Font(label1.Font, FontStyle.Bold); // Make text bold
        this.panelSection5.Controls.Add(label20);
        // Terminal
        this.CSVBox = new RichTextBox();
        CSVBox.ReadOnly = true;
        CSVBox.BackColor = Color.White;
        CSVBox.ForeColor = Color.Black;
        CSVBox.Font = new Font("Consolas", 10);
        CSVBox.Location = new Point(375, 145);
        CSVBox.Size = new Size(1050, 500);
        this.panelSection5.Controls.Add(CSVBox);

        // Panel 4 (RIGHT UP)
        this.panelSection4 = new RoundedPanel();
        this.panelSection4.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection4.Location = new System.Drawing.Point(1540, 20); // Position of the first panel
        this.panelSection4.Name = "panelSection4";
        this.panelSection4.Size = new System.Drawing.Size(1440, 540); // Size of the first panel, before it was 1820
        this.panelSection4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection4, "POSITION CONTROLS", 10, 10); // Label settings
        // Textbox 7 (for X)
        this.textBox7 = new TextBox();
        textBox7.Location = new Point(75, 160);
        textBox7.Size = new Size(310, 40);
        textBox7.ReadOnly = false; 
        this.panelSection4.Controls.Add(textBox7);
        // Label 12
        Label label12 = new Label();
        label12.Text = "SET POSITION X:";
        label12.ForeColor = Color.Black; // Text color
        label12.BackColor = Color.Transparent; // Set the background to transparent
        label12.Location = new Point(75, 120); // Set the location
        label12.AutoSize = true; // Automatically adjusts the size to the text
        label12.Font = new Font(label12.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label12);
        // Textbox 8 (for Y)
        this.textBox8 = new TextBox();
        textBox8.Location = new Point(404, 160);
        textBox8.Size = new Size(310, 40);
        textBox8.ReadOnly = false; 
        this.panelSection4.Controls.Add(textBox8);
        // Label 13
        Label label13 = new Label();
        label13.Text = "SET POSITION Y:";
        label13.ForeColor = Color.Black; // Text color
        label13.BackColor = Color.Transparent; // Set the background to transparent
        label13.Location = new Point(404, 120); // Set the location
        label13.AutoSize = true; // Automatically adjusts the size to the text
        label13.Font = new Font(label3.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label13);
        // Textbox 9 (for Z)
        this.textBox9 = new TextBox();
        textBox9.Location = new Point(734, 160);
        textBox9.Size = new Size(310, 40); // 430*3 + 150 = 1440
        textBox9.ReadOnly = false;
        this.panelSection4.Controls.Add(textBox9);
        // Label 14
        Label label14 = new Label();
        label14.Text = "SET POSITION Z:";
        label14.ForeColor = Color.Black; // Text color
        label14.BackColor = Color.Transparent; // Set the background to transparent
        label14.Location = new Point(734, 120); // Set the location
        label14.AutoSize = true; // Automatically adjusts the size to the text
        label14.Font = new Font(label14.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label14);
        // Textbox 10 (for rotation)
        this.textBox10 = new TextBox();
        textBox10.Location = new Point(1064, 160);
        textBox10.Size = new Size(310, 40); // 430*3 + 150 = 1440
        textBox10.ReadOnly = false;
        this.panelSection4.Controls.Add(textBox10);
        // Label 14
        Label label19 = new Label();
        label19.Text = "SET ROTATION:";
        label19.ForeColor = Color.Black; // Text color
        label19.BackColor = Color.Transparent; // Set the background to transparent
        label19.Location = new Point(1064, 120); // Set the location
        label19.AutoSize = true; // Automatically adjusts the size to the text
        label19.Font = new Font(label19.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label19);

        // Label 21
        Label label21 = new Label();
        label21.Text = "XYZ UNIT:";
        label21.ForeColor = Color.Black; // Text color
        label21.BackColor = Color.Transparent; // Set the background to transparent
        label21.Location = new Point(70, 410); // Set the location
        label21.AutoSize = true; // Automatically adjusts the size to the text
        label21.Font = new Font(label21.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label21);

        // Label 22
        Label label22 = new Label();
        label22.Text = "R UNIT:";
        label22.ForeColor = Color.Black; // Text color
        label22.BackColor = Color.Transparent; // Set the background to transparent
        label22.Location = new Point(1064, 410); // Set the location
        label22.AutoSize = true; // Automatically adjusts the size to the text
        label22.Font = new Font(label22.Font, FontStyle.Bold); // Make text bold
        this.panelSection4.Controls.Add(label22);

        // Progress Bar 2
        this.progressBar2 = new System.Windows.Forms.ProgressBar();
        this.SuspendLayout();
        this.progressBar2.Location = new System.Drawing.Point(1615, 240);
        this.progressBar2.Size = new System.Drawing.Size(1302, 60);
        this.Controls.Add(this.progressBar2);

        //  button 7 (SET POSITION)
        this.button7 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button7.Text = "SEND";
        this.button7.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button7.Location = new System.Drawing.Point(1615, 320);
        this.button7.Size = new System.Drawing.Size(1302, 100);
        this.button7.Click += new System.EventHandler(this.button7_Click);
        this.Controls.Add(this.button7);

        //  button 23 (RECALIBRATE) 
        this.button23 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button23.Text = "CALIBRATION WINDOW";
        this.button23.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button23.Location = new System.Drawing.Point(2120, 440);
        this.button23.Size = new System.Drawing.Size(285, 100);
        this.button23.Click += new System.EventHandler(this.button23_Click);
        this.Controls.Add(this.button23);

        // Panel 6 (RIGHT MIDDLE)
        this.panelSection6 = new RoundedPanel();
        this.panelSection6.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection6.Location = new System.Drawing.Point(1540, 580); // Position of the first panel
        this.panelSection6.Name = "panelSection6";
        this.panelSection6.Size = new System.Drawing.Size(440, 540); // Size of the first panel, before it was 710
        this.panelSection6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection6, "SETTING CONTROLS", 10, 10); // Label settings
        // Label 15
        Label label15 = new Label();
        label15.Text = "PROCEDURE BUTTONS:";
        label15.ForeColor = Color.Black; // Text color
        label15.BackColor = Color.Transparent; // Set the background to transparent
        label15.Location = new Point(70, 100); // Set the location
        label15.AutoSize = true; // Automatically adjusts the size to the text
        label15.Font = new Font(label15.Font, FontStyle.Bold); // Make text bold
        this.panelSection6.Controls.Add(label15);
        //  button 8 (TEACH PICK)
        this.button8 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button8.Text = "SETPICK";
        this.button8.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        this.button8.Location = new System.Drawing.Point(1615, 720);
        this.button8.Size = new System.Drawing.Size(280, 160); // 580 before
        this.button8.Click += new System.EventHandler(this.button8_Click); //change
        this.Controls.Add(this.button8);
        //  button 9 (PICK)
        this.button9 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button9.Text = "SETPCB";
        this.button9.Font = new Font("Segoe UI", 12, FontStyle.Regular);
        //this.button9.Font = new System.Drawing.Font("Arial", 14);  // Set font family and size
        this.button9.Location = new System.Drawing.Point(1615, 900);
        this.button9.Size = new System.Drawing.Size(280, 160); // 635 before
        this.button9.Click += new System.EventHandler(this.button9_Click); //change
        this.Controls.Add(this.button9);

        this.panelSection8 = new RoundedPanel();
        this.panelSection8.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection8.Location = new System.Drawing.Point(2000, 580); // Position of the first panel
        this.panelSection8.Name = "panelSection8";
        this.panelSection8.Size = new System.Drawing.Size(980, 540); // Size of the first panel, before it was 710
        this.panelSection8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection8, "ARROW CONTROLS", 10, 10); // Label settings

        // Label 15
        Label label16 = new Label();
        label16.Text = "Z-AXIS:";
        label16.ForeColor = Color.Black; // Text color
        label16.BackColor = Color.Transparent; // Set the background to transparent
        label16.Location = new Point(120, 100); // Set the location
        label16.AutoSize = true; // Automatically adjusts the size to the text
        label16.Font = new Font(label16.Font, FontStyle.Bold); // Make text bold
        this.panelSection8.Controls.Add(label16);

        //  button 16 (ARROW UP)
        this.button16 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button16.Text = "↑";
        this.button16.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button16.Location = new System.Drawing.Point(2500, 720);
        this.button16.Size = new System.Drawing.Size(250, 160); // 635 before
        this.button16.Click += new System.EventHandler(this.button16_Click); //change
        this.Controls.Add(this.button16);
        //  button 17 (ARROW DOWN)
        this.button17 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button17.Text = "↓";
        this.button17.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button17.Location = new System.Drawing.Point(2500, 900);
        this.button17.Size = new System.Drawing.Size(250, 160); // 635 before
        this.button17.Click += new System.EventHandler(this.button17_Click); //change
        this.Controls.Add(this.button17);
        //  button 18 (ARROW RIGHT)
        this.button18 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button18.Text = "→";
        this.button18.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button18.Location = new System.Drawing.Point(2770, 720);
        this.button18.Size = new System.Drawing.Size(140, 340); // 635 before
        this.button18.Click += new System.EventHandler(this.button18_Click); //change
        this.Controls.Add(this.button18);
        //  button 19 (ARROW LEFT)
        this.button19 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button19.Text = "←";
        this.button19.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button19.Location = new System.Drawing.Point(2340, 720);
        this.button19.Size = new System.Drawing.Size(140, 340); // 635 before
        this.button19.Click += new System.EventHandler(this.button19_Click); //change
        this.Controls.Add(this.button19);

        //  button 20 (Z UP)
        this.button20 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button20.Text = "↑";
        this.button20.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button20.Location = new System.Drawing.Point(2050, 720);
        this.button20.Size = new System.Drawing.Size(250, 160); // 635 before
        this.button20.Click += new System.EventHandler(this.button21_Click); //change
        this.Controls.Add(this.button20);
        //  button 21 (Z DOWN)
        this.button21 = new System.Windows.Forms.Button();
        this.SuspendLayout();
        this.button21.Text = "↓";
        this.button21.Font = new Font("Segoe UI", 36, FontStyle.Regular);
        this.button21.Location = new System.Drawing.Point(2050, 900);
        this.button21.Size = new System.Drawing.Size(250, 160); // 635 before
        this.button21.Click += new System.EventHandler(this.button20_Click); //change
        this.Controls.Add(this.button21);

        //Textbox Unit Choice
        this.textBox12 = new TextBox();
        textBox12.Location = new Point(627, 88); // 357 b4
        textBox12.Size = new Size(280, 40);
        textBox12.ReadOnly = false; 
        textBox12.KeyDown += textBox12_KeyDown;
        this.panelSection8.Controls.Add(textBox12);

        // Panel 7 (RIGHT DOWN)
        this.panelSection7 = new RoundedPanel();
        this.panelSection7.BackColor = System.Drawing.Color.Gainsboro;
        this.panelSection7.Location = new System.Drawing.Point(1540, 1140); // Position of the first panel
        this.panelSection7.Name = "panelSection7";
        this.panelSection7.Size = new System.Drawing.Size(1440, 700); // Size of the first panel, before it was 1440
        this.panelSection7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        AddLabelToPanel(this.panelSection7, "MAIN CONTROLS", 10, 10); // Label settings
        // Label 16
        // Label label16 = new Label();
        // label16.Text = "Home:";
        // label16.ForeColor = Color.Black; // Text color
        // label16.BackColor = Color.Transparent; // Set the background to transparent
        // label16.Location = new Point(70, 100); // Set the location
        // label16.AutoSize = true; // Automatically adjusts the size to the text
        // label16.Font = new Font(label16.Font, FontStyle.Bold); // Make text bold
        // this.panelSection7.Controls.Add(label16);
        // Label 17
        // Label label17 = new Label();
        // label17.Text = "Stop:";
        // label17.ForeColor = Color.Black; // Text color
        // label17.BackColor = Color.Transparent; // Set the background to transparent
        // label17.Location = new Point(725, 100); // Set the location
        // label17.AutoSize = true; // Automatically adjusts the size to the text
        // label17.Font = new Font(label17.Font, FontStyle.Bold); // Make text bold
        // this.panelSection7.Controls.Add(label17);
        // Label 22
        // Label label22 = new Label();
        // label22.Text = "Reset:";
        // label22.ForeColor = Color.Black; // Text color
        // label22.BackColor = Color.Transparent; // Set the background to transparent
        // label22.Location = new Point(70, 380); // Set the location
        // label22.AutoSize = true; // Automatically adjusts the size to the text
        // label22.Font = new Font(label22.Font, FontStyle.Bold); // Make text bold
        // this.panelSection7.Controls.Add(label22);

        //this.ClientSize = new System.Drawing.Size(640, 450);     
        // Progress bar
        this.Controls.Add(this.progressBar1);
        this.Controls.Add(this.progressBar2);
        // Comboboxes
        this.Controls.Add(this.comboBoxCOMPorts);
        //this.Controls.Add(this.comboBoxBaudRates);
        this.Controls.Add(this.comboBoxUnitChoice);
        this.Controls.Add(this.comboBoxUnitChoice2);
        this.Controls.Add(this.comboBoxUnitChoice3);
        // Panels
        this.Controls.Add(this.panelSection1);
        this.Controls.Add(this.panelSection2);
        this.Controls.Add(this.panelSection3);
        this.Controls.Add(this.panelSection4);
        this.Controls.Add(this.panelSection5);
        this.Controls.Add(this.panelSection6);
        this.Controls.Add(this.panelSection7);
        this.Controls.Add(this.panelSection8);
        this.Controls.Add(this.checkBoxDarkMode);
        this.Name = "YourForm";
        this.Text = "Multi-section Form Example";
        this.ResumeLayout(false);
        
        ////////////////////////////////////////////////////////////////////////
        this.components = new System.ComponentModel.Container(); // default
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font; // default
        this.ClientSize = new System.Drawing.Size(2560 , 1440); // default
        this.Text = "Pick and Place Machine Desktop Application"; // default with changed text
    }

    #endregion
}
