namespace QuizArena.Cliente;

partial class PartidaForm
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    private void InitializeComponent()
    {
        lblFase = new Label();
        lblTiempo = new Label();
        lblPregunta = new Label();
        btnA = new Button();
        btnB = new Button();
        btnC = new Button();
        btnD = new Button();
        lblInfo = new Label();
        txtMarcador = new TextBox();
        btnCerrarPartida = new Button();
        SuspendLayout();
        //
        // lblFase
        //
        lblFase.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblFase.Location = new Point(25, 20);
        lblFase.Name = "lblFase";
        lblFase.Size = new Size(570, 30);
        lblFase.TabIndex = 0;
        lblFase.Text = "Esperando partida";
        lblFase.AutoSize = false;
        //
        // lblTiempo
        //
        lblTiempo.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTiempo.Location = new Point(640, 20);
        lblTiempo.Name = "lblTiempo";
        lblTiempo.Size = new Size(125, 30);
        lblTiempo.TabIndex = 1;
        lblTiempo.Text = "--:--";
        lblTiempo.AutoSize = false;
        lblTiempo.TextAlign = ContentAlignment.MiddleRight;
        //
        // lblPregunta
        //
        lblPregunta.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblPregunta.Location = new Point(25, 75);
        lblPregunta.Name = "lblPregunta";
        lblPregunta.Size = new Size(740, 80);
        lblPregunta.TabIndex = 2;
        lblPregunta.Text = "Pregunta pendiente";
        lblPregunta.AutoSize = false;
        //
        // btnA
        //
        btnA.Location = new Point(25, 175);
        btnA.Name = "btnA";
        btnA.Size = new Size(355, 48);
        btnA.TabIndex = 3;
        btnA.Text = "Opción A";
        btnA.UseVisualStyleBackColor = true;
        btnA.Enabled = false;
        //
        // btnB
        //
        btnB.Location = new Point(395, 175);
        btnB.Name = "btnB";
        btnB.Size = new Size(355, 48);
        btnB.TabIndex = 4;
        btnB.Text = "Opción B";
        btnB.UseVisualStyleBackColor = true;
        btnB.Enabled = false;
        //
        // btnC
        //
        btnC.Location = new Point(25, 235);
        btnC.Name = "btnC";
        btnC.Size = new Size(355, 48);
        btnC.TabIndex = 5;
        btnC.Text = "Opción C";
        btnC.UseVisualStyleBackColor = true;
        btnC.Enabled = false;
        //
        // btnD
        //
        btnD.Location = new Point(395, 235);
        btnD.Name = "btnD";
        btnD.Size = new Size(355, 48);
        btnD.TabIndex = 6;
        btnD.Text = "Opción D";
        btnD.UseVisualStyleBackColor = true;
        btnD.Enabled = false;
        //
        // lblInfo
        //
        lblInfo.Location = new Point(25, 300);
        lblInfo.Name = "lblInfo";
        lblInfo.Size = new Size(740, 50);
        lblInfo.TabIndex = 7;
        lblInfo.Text = "Esperando datos";
        lblInfo.AutoSize = false;
        //
        // txtMarcador
        //
        txtMarcador.Font = new Font("Segoe UI", 10F);
        txtMarcador.Multiline = true;
        txtMarcador.Location = new Point(25, 365);
        txtMarcador.Name = "txtMarcador";
        txtMarcador.Size = new Size(740, 135);
        txtMarcador.TabIndex = 8;
        txtMarcador.ReadOnly = true;
        txtMarcador.ScrollBars = ScrollBars.Vertical;
        //
        // btnCerrarPartida
        //
        btnCerrarPartida.Location = new Point(290, 520);
        btnCerrarPartida.Name = "btnCerrarPartida";
        btnCerrarPartida.Size = new Size(220, 38);
        btnCerrarPartida.TabIndex = 9;
        btnCerrarPartida.Text = "Volver a la sala";
        btnCerrarPartida.UseVisualStyleBackColor = true;
        Controls.Add(lblFase);
        Controls.Add(lblTiempo);
        Controls.Add(lblPregunta);
        Controls.Add(btnA);
        Controls.Add(btnB);
        Controls.Add(btnC);
        Controls.Add(btnD);
        Controls.Add(lblInfo);
        Controls.Add(txtMarcador);
        Controls.Add(btnCerrarPartida);
        AutoScaleDimensions = new SizeF(8F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 580);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "PartidaForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quiz Arena - Partida";
        ResumeLayout(false);
        PerformLayout();
    }
    #endregion

    private Label lblFase;
    private Label lblTiempo;
    private Label lblPregunta;
    private Button btnA;
    private Button btnB;
    private Button btnC;
    private Button btnD;
    private Label lblInfo;
    private TextBox txtMarcador;
    private Button btnCerrarPartida;
}
