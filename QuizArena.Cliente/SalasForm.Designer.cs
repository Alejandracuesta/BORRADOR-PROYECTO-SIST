namespace QuizArena.Cliente;

partial class SalasForm
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
        grpConfiguracion = new GroupBox();
        grpBuscador = new GroupBox();
        grpSala = new GroupBox();
        lblNombreSala = new Label();
        lblModalidad = new Label();
        lblAforo = new Label();
        lblPreguntas = new Label();
        lblCategoria = new Label();
        lblDificultad = new Label();
        txtNombreSala = new TextBox();
        cboModalidad = new ComboBox();
        nudMaxJugadores = new NumericUpDown();
        cboPreguntas = new ComboBox();
        cboCategorias = new ComboBox();
        cboDificultad = new ComboBox();
        chkPrivada = new CheckBox();
        btnCrearSala = new Button();
        btnGuardarConfig = new Button();
        lstPublicas = new ListBox();
        btnActualizarSalas = new Button();
        btnUnirsePublica = new Button();
        txtCodigo = new TextBox();
        btnUnirseCodigo = new Button();
        lblSala = new Label();
        lblEstadoSala = new Label();
        lstParticipantes = new ListBox();
        btnPreparado = new Button();
        btnSalirSala = new Button();
        lblAviso = new Label();
        lblCodigoAyuda = new Label();
        btnCopiarCodigo = new Button();
        btnIniciarPartida = new Button();
        btnVerPartida = new Button();
        grpConfiguracion.SuspendLayout();
        grpBuscador.SuspendLayout();
        grpSala.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudMaxJugadores).BeginInit();
        btnChatSala = new Button { Name = "btnChatSala", Text = "Abrir chat", Location = new Point(20, 500), Size = new Size(185, 35) };
        grpSala.Controls.Add(btnChatSala);
        btnCancelarSala = new Button();
        btnCancelarSala.Name = "btnCancelarSala";
        btnCancelarSala.Text = "Cancelar sala";
        btnCancelarSala.Location = new Point(230, 545);
        btnCancelarSala.Size = new Size(205, 35);
        grpSala.Controls.Add(btnCancelarSala);
        btnRevancha = new Button { Name = "btnRevancha", Text = "Solicitar revancha", Location = new Point(230, 500), Size = new Size(205, 35) };
        grpSala.Controls.Add(btnRevancha);
        lblConexion = new Label { Name = "lblConexion", Text = "Comprobando comunicación...", Location = new Point(20, 585), Size = new Size(440, 24) };
        grpSala.Controls.Add(lblConexion);
        btnClasificacion = new Button { Name = "btnClasificacion", Text = "Clasificación global", Location = new Point(20, 270), Size = new Size(200, 36) };
        grpBuscador.Controls.Add(btnClasificacion);
        SuspendLayout();
        //
        // grpConfiguracion
        //
        grpConfiguracion.Location = new Point(15, 15);
        grpConfiguracion.Name = "grpConfiguracion";
        grpConfiguracion.Size = new Size(450, 260);
        grpConfiguracion.TabIndex = 0;
        grpConfiguracion.Text = "Crear / configurar sala";
        //
        // grpBuscador
        //
        grpBuscador.Location = new Point(15, 285);
        grpBuscador.Name = "grpBuscador";
        grpBuscador.Size = new Size(450, 320);
        grpBuscador.TabIndex = 1;
        grpBuscador.Text = "Salas públicas y acceso por código";
        //
        // grpSala
        //
        grpSala.Location = new Point(480, 15);
        grpSala.Name = "grpSala";
        grpSala.Size = new Size(485, 640);
        grpSala.TabIndex = 2;
        grpSala.Text = "Sala de espera";
        //
        // lblNombreSala
        //
        lblNombreSala.Location = new Point(15, 20);
        lblNombreSala.Name = "lblNombreSala";
        lblNombreSala.TabIndex = 0;
        lblNombreSala.Text = "Nombre";
        lblNombreSala.AutoSize = true;
        //
        // lblModalidad
        //
        lblModalidad.Location = new Point(240, 20);
        lblModalidad.Name = "lblModalidad";
        lblModalidad.TabIndex = 1;
        lblModalidad.Text = "Modalidad";
        lblModalidad.AutoSize = true;
        //
        // lblAforo
        //
        lblAforo.Location = new Point(15, 80);
        lblAforo.Name = "lblAforo";
        lblAforo.TabIndex = 2;
        lblAforo.Text = "Jugadores";
        lblAforo.AutoSize = true;
        //
        // lblPreguntas
        //
        lblPreguntas.Location = new Point(115, 80);
        lblPreguntas.Name = "lblPreguntas";
        lblPreguntas.TabIndex = 3;
        lblPreguntas.Text = "Preguntas";
        lblPreguntas.AutoSize = true;
        //
        // lblCategoria
        //
        lblCategoria.Location = new Point(240, 80);
        lblCategoria.Name = "lblCategoria";
        lblCategoria.TabIndex = 4;
        lblCategoria.Text = "Categoría";
        lblCategoria.AutoSize = true;
        //
        // lblDificultad
        //
        lblDificultad.Location = new Point(15, 140);
        lblDificultad.Name = "lblDificultad";
        lblDificultad.TabIndex = 5;
        lblDificultad.Text = "Dificultad";
        lblDificultad.AutoSize = true;
        //
        // txtNombreSala
        //
        txtNombreSala.Location = new Point(15, 40);
        txtNombreSala.Name = "txtNombreSala";
        txtNombreSala.Size = new Size(210, 28);
        txtNombreSala.TabIndex = 6;
        txtNombreSala.MaxLength = 40;
        //
        // cboModalidad
        //
        cboModalidad.Location = new Point(240, 40);
        cboModalidad.Name = "cboModalidad";
        cboModalidad.Size = new Size(190, 28);
        cboModalidad.TabIndex = 7;
        cboModalidad.DropDownStyle = ComboBoxStyle.DropDownList;
        //
        // nudMaxJugadores
        //
        nudMaxJugadores.Location = new Point(15, 100);
        nudMaxJugadores.Name = "nudMaxJugadores";
        nudMaxJugadores.Size = new Size(85, 28);
        nudMaxJugadores.TabIndex = 8;
        nudMaxJugadores.Minimum = 2;
        nudMaxJugadores.Maximum = 6;
        nudMaxJugadores.Value = 4;
        //
        // cboPreguntas
        //
        cboPreguntas.Location = new Point(115, 100);
        cboPreguntas.Name = "cboPreguntas";
        cboPreguntas.Size = new Size(110, 28);
        cboPreguntas.TabIndex = 9;
        cboPreguntas.DropDownStyle = ComboBoxStyle.DropDownList;
        //
        // cboCategorias
        //
        cboCategorias.Location = new Point(240, 100);
        cboCategorias.Name = "cboCategorias";
        cboCategorias.Size = new Size(190, 28);
        cboCategorias.TabIndex = 10;
        cboCategorias.DropDownStyle = ComboBoxStyle.DropDownList;
        //
        // cboDificultad
        //
        cboDificultad.Location = new Point(15, 160);
        cboDificultad.Name = "cboDificultad";
        cboDificultad.Size = new Size(180, 28);
        cboDificultad.TabIndex = 11;
        cboDificultad.DropDownStyle = ComboBoxStyle.DropDownList;
        //
        // chkPrivada
        //
        chkPrivada.Location = new Point(210, 160);
        chkPrivada.Name = "chkPrivada";
        chkPrivada.Size = new Size(180, 28);
        chkPrivada.TabIndex = 12;
        chkPrivada.Text = "Sala privada";
        chkPrivada.UseVisualStyleBackColor = true;
        //
        // btnCrearSala
        //
        btnCrearSala.Location = new Point(15, 210);
        btnCrearSala.Name = "btnCrearSala";
        btnCrearSala.Size = new Size(190, 35);
        btnCrearSala.TabIndex = 13;
        btnCrearSala.Text = "Crear sala";
        btnCrearSala.UseVisualStyleBackColor = true;
        //
        // btnGuardarConfig
        //
        btnGuardarConfig.Location = new Point(215, 210);
        btnGuardarConfig.Name = "btnGuardarConfig";
        btnGuardarConfig.Size = new Size(215, 35);
        btnGuardarConfig.TabIndex = 14;
        btnGuardarConfig.Text = "Guardar configuración";
        btnGuardarConfig.Enabled = false;
        btnGuardarConfig.UseVisualStyleBackColor = true;
        //
        // lstPublicas
        //
        lstPublicas.Font = new Font("Segoe UI", 10F);
        lstPublicas.IntegralHeight = false;
        lstPublicas.Location = new Point(15, 30);
        lstPublicas.Name = "lstPublicas";
        lstPublicas.Size = new Size(420, 120);
        lstPublicas.TabIndex = 0;
        lstPublicas.HorizontalScrollbar = true;
        //
        // btnActualizarSalas
        //
        btnActualizarSalas.Location = new Point(15, 160);
        btnActualizarSalas.Name = "btnActualizarSalas";
        btnActualizarSalas.Size = new Size(195, 32);
        btnActualizarSalas.TabIndex = 1;
        btnActualizarSalas.Text = "Actualizar salas";
        btnActualizarSalas.UseVisualStyleBackColor = true;
        //
        // btnUnirsePublica
        //
        btnUnirsePublica.Location = new Point(220, 160);
        btnUnirsePublica.Name = "btnUnirsePublica";
        btnUnirsePublica.Size = new Size(215, 32);
        btnUnirsePublica.TabIndex = 2;
        btnUnirsePublica.Text = "Unirse a seleccionada";
        btnUnirsePublica.Enabled = false;
        btnUnirsePublica.UseVisualStyleBackColor = true;
        //
        // txtCodigo
        //
        txtCodigo.Font = new Font("Consolas", 12F, FontStyle.Bold);
        txtCodigo.TextAlign = HorizontalAlignment.Center;
        txtCodigo.AccessibleName = "Código de sala";
        txtCodigo.Location = new Point(15, 202);
        txtCodigo.Name = "txtCodigo";
        txtCodigo.Size = new Size(230, 28);
        txtCodigo.TabIndex = 3;
        txtCodigo.MaxLength = 8;
        txtCodigo.CharacterCasing = CharacterCasing.Upper;
        txtCodigo.PlaceholderText = "Código de 8 caracteres";
        //
        // btnUnirseCodigo
        //
        btnUnirseCodigo.Location = new Point(255, 200);
        btnUnirseCodigo.Name = "btnUnirseCodigo";
        btnUnirseCodigo.Size = new Size(180, 32);
        btnUnirseCodigo.TabIndex = 4;
        btnUnirseCodigo.Text = "Entrar con código";
        btnUnirseCodigo.UseVisualStyleBackColor = true;
        //
        // lblSala
        //
        lblSala.Location = new Point(20, 35);
        lblSala.Name = "lblSala";
        lblSala.TabIndex = 0;
        lblSala.Text = "Ninguna sala activa";
        lblSala.AutoSize = true;
        lblSala.MaximumSize = new Size(290, 0);
        //
        // lblEstadoSala
        //
        lblEstadoSala.Location = new Point(20, 100);
        lblEstadoSala.Name = "lblEstadoSala";
        lblEstadoSala.TabIndex = 1;
        lblEstadoSala.Text = "Sin sala";
        lblEstadoSala.AutoSize = true;
        lblEstadoSala.MaximumSize = new Size(440, 0);
        //
        // lstParticipantes
        //
        lstParticipantes.Font = new Font("Segoe UI", 10F);
        lstParticipantes.IntegralHeight = false;
        lstParticipantes.Location = new Point(20, 145);
        lstParticipantes.Name = "lstParticipantes";
        lstParticipantes.Size = new Size(440, 235);
        lstParticipantes.TabIndex = 2;
        lstParticipantes.HorizontalScrollbar = true;
        //
        // btnPreparado
        //
        btnPreparado.Location = new Point(20, 400);
        btnPreparado.Name = "btnPreparado";
        btnPreparado.Size = new Size(200, 38);
        btnPreparado.TabIndex = 3;
        btnPreparado.Text = "Estoy preparado";
        btnPreparado.Enabled = false;
        btnPreparado.UseVisualStyleBackColor = true;
        //
        // btnSalirSala
        //
        btnSalirSala.Location = new Point(240, 400);
        btnSalirSala.Name = "btnSalirSala";
        btnSalirSala.Size = new Size(200, 38);
        btnSalirSala.TabIndex = 4;
        btnSalirSala.Text = "Abandonar sala";
        btnSalirSala.Enabled = false;
        btnSalirSala.UseVisualStyleBackColor = true;
        //
        // btnIniciarPartida
        //
        btnIniciarPartida.Location = new Point(20, 448);
        btnIniciarPartida.Name = "btnIniciarPartida";
        btnIniciarPartida.Size = new Size(200, 36);
        btnIniciarPartida.TabIndex = 5;
        btnIniciarPartida.Text = "Iniciar partida";
        btnIniciarPartida.Enabled = false;
        btnIniciarPartida.UseVisualStyleBackColor = true;
        //
        // btnVerPartida
        //
        btnVerPartida.Location = new Point(240, 448);
        btnVerPartida.Name = "btnVerPartida";
        btnVerPartida.Size = new Size(200, 36);
        btnVerPartida.TabIndex = 6;
        btnVerPartida.Text = "Entrar en partida";
        btnVerPartida.Enabled = false;
        btnVerPartida.UseVisualStyleBackColor = true;
        //
        // lblAviso
        //
        lblAviso.Location = new Point(20, 615);
        lblAviso.Name = "lblAviso";
        lblAviso.TabIndex = 7;
        lblAviso.Text = "Todos preparados: el anfitrión puede iniciar.";
        lblAviso.AutoSize = true;
        lblAviso.MaximumSize = new Size(440, 0);
        // lblCodigoAyuda
        lblCodigoAyuda.AutoSize = true;
        lblCodigoAyuda.Location = new Point(15, 242);
        lblCodigoAyuda.MaximumSize = new Size(420, 0);
        lblCodigoAyuda.Name = "lblCodigoAyuda";
        lblCodigoAyuda.TabIndex = 5;
        lblCodigoAyuda.Text = "Escribe o pega un código de 8 caracteres";
        // btnCopiarCodigo
        btnCopiarCodigo.Location = new Point(325, 30);
        btnCopiarCodigo.Name = "btnCopiarCodigo";
        btnCopiarCodigo.Size = new Size(130, 30);
        btnCopiarCodigo.TabIndex = 8;
        btnCopiarCodigo.Text = "Copiar código";
        btnCopiarCodigo.Enabled = false;
        btnCopiarCodigo.UseVisualStyleBackColor = true;
        Controls.Add(grpConfiguracion);
        Controls.Add(grpBuscador);
        Controls.Add(grpSala);
        grpConfiguracion.Controls.Add(lblNombreSala);
        grpConfiguracion.Controls.Add(lblModalidad);
        grpConfiguracion.Controls.Add(lblAforo);
        grpConfiguracion.Controls.Add(lblPreguntas);
        grpConfiguracion.Controls.Add(lblCategoria);
        grpConfiguracion.Controls.Add(lblDificultad);
        grpConfiguracion.Controls.Add(txtNombreSala);
        grpConfiguracion.Controls.Add(cboModalidad);
        grpConfiguracion.Controls.Add(nudMaxJugadores);
        grpConfiguracion.Controls.Add(cboPreguntas);
        grpConfiguracion.Controls.Add(cboCategorias);
        grpConfiguracion.Controls.Add(cboDificultad);
        grpConfiguracion.Controls.Add(chkPrivada);
        grpConfiguracion.Controls.Add(btnCrearSala);
        grpConfiguracion.Controls.Add(btnGuardarConfig);
        grpBuscador.Controls.Add(lblCodigoAyuda);
        grpBuscador.Controls.Add(lstPublicas);
        grpBuscador.Controls.Add(btnActualizarSalas);
        grpBuscador.Controls.Add(btnUnirsePublica);
        grpBuscador.Controls.Add(txtCodigo);
        grpBuscador.Controls.Add(btnUnirseCodigo);
        grpSala.Controls.Add(btnCopiarCodigo);
        grpSala.Controls.Add(lblSala);
        grpSala.Controls.Add(lblEstadoSala);
        grpSala.Controls.Add(lstParticipantes);
        grpSala.Controls.Add(btnPreparado);
        grpSala.Controls.Add(btnSalirSala);
        grpSala.Controls.Add(btnIniciarPartida);
        grpSala.Controls.Add(btnVerPartida);
        grpSala.Controls.Add(lblAviso);
        AutoScaleDimensions = new SizeF(8F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 690);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "SalasForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quiz Arena - Salas multijugador";
        grpConfiguracion.ResumeLayout(false);
        grpConfiguracion.PerformLayout();
        grpBuscador.ResumeLayout(false);
        grpBuscador.PerformLayout();
        grpSala.ResumeLayout(false);
        grpSala.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudMaxJugadores).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private GroupBox grpConfiguracion;
    private GroupBox grpBuscador;
    private GroupBox grpSala;
    private Label lblNombreSala;
    private Label lblModalidad;
    private Label lblAforo;
    private Label lblPreguntas;
    private Label lblCategoria;
    private Label lblDificultad;
    private TextBox txtNombreSala;
    private ComboBox cboModalidad;
    private NumericUpDown nudMaxJugadores;
    private ComboBox cboPreguntas;
    private ComboBox cboCategorias;
    private ComboBox cboDificultad;
    private CheckBox chkPrivada;
    private Button btnCrearSala;
    private Button btnGuardarConfig;
    private ListBox lstPublicas;
    private Button btnActualizarSalas;
    private Button btnUnirsePublica;
    private TextBox txtCodigo;
    private Button btnUnirseCodigo;
    private Label lblSala;
    private Label lblEstadoSala;
    private ListBox lstParticipantes;
    private Button btnPreparado;
    private Button btnSalirSala;
    private Label lblAviso;
    private Label lblCodigoAyuda;
    private Button btnCopiarCodigo;
    private Button btnIniciarPartida;
    private Button btnVerPartida;
    private Button btnChatSala;
    private Button btnCancelarSala;
    private Button btnRevancha;
    private Label lblConexion;
    private Button btnClasificacion;
}
