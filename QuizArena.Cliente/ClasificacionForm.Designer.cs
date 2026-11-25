namespace QuizArena.Cliente;
partial class ClasificacionForm
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    protected override void Dispose(bool disposing) { if (disposing) components.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        SuspendLayout();
        lblTituloIndividual = new Label { Name = "lblTituloIndividual", Text = "Clasificación individual", Location = new Point(20, 15), Size = new Size(600, 25) };
        Controls.Add(lblTituloIndividual);
        lvIndividual = new ListView { Name = "lvIndividual", Text = "", Location = new Point(20, 45), Size = new Size(810, 205) };
        Controls.Add(lvIndividual);
        lblTituloEquipos = new Label { Name = "lblTituloEquipos", Text = "Mejores resultados por equipos", Location = new Point(20, 275), Size = new Size(600, 25) };
        Controls.Add(lblTituloEquipos);
        lvEquipos = new ListView { Name = "lvEquipos", Text = "", Location = new Point(20, 305), Size = new Size(810, 205) };
        Controls.Add(lvEquipos);
        btnActualizar = new Button { Name = "btnActualizar", Text = "Actualizar", Location = new Point(340, 530), Size = new Size(170, 38) };
        Controls.Add(btnActualizar);
        lvIndividual.View = lvEquipos.View = View.Details;
        lvIndividual.FullRowSelect = lvEquipos.FullRowSelect = true;
        lvIndividual.GridLines = lvEquipos.GridLines = true;
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
