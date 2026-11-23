namespace QuizArena.Cliente;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        btnComprobar = new Button();
        lblEstado = new Label();
        SuspendLayout();
        //
        // btnComprobar
        //
        btnComprobar.Location = new Point(28, 28);
        btnComprobar.Name = "btnComprobar";
        btnComprobar.Size = new Size(210, 42);
        btnComprobar.TabIndex = 0;
        btnComprobar.Text = "Comprobar conexión";
        btnComprobar.UseVisualStyleBackColor = true;
        btnComprobar.Click += btnComprobar_Click;
        //
        // lblEstado
        //
        lblEstado.AutoSize = true;
        lblEstado.Location = new Point(28, 94);
        lblEstado.Name = "lblEstado";
        lblEstado.Size = new Size(103, 19);
        lblEstado.TabIndex = 1;
        lblEstado.Text = "Sin comprobar";
        //
        // Form1
        //
        AcceptButton = btnComprobar;
        AutoScaleDimensions = new SizeF(8F, 19F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(450, 150);
        Controls.Add(lblEstado);
        Controls.Add(btnComprobar);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Quiz Arena · Conexión con el servidor";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button btnComprobar;
    private Label lblEstado;
}
