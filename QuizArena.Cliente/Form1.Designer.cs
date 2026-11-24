namespace QuizArena.Cliente;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblTitulo = new Label();
        lblNombre = new Label();
        txtNombre = new TextBox();
        lblClave = new Label();
        txtClave = new TextBox();
        btnRegistrar = new Button();
        btnEntrar = new Button();
        lblJugador = new Label();
        btnPerfil = new Button();
        btnSalir = new Button();
        btnComprobar = new Button();
        lblEstado = new Label();
        SuspendLayout();
        //
        // lblTitulo
        //
        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitulo.Location = new Point(30, 20);
        lblTitulo.Name = "lblTitulo";
        lblTitulo.Size = new Size(179, 37);
        lblTitulo.TabIndex = 0;
        lblTitulo.Text = "QUIZ ARENA";
        //
        // lblNombre
        //
        lblNombre.AutoSize = true;
        lblNombre.Location = new Point(30, 75);
        lblNombre.Name = "lblNombre";
        lblNombre.Size = new Size(133, 19);
        lblNombre.TabIndex = 1;
        lblNombre.Text = "Nombre de jugador:";
        //
        // txtNombre
        //
        txtNombre.Location = new Point(190, 70);
        txtNombre.MaxLength = 20;
        txtNombre.Name = "txtNombre";
        txtNombre.Size = new Size(260, 28);
        txtNombre.TabIndex = 2;
        //
        // lblClave
        //
        lblClave.AutoSize = true;
        lblClave.Location = new Point(30, 120);
        lblClave.Name = "lblClave";
        lblClave.Size = new Size(83, 19);
        lblClave.TabIndex = 3;
        lblClave.Text = "Contraseña:";
        //
        // txtClave
        //
        txtClave.Location = new Point(190, 115);
        txtClave.MaxLength = 64;
        txtClave.Name = "txtClave";
        txtClave.Size = new Size(260, 28);
        txtClave.TabIndex = 4;
        txtClave.UseSystemPasswordChar = true;
        //
        // btnRegistrar
        //
        btnRegistrar.Location = new Point(190, 165);
        btnRegistrar.Name = "btnRegistrar";
        btnRegistrar.Size = new Size(125, 38);
        btnRegistrar.TabIndex = 5;
        btnRegistrar.Text = "Registrarse";
        btnRegistrar.UseVisualStyleBackColor = true;
        //
        // btnEntrar
        //
        btnEntrar.Location = new Point(325, 165);
        btnEntrar.Name = "btnEntrar";
        btnEntrar.Size = new Size(125, 38);
        btnEntrar.TabIndex = 6;
        btnEntrar.Text = "Iniciar sesión";
        btnEntrar.UseVisualStyleBackColor = true;
        //
        // lblJugador
        //
        lblJugador.AutoSize = true;
        lblJugador.Location = new Point(30, 225);
        lblJugador.Name = "lblJugador";
        lblJugador.Size = new Size(128, 19);
        lblJugador.TabIndex = 7;
        lblJugador.Text = "Sin sesión iniciada";
        //
        // btnPerfil
        //
        btnPerfil.Enabled = false;
        btnPerfil.Location = new Point(30, 265);
        btnPerfil.Name = "btnPerfil";
        btnPerfil.Size = new Size(140, 38);
        btnPerfil.TabIndex = 8;
        btnPerfil.Text = "Ver mi perfil";
        btnPerfil.UseVisualStyleBackColor = true;
        //
        // btnSalir
        //
        btnSalir.Enabled = false;
        btnSalir.Location = new Point(185, 265);
        btnSalir.Name = "btnSalir";
        btnSalir.Size = new Size(140, 38);
        btnSalir.TabIndex = 9;
        btnSalir.Text = "Cerrar sesión";
        btnSalir.UseVisualStyleBackColor = true;
        //
        // btnComprobar
        //
        btnComprobar.Location = new Point(30, 335);
        btnComprobar.Name = "btnComprobar";
        btnComprobar.Size = new Size(210, 38);
        btnComprobar.TabIndex = 10;
        btnComprobar.Text = "Comprobar conexión";
        btnComprobar.UseVisualStyleBackColor = true;
        btnComprobar.Click += btnComprobar_Click;
        //
        // lblEstado
        //
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(255, 345);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(103, 19);
        lblEstado.TabIndex = 11;
        lblEstado.Text = "Sin comprobar";
        //
        // Form1
        //
        AcceptButton = btnEntrar;
        AutoScaleDimensions = new SizeF(8F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(650, 410);
        Controls.Add(lblEstado);
        Controls.Add(btnComprobar);
        Controls.Add(btnSalir);
        Controls.Add(btnPerfil);
        Controls.Add(lblJugador);
        Controls.Add(btnEntrar);
        Controls.Add(btnRegistrar);
        Controls.Add(txtClave);
        Controls.Add(lblClave);
        Controls.Add(txtNombre);
        Controls.Add(lblNombre);
        Controls.Add(lblTitulo);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Quiz Arena - Acceso de jugadores";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitulo;
    private Label lblNombre;
    private TextBox txtNombre;
    private Label lblClave;
    private TextBox txtClave;
    private Button btnRegistrar;
    private Button btnEntrar;
    private Label lblJugador;
    private Button btnPerfil;
    private Button btnSalir;
    private Button btnComprobar;
    private Label lblEstado;
}
