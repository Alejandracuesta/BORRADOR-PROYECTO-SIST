namespace QuizArena.Cliente;
partial class ChatForm
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        SuspendLayout();
        lblAmbitoChat = new Label();
        lblAmbitoChat.Name = "lblAmbitoChat";
        lblAmbitoChat.Text = "Chat de sala";
        lblAmbitoChat.Location = new Point(20, 15);
        lblAmbitoChat.Size = new Size(550, 25);
        Controls.Add(lblAmbitoChat);
        lstMensajes = new ListBox();
        lstMensajes.Name = "lstMensajes";
        lstMensajes.Text = "";
        lstMensajes.Location = new Point(20, 50);
        lstMensajes.Size = new Size(550, 325);
        Controls.Add(lstMensajes);
        txtMensaje = new TextBox();
        txtMensaje.Name = "txtMensaje";
        txtMensaje.Text = "";
        txtMensaje.Location = new Point(20, 395);
        txtMensaje.Size = new Size(420, 28);
        Controls.Add(txtMensaje);
        btnEnviar = new Button();
        btnEnviar.Name = "btnEnviar";
        btnEnviar.Text = "Enviar";
        btnEnviar.Location = new Point(455, 395);
        btnEnviar.Size = new Size(115, 30);
        Controls.Add(btnEnviar);
        btnAplauso = new Button();
        btnAplauso.Name = "btnAplauso";
        btnAplauso.Text = "Aplauso";
        btnAplauso.Location = new Point(20, 445);
        btnAplauso.Size = new Size(120, 32);
        Controls.Add(btnAplauso);
        btnAnimo = new Button();
        btnAnimo.Name = "btnAnimo";
        btnAnimo.Text = "Ánimo";
        btnAnimo.Location = new Point(150, 445);
        btnAnimo.Size = new Size(120, 32);
        Controls.Add(btnAnimo);
        btnSorpresa = new Button();
        btnSorpresa.Name = "btnSorpresa";
        btnSorpresa.Text = "Sorpresa";
        btnSorpresa.Location = new Point(280, 445);
        btnSorpresa.Size = new Size(120, 32);
        Controls.Add(btnSorpresa);
        txtMensaje.MaxLength = 250;
        lstMensajes.HorizontalScrollbar = true;
        ClientSize = new Size(600, 500);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Text = "Quiz Arena - Chat";
        ResumeLayout(false);
        PerformLayout();
    }
    private Label lblAmbitoChat = null!;
    private ListBox lstMensajes = null!;
    private TextBox txtMensaje = null!;
    private Button btnEnviar = null!;
    private Button btnAplauso = null!;
    private Button btnAnimo = null!;
    private Button btnSorpresa = null!;
}
