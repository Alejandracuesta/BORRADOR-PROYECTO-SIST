namespace QuizArena.Cliente;
partial class ClasificacionForm
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    protected override void Dispose(bool disposing) { if (disposing) components.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        SuspendLayout();
        lblTituloIndividual = new Label();
        lblTituloIndividual.Name = "lblTituloIndividual";
        lblTituloIndividual.Text = "Clasificación individual";
        lblTituloIndividual.Location = new Point(20, 15);
        lblTituloIndividual.Size = new Size(600, 25);
        Controls.Add(lblTituloIndividual);
        lvIndividual = new ListView();
        lvIndividual.Name = "lvIndividual";
        lvIndividual.Text = "";
        lvIndividual.Location = new Point(20, 45);
        lvIndividual.Size = new Size(810, 205);
        Controls.Add(lvIndividual);
        lblTituloEquipos = new Label();
        lblTituloEquipos.Name = "lblTituloEquipos";
        lblTituloEquipos.Text = "Mejores resultados por equipos";
        lblTituloEquipos.Location = new Point(20, 275);
        lblTituloEquipos.Size = new Size(600, 25);
        Controls.Add(lblTituloEquipos);
        lvEquipos = new ListView();
        lvEquipos.Name = "lvEquipos";
        lvEquipos.Text = "";
        lvEquipos.Location = new Point(20, 305);
        lvEquipos.Size = new Size(810, 205);
        Controls.Add(lvEquipos);
        btnActualizar = new Button();
        btnActualizar.Name = "btnActualizar";
        btnActualizar.Text = "Actualizar";
        btnActualizar.Location = new Point(340, 530);
        btnActualizar.Size = new Size(170, 38);
        Controls.Add(btnActualizar);
        lvIndividual.View = View.Details;
        lvEquipos.View = View.Details;
        lvIndividual.FullRowSelect = true;
        lvEquipos.FullRowSelect = true;
        lvIndividual.GridLines = true;
        lvEquipos.GridLines = true;
        ClientSize = new Size(850, 600);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quiz Arena - Clasificación global";
        ResumeLayout(false);
    }
    private Label lblTituloIndividual;
    private ListView lvIndividual;
    private Label lblTituloEquipos;
    private ListView lvEquipos;
    private Button btnActualizar;
}
