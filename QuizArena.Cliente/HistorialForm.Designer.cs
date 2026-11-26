namespace QuizArena.Cliente;
partial class HistorialForm
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    protected override void Dispose(bool disposing) { if (disposing) components.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        dgvHistorial = new DataGridView();
        btnActualizar = new Button();
        btnVerResultado = new Button();
        txtResultado = new TextBox();
        lblResultado = new Label();
        SuspendLayout();
        dgvHistorial.Name = "dgvHistorial";
        dgvHistorial.Text = "";
        dgvHistorial.Location = new Point(20, 20);
        dgvHistorial.Size = new Size(810, 300);
        dgvHistorial.ReadOnly = true;
        dgvHistorial.AllowUserToAddRows = false;
        dgvHistorial.AllowUserToDeleteRows = false;
        dgvHistorial.MultiSelect = false;
        dgvHistorial.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.Controls.Add(dgvHistorial);
        btnActualizar.Name = "btnActualizar";
        btnActualizar.Text = "Actualizar";
        btnActualizar.Location = new Point(20, 335);
        btnActualizar.Size = new Size(170, 35);
        this.Controls.Add(btnActualizar);
        btnVerResultado.Name = "btnVerResultado";
        btnVerResultado.Text = "Ver resultado";
        btnVerResultado.Location = new Point(210, 335);
        btnVerResultado.Size = new Size(170, 35);
        this.Controls.Add(btnVerResultado);
        txtResultado.Name = "txtResultado";
        txtResultado.Text = "";
        txtResultado.Location = new Point(20, 390);
        txtResultado.Size = new Size(810, 170);
        txtResultado.ReadOnly = true;
        txtResultado.Multiline = true;
        txtResultado.ScrollBars = ScrollBars.Vertical;
        this.Controls.Add(txtResultado);
        lblResultado.Name = "lblResultado";
        lblResultado.Text = "";
        lblResultado.Location = new Point(20, 575);
        lblResultado.Size = new Size(810, 40);
        this.Controls.Add(lblResultado);
        ClientSize = new Size(850, 630);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quiz Arena - Historial de partidas";
        ResumeLayout(false);
        PerformLayout();
    }
    private DataGridView dgvHistorial = null!;
    private Button btnActualizar = null!;
    private Button btnVerResultado = null!;
    private TextBox txtResultado = null!;
    private Label lblResultado = null!;
}
