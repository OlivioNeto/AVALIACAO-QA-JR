namespace PROJETO_QA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnConsulta_Click(object sender, EventArgs e)
        {
            lbPreco.Text = "Pesquisando";

            double preco = 350000.75;

            lbPreco.Text = preco.ToString("C");
        }
    }
}
