namespace PROJETO_QA
{
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
            lbPreco = new Label();
            btnConsulta = new Button();
            dgvHistorico = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvHistorico).BeginInit();
            SuspendLayout();
            // 
            // lbPreco
            // 
            lbPreco.AutoSize = true;
            lbPreco.Location = new Point(48, 45);
            lbPreco.Name = "lbPreco";
            lbPreco.Size = new Size(153, 20);
            lbPreco.TabIndex = 0;
            lbPreco.Text = "Qual a atual cotação?";
            lbPreco.Click += label1_Click;
            // 
            // btnConsulta
            // 
            btnConsulta.Location = new Point(48, 86);
            btnConsulta.Name = "btnConsulta";
            btnConsulta.Size = new Size(167, 46);
            btnConsulta.TabIndex = 1;
            btnConsulta.Text = "Pesquisar a Cotação";
            btnConsulta.UseVisualStyleBackColor = true;
            btnConsulta.Click += btnConsulta_Click;
            // 
            // dgvHistorico
            // 
            dgvHistorico.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHistorico.Location = new Point(48, 166);
            dgvHistorico.Name = "dgvHistorico";
            dgvHistorico.RowHeadersWidth = 51;
            dgvHistorico.RowTemplate.Height = 29;
            dgvHistorico.Size = new Size(525, 272);
            dgvHistorico.TabIndex = 2;
            dgvHistorico.CellContentClick += dgvHistorico_CellContentClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvHistorico);
            Controls.Add(btnConsulta);
            Controls.Add(lbPreco);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgvHistorico).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbPreco;
        private Button btnConsulta;
        private DataGridView dgvHistorico;

        
    }
}
