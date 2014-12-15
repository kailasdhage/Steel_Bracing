namespace Steel_Bracing_2d
{
    partial class FrmBracing
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBracing));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.gpAngle = new System.Windows.Forms.GroupBox();
            this.cmbPipe = new System.Windows.Forms.ComboBox();
            this.cmbChannel = new System.Windows.Forms.ComboBox();
            this.lblStarPlateOffset = new System.Windows.Forms.Label();
            this.txtStarPlateOffset = new System.Windows.Forms.TextBox();
            this.lblStartPlateWidth = new System.Windows.Forms.Label();
            this.txtStarPlateWidth = new System.Windows.Forms.TextBox();
            this.lblStarPlateThk = new System.Windows.Forms.Label();
            this.txtStarPlateThk = new System.Windows.Forms.TextBox();
            this.lblSteelSection = new System.Windows.Forms.Label();
            this.cmbAngle = new System.Windows.Forms.ComboBox();
            this.lblCGOffset = new System.Windows.Forms.Label();
            this.txtCGOffset = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkOblongHole = new System.Windows.Forms.CheckBox();
            this.txtOblongCenterOffset = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPitch = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtNoOfHoles = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtHoleDia = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtYOffset = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtXOffset = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkAddLengthDimensions = new System.Windows.Forms.CheckBox();
            this.lblType2 = new System.Windows.Forms.Label();
            this.txtCenterPlateOffset = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtPlateOffset = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEdgeOffset = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOverlap = new System.Windows.Forms.TextBox();
            this.radType1 = new System.Windows.Forms.RadioButton();
            this.radType2 = new System.Windows.Forms.RadioButton();
            this.radType3 = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radPipe = new System.Windows.Forms.RadioButton();
            this.radBackToBackChannel = new System.Windows.Forms.RadioButton();
            this.radBoxChannel = new System.Windows.Forms.RadioButton();
            this.radBackToBackAngle = new System.Windows.Forms.RadioButton();
            this.radStarAngle = new System.Windows.Forms.RadioButton();
            this.radSimpleAngle = new System.Windows.Forms.RadioButton();
            this.radType4 = new System.Windows.Forms.RadioButton();
            this.picType4 = new System.Windows.Forms.PictureBox();
            this.picType3 = new System.Windows.Forms.PictureBox();
            this.picType2 = new System.Windows.Forms.PictureBox();
            this.picType1 = new System.Windows.Forms.PictureBox();
            this.gpAngle.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picType4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(699, 461);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(116, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(574, 462);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(116, 30);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // gpAngle
            // 
            this.gpAngle.Controls.Add(this.cmbPipe);
            this.gpAngle.Controls.Add(this.cmbChannel);
            this.gpAngle.Controls.Add(this.lblStarPlateOffset);
            this.gpAngle.Controls.Add(this.txtStarPlateOffset);
            this.gpAngle.Controls.Add(this.lblStartPlateWidth);
            this.gpAngle.Controls.Add(this.txtStarPlateWidth);
            this.gpAngle.Controls.Add(this.lblStarPlateThk);
            this.gpAngle.Controls.Add(this.txtStarPlateThk);
            this.gpAngle.Controls.Add(this.lblSteelSection);
            this.gpAngle.Controls.Add(this.cmbAngle);
            this.gpAngle.Controls.Add(this.lblCGOffset);
            this.gpAngle.Controls.Add(this.txtCGOffset);
            this.gpAngle.Location = new System.Drawing.Point(13, 87);
            this.gpAngle.Name = "gpAngle";
            this.gpAngle.Size = new System.Drawing.Size(382, 114);
            this.gpAngle.TabIndex = 1;
            this.gpAngle.TabStop = false;
            this.gpAngle.Text = "Steel Angle";
            // 
            // cmbPipe
            // 
            this.cmbPipe.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPipe.FormattingEnabled = true;
            this.cmbPipe.Location = new System.Drawing.Point(80, 19);
            this.cmbPipe.Name = "cmbPipe";
            this.cmbPipe.Size = new System.Drawing.Size(280, 25);
            this.cmbPipe.TabIndex = 10;
            this.cmbPipe.Visible = false;
            // 
            // cmbChannel
            // 
            this.cmbChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChannel.FormattingEnabled = true;
            this.cmbChannel.Location = new System.Drawing.Point(166, 19);
            this.cmbChannel.Name = "cmbChannel";
            this.cmbChannel.Size = new System.Drawing.Size(115, 25);
            this.cmbChannel.TabIndex = 2;
            this.cmbChannel.Visible = false;
            this.cmbChannel.SelectedIndexChanged += new System.EventHandler(this.cmbChannel_SelectedIndexChanged);
            // 
            // lblStarPlateOffset
            // 
            this.lblStarPlateOffset.AutoSize = true;
            this.lblStarPlateOffset.Location = new System.Drawing.Point(212, 88);
            this.lblStarPlateOffset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStarPlateOffset.Name = "lblStarPlateOffset";
            this.lblStarPlateOffset.Size = new System.Drawing.Size(95, 17);
            this.lblStarPlateOffset.TabIndex = 8;
            this.lblStarPlateOffset.Text = "Plate Offset:";
            this.lblStarPlateOffset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtStarPlateOffset
            // 
            this.txtStarPlateOffset.Enabled = false;
            this.txtStarPlateOffset.Location = new System.Drawing.Point(312, 87);
            this.txtStarPlateOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtStarPlateOffset.Name = "txtStarPlateOffset";
            this.txtStarPlateOffset.Size = new System.Drawing.Size(48, 24);
            this.txtStarPlateOffset.TabIndex = 9;
            this.txtStarPlateOffset.Text = "600";
            // 
            // lblStartPlateWidth
            // 
            this.lblStartPlateWidth.AutoSize = true;
            this.lblStartPlateWidth.Location = new System.Drawing.Point(39, 86);
            this.lblStartPlateWidth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStartPlateWidth.Name = "lblStartPlateWidth";
            this.lblStartPlateWidth.Size = new System.Drawing.Size(94, 17);
            this.lblStartPlateWidth.TabIndex = 7;
            this.lblStartPlateWidth.Text = "Plate Width:";
            this.lblStartPlateWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtStarPlateWidth
            // 
            this.txtStarPlateWidth.Enabled = false;
            this.txtStarPlateWidth.Location = new System.Drawing.Point(156, 83);
            this.txtStarPlateWidth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtStarPlateWidth.Name = "txtStarPlateWidth";
            this.txtStarPlateWidth.Size = new System.Drawing.Size(47, 24);
            this.txtStarPlateWidth.TabIndex = 7;
            this.txtStarPlateWidth.Text = "75";
            // 
            // lblStarPlateThk
            // 
            this.lblStarPlateThk.AutoSize = true;
            this.lblStarPlateThk.Location = new System.Drawing.Point(223, 56);
            this.lblStarPlateThk.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStarPlateThk.Name = "lblStarPlateThk";
            this.lblStarPlateThk.Size = new System.Drawing.Size(78, 17);
            this.lblStarPlateThk.TabIndex = 5;
            this.lblStarPlateThk.Text = "Plate Thk:";
            this.lblStarPlateThk.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtStarPlateThk
            // 
            this.txtStarPlateThk.Enabled = false;
            this.txtStarPlateThk.Location = new System.Drawing.Point(312, 56);
            this.txtStarPlateThk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtStarPlateThk.Name = "txtStarPlateThk";
            this.txtStarPlateThk.Size = new System.Drawing.Size(48, 24);
            this.txtStarPlateThk.TabIndex = 6;
            this.txtStarPlateThk.Text = "8";
            // 
            // lblSteelSection
            // 
            this.lblSteelSection.AutoSize = true;
            this.lblSteelSection.Location = new System.Drawing.Point(5, 22);
            this.lblSteelSection.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSteelSection.Name = "lblSteelSection";
            this.lblSteelSection.Size = new System.Drawing.Size(53, 17);
            this.lblSteelSection.TabIndex = 0;
            this.lblSteelSection.Text = "Angle:";
            this.lblSteelSection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbAngle
            // 
            this.cmbAngle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAngle.FormattingEnabled = true;
            this.cmbAngle.Location = new System.Drawing.Point(80, 19);
            this.cmbAngle.Name = "cmbAngle";
            this.cmbAngle.Size = new System.Drawing.Size(280, 25);
            this.cmbAngle.TabIndex = 1;
            this.cmbAngle.SelectedIndexChanged += new System.EventHandler(this.cmbAngle_SelectedIndexChanged);
            // 
            // lblCGOffset
            // 
            this.lblCGOffset.AutoSize = true;
            this.lblCGOffset.Location = new System.Drawing.Point(4, 56);
            this.lblCGOffset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCGOffset.Name = "lblCGOffset";
            this.lblCGOffset.Size = new System.Drawing.Size(146, 17);
            this.lblCGOffset.TabIndex = 3;
            this.lblCGOffset.Text = "CG Back Off(300.5)";
            this.lblCGOffset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCGOffset
            // 
            this.txtCGOffset.Location = new System.Drawing.Point(156, 53);
            this.txtCGOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtCGOffset.Name = "txtCGOffset";
            this.txtCGOffset.Size = new System.Drawing.Size(47, 24);
            this.txtCGOffset.TabIndex = 4;
            this.txtCGOffset.Text = "20";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkOblongHole);
            this.groupBox1.Controls.Add(this.txtOblongCenterOffset);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.txtPitch);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.txtNoOfHoles);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtHoleDia);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtYOffset);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtXOffset);
            this.groupBox1.Location = new System.Drawing.Point(12, 211);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(383, 131);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hole Details";
            // 
            // chkOblongHole
            // 
            this.chkOblongHole.AutoSize = true;
            this.chkOblongHole.Checked = true;
            this.chkOblongHole.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOblongHole.Location = new System.Drawing.Point(179, 79);
            this.chkOblongHole.Name = "chkOblongHole";
            this.chkOblongHole.Size = new System.Drawing.Size(128, 21);
            this.chkOblongHole.TabIndex = 10;
            this.chkOblongHole.Text = "Oblong Offset";
            this.chkOblongHole.UseVisualStyleBackColor = true;
            this.chkOblongHole.CheckedChanged += new System.EventHandler(this.chkOblongHole_CheckedChanged);
            // 
            // txtOblongCenterOffset
            // 
            this.txtOblongCenterOffset.Location = new System.Drawing.Point(320, 76);
            this.txtOblongCenterOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtOblongCenterOffset.Name = "txtOblongCenterOffset";
            this.txtOblongCenterOffset.Size = new System.Drawing.Size(49, 24);
            this.txtOblongCenterOffset.TabIndex = 11;
            this.txtOblongCenterOffset.Text = "5";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(36, 85);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 17);
            this.label11.TabIndex = 4;
            this.label11.Text = "Pitch:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPitch
            // 
            this.txtPitch.Location = new System.Drawing.Point(100, 81);
            this.txtPitch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtPitch.Name = "txtPitch";
            this.txtPitch.Size = new System.Drawing.Size(53, 24);
            this.txtPitch.TabIndex = 5;
            this.txtPitch.Text = "50";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(209, 51);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(95, 17);
            this.label10.TabIndex = 8;
            this.label10.Text = "No of Holes:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtNoOfHoles
            // 
            this.txtNoOfHoles.Location = new System.Drawing.Point(320, 48);
            this.txtNoOfHoles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtNoOfHoles.Name = "txtNoOfHoles";
            this.txtNoOfHoles.Size = new System.Drawing.Size(49, 24);
            this.txtNoOfHoles.TabIndex = 9;
            this.txtNoOfHoles.Text = "2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 54);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Diameter:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHoleDia
            // 
            this.txtHoleDia.Location = new System.Drawing.Point(100, 51);
            this.txtHoleDia.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtHoleDia.Name = "txtHoleDia";
            this.txtHoleDia.Size = new System.Drawing.Size(53, 24);
            this.txtHoleDia.TabIndex = 3;
            this.txtHoleDia.Text = "22";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(212, 21);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Bolt Offset:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtYOffset
            // 
            this.txtYOffset.Location = new System.Drawing.Point(320, 18);
            this.txtYOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtYOffset.Name = "txtYOffset";
            this.txtYOffset.Size = new System.Drawing.Size(49, 24);
            this.txtYOffset.TabIndex = 7;
            this.txtYOffset.Text = "35";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 21);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "X-Offset:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtXOffset
            // 
            this.txtXOffset.Location = new System.Drawing.Point(100, 18);
            this.txtXOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtXOffset.Name = "txtXOffset";
            this.txtXOffset.Size = new System.Drawing.Size(53, 24);
            this.txtXOffset.TabIndex = 1;
            this.txtXOffset.Text = "50";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkAddLengthDimensions);
            this.groupBox2.Controls.Add(this.lblType2);
            this.groupBox2.Controls.Add(this.txtCenterPlateOffset);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.txtPlateOffset);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtEdgeOffset);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtOverlap);
            this.groupBox2.Location = new System.Drawing.Point(13, 362);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(382, 119);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Misc Settings";
            // 
            // chkAddLengthDimensions
            // 
            this.chkAddLengthDimensions.AutoSize = true;
            this.chkAddLengthDimensions.Checked = true;
            this.chkAddLengthDimensions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAddLengthDimensions.Location = new System.Drawing.Point(170, 89);
            this.chkAddLengthDimensions.Name = "chkAddLengthDimensions";
            this.chkAddLengthDimensions.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkAddLengthDimensions.Size = new System.Drawing.Size(198, 21);
            this.chkAddLengthDimensions.TabIndex = 8;
            this.chkAddLengthDimensions.Text = "Add Length Dimensions";
            this.chkAddLengthDimensions.UseVisualStyleBackColor = true;
            // 
            // lblType2
            // 
            this.lblType2.AutoSize = true;
            this.lblType2.Location = new System.Drawing.Point(155, 56);
            this.lblType2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblType2.Name = "lblType2";
            this.lblType2.Size = new System.Drawing.Size(167, 17);
            this.lblType2.TabIndex = 6;
            this.lblType2.Text = "Center Plate Champer:";
            this.lblType2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCenterPlateOffset
            // 
            this.txtCenterPlateOffset.Location = new System.Drawing.Point(325, 56);
            this.txtCenterPlateOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtCenterPlateOffset.Name = "txtCenterPlateOffset";
            this.txtCenterPlateOffset.Size = new System.Drawing.Size(44, 24);
            this.txtCenterPlateOffset.TabIndex = 7;
            this.txtCenterPlateOffset.Text = "50";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(195, 21);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(128, 17);
            this.label15.TabIndex = 4;
            this.label15.Text = "Plate Extend Off:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPlateOffset
            // 
            this.txtPlateOffset.Location = new System.Drawing.Point(328, 18);
            this.txtPlateOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtPlateOffset.Name = "txtPlateOffset";
            this.txtPlateOffset.Size = new System.Drawing.Size(40, 24);
            this.txtPlateOffset.TabIndex = 5;
            this.txtPlateOffset.Text = "20";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 18);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "Edge Offset:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtEdgeOffset
            // 
            this.txtEdgeOffset.Location = new System.Drawing.Point(113, 15);
            this.txtEdgeOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtEdgeOffset.Name = "txtEdgeOffset";
            this.txtEdgeOffset.Size = new System.Drawing.Size(40, 24);
            this.txtEdgeOffset.TabIndex = 1;
            this.txtEdgeOffset.Text = "20";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 57);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Weld Length:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtOverlap
            // 
            this.txtOverlap.Location = new System.Drawing.Point(114, 54);
            this.txtOverlap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtOverlap.Name = "txtOverlap";
            this.txtOverlap.Size = new System.Drawing.Size(40, 24);
            this.txtOverlap.TabIndex = 3;
            this.txtOverlap.Text = "250";
            // 
            // radType1
            // 
            this.radType1.AutoSize = true;
            this.radType1.Checked = true;
            this.radType1.Location = new System.Drawing.Point(444, 132);
            this.radType1.Name = "radType1";
            this.radType1.Size = new System.Drawing.Size(78, 21);
            this.radType1.TabIndex = 3;
            this.radType1.TabStop = true;
            this.radType1.Text = "Type-1";
            this.radType1.UseVisualStyleBackColor = true;
            this.radType1.CheckedChanged += new System.EventHandler(this.radType1_CheckedChanged);
            // 
            // radType2
            // 
            this.radType2.AutoSize = true;
            this.radType2.Location = new System.Drawing.Point(582, 132);
            this.radType2.Name = "radType2";
            this.radType2.Size = new System.Drawing.Size(78, 21);
            this.radType2.TabIndex = 4;
            this.radType2.Text = "Type-2";
            this.radType2.UseVisualStyleBackColor = true;
            this.radType2.CheckedChanged += new System.EventHandler(this.radType2_CheckedChanged);
            // 
            // radType3
            // 
            this.radType3.AutoSize = true;
            this.radType3.Location = new System.Drawing.Point(720, 132);
            this.radType3.Name = "radType3";
            this.radType3.Size = new System.Drawing.Size(78, 21);
            this.radType3.TabIndex = 5;
            this.radType3.Text = "Type-3";
            this.radType3.UseVisualStyleBackColor = true;
            this.radType3.CheckedChanged += new System.EventHandler(this.radType3_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radPipe);
            this.groupBox3.Controls.Add(this.radBackToBackChannel);
            this.groupBox3.Controls.Add(this.radBoxChannel);
            this.groupBox3.Controls.Add(this.radBackToBackAngle);
            this.groupBox3.Controls.Add(this.radStarAngle);
            this.groupBox3.Controls.Add(this.radSimpleAngle);
            this.groupBox3.Location = new System.Drawing.Point(12, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(383, 74);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Member Type";
            // 
            // radPipe
            // 
            this.radPipe.AutoSize = true;
            this.radPipe.Location = new System.Drawing.Point(270, 44);
            this.radPipe.Name = "radPipe";
            this.radPipe.Size = new System.Drawing.Size(57, 21);
            this.radPipe.TabIndex = 5;
            this.radPipe.Text = "Pipe";
            this.radPipe.UseVisualStyleBackColor = true;
            this.radPipe.CheckedChanged += new System.EventHandler(this.radPipe_CheckedChanged);
            // 
            // radBackToBackChannel
            // 
            this.radBackToBackChannel.AutoSize = true;
            this.radBackToBackChannel.Location = new System.Drawing.Point(139, 44);
            this.radBackToBackChannel.Name = "radBackToBackChannel";
            this.radBackToBackChannel.Size = new System.Drawing.Size(116, 21);
            this.radBackToBackChannel.TabIndex = 4;
            this.radBackToBackChannel.Text = "B/B Channel";
            this.radBackToBackChannel.UseVisualStyleBackColor = true;
            this.radBackToBackChannel.CheckedChanged += new System.EventHandler(this.radBackToBackChannel_CheckedChanged);
            // 
            // radBoxChannel
            // 
            this.radBoxChannel.AutoSize = true;
            this.radBoxChannel.Location = new System.Drawing.Point(6, 44);
            this.radBoxChannel.Name = "radBoxChannel";
            this.radBoxChannel.Size = new System.Drawing.Size(118, 21);
            this.radBoxChannel.TabIndex = 3;
            this.radBoxChannel.Text = "Box Channel";
            this.radBoxChannel.UseVisualStyleBackColor = true;
            this.radBoxChannel.CheckedChanged += new System.EventHandler(this.radBoxChannel_CheckedChanged);
            // 
            // radBackToBackAngle
            // 
            this.radBackToBackAngle.AutoSize = true;
            this.radBackToBackAngle.Location = new System.Drawing.Point(270, 20);
            this.radBackToBackAngle.Name = "radBackToBackAngle";
            this.radBackToBackAngle.Size = new System.Drawing.Size(99, 21);
            this.radBackToBackAngle.TabIndex = 2;
            this.radBackToBackAngle.Text = "B/B Angle";
            this.radBackToBackAngle.UseVisualStyleBackColor = true;
            this.radBackToBackAngle.CheckedChanged += new System.EventHandler(this.radBackToBackAngle_CheckedChanged);
            // 
            // radStarAngle
            // 
            this.radStarAngle.AutoSize = true;
            this.radStarAngle.Location = new System.Drawing.Point(139, 20);
            this.radStarAngle.Name = "radStarAngle";
            this.radStarAngle.Size = new System.Drawing.Size(103, 21);
            this.radStarAngle.TabIndex = 1;
            this.radStarAngle.Text = "Star Angle";
            this.radStarAngle.UseVisualStyleBackColor = true;
            this.radStarAngle.CheckedChanged += new System.EventHandler(this.radStarAngle_CheckedChanged);
            // 
            // radSimpleAngle
            // 
            this.radSimpleAngle.AutoSize = true;
            this.radSimpleAngle.Checked = true;
            this.radSimpleAngle.Location = new System.Drawing.Point(6, 20);
            this.radSimpleAngle.Name = "radSimpleAngle";
            this.radSimpleAngle.Size = new System.Drawing.Size(119, 21);
            this.radSimpleAngle.TabIndex = 0;
            this.radSimpleAngle.TabStop = true;
            this.radSimpleAngle.Text = "Simple Angle";
            this.radSimpleAngle.UseVisualStyleBackColor = true;
            this.radSimpleAngle.CheckedChanged += new System.EventHandler(this.radSimpleAngle_CheckedChanged);
            // 
            // radType4
            // 
            this.radType4.AutoSize = true;
            this.radType4.Location = new System.Drawing.Point(449, 296);
            this.radType4.Name = "radType4";
            this.radType4.Size = new System.Drawing.Size(78, 21);
            this.radType4.TabIndex = 6;
            this.radType4.Text = "Type-4";
            this.radType4.UseVisualStyleBackColor = true;
            this.radType4.CheckedChanged += new System.EventHandler(this.radType4_CheckedChanged);
            // 
            // picType4
            // 
            this.picType4.Image = ((System.Drawing.Image)(resources.GetObject("picType4.Image")));
            this.picType4.Location = new System.Drawing.Point(426, 180);
            this.picType4.Name = "picType4";
            this.picType4.Size = new System.Drawing.Size(111, 110);
            this.picType4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picType4.TabIndex = 21;
            this.picType4.TabStop = false;
            this.picType4.Click += new System.EventHandler(this.picType4_Click);
            // 
            // picType3
            // 
            this.picType3.Image = ((System.Drawing.Image)(resources.GetObject("picType3.Image")));
            this.picType3.Location = new System.Drawing.Point(697, 16);
            this.picType3.Name = "picType3";
            this.picType3.Size = new System.Drawing.Size(111, 110);
            this.picType3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picType3.TabIndex = 19;
            this.picType3.TabStop = false;
            this.picType3.Click += new System.EventHandler(this.picType3_Click);
            // 
            // picType2
            // 
            this.picType2.Image = ((System.Drawing.Image)(resources.GetObject("picType2.Image")));
            this.picType2.Location = new System.Drawing.Point(558, 16);
            this.picType2.Name = "picType2";
            this.picType2.Size = new System.Drawing.Size(111, 110);
            this.picType2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picType2.TabIndex = 18;
            this.picType2.TabStop = false;
            this.picType2.Click += new System.EventHandler(this.picType2_Click);
            // 
            // picType1
            // 
            this.picType1.Image = ((System.Drawing.Image)(resources.GetObject("picType1.Image")));
            this.picType1.Location = new System.Drawing.Point(426, 16);
            this.picType1.Name = "picType1";
            this.picType1.Size = new System.Drawing.Size(111, 110);
            this.picType1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picType1.TabIndex = 17;
            this.picType1.TabStop = false;
            this.picType1.Click += new System.EventHandler(this.picType1_Click);
            // 
            // FrmBracing
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(826, 503);
            this.Controls.Add(this.radType4);
            this.Controls.Add(this.picType4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.radType3);
            this.Controls.Add(this.picType3);
            this.Controls.Add(this.radType2);
            this.Controls.Add(this.radType1);
            this.Controls.Add(this.picType2);
            this.Controls.Add(this.picType1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gpAngle);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmBracing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Steel Bracing 2d";
            this.Load += new System.EventHandler(this.frmBracing_Load);
            this.gpAngle.ResumeLayout(false);
            this.gpAngle.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picType4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picType1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.GroupBox gpAngle;
        private System.Windows.Forms.Label lblCGOffset;
        private System.Windows.Forms.TextBox txtCGOffset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHoleDia;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtYOffset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtXOffset;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtEdgeOffset;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtOverlap;
        private System.Windows.Forms.ComboBox cmbAngle;
        private System.Windows.Forms.Label lblSteelSection;
        private System.Windows.Forms.PictureBox picType1;
        private System.Windows.Forms.PictureBox picType2;
        private System.Windows.Forms.RadioButton radType1;
        private System.Windows.Forms.RadioButton radType2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtNoOfHoles;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPitch;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtPlateOffset;
        private System.Windows.Forms.Label lblStarPlateThk;
        private System.Windows.Forms.TextBox txtStarPlateThk;
        private System.Windows.Forms.Label lblStartPlateWidth;
        private System.Windows.Forms.TextBox txtStarPlateWidth;
        private System.Windows.Forms.Label lblType2;
        private System.Windows.Forms.TextBox txtCenterPlateOffset;
        private System.Windows.Forms.Label lblStarPlateOffset;
        private System.Windows.Forms.TextBox txtStarPlateOffset;
        private System.Windows.Forms.PictureBox picType3;
        private System.Windows.Forms.RadioButton radType3;
        private System.Windows.Forms.CheckBox chkAddLengthDimensions;
        private System.Windows.Forms.CheckBox chkOblongHole;
        private System.Windows.Forms.TextBox txtOblongCenterOffset;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radBackToBackAngle;
        private System.Windows.Forms.RadioButton radStarAngle;
        private System.Windows.Forms.RadioButton radSimpleAngle;
        private System.Windows.Forms.RadioButton radBackToBackChannel;
        private System.Windows.Forms.RadioButton radBoxChannel;
        private System.Windows.Forms.ComboBox cmbChannel;
        private System.Windows.Forms.RadioButton radPipe;
        private System.Windows.Forms.ComboBox cmbPipe;
        private System.Windows.Forms.RadioButton radType4;
        private System.Windows.Forms.PictureBox picType4;
    }
}