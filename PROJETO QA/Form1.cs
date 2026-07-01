using PROJETO_QA.Repositories;
using PROJETO_QA.Services;
using System.Configuration;
using System.Data;

namespace PROJETO_QA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["CoinGeckoDb"]?.ConnectionString
                ?? throw new InvalidOperationException("Connection string 'CoinGeckoDb' não foi encontrada no arquivo de configuração.");  
            cotacaoRepository = new CotacaoRepository(connectionString);
        }

        private readonly CoinGeckoService coinGeckoService = new CoinGeckoService();

        private readonly string connectionString;

        private readonly CotacaoRepository cotacaoRepository;

        private async void btnConsulta_Click(object sender, EventArgs e)
        {
            try
            {
                btnConsulta.Enabled = false;
                btnConsulta.Text = "Atualizando...";

                lbPreco.Text = "Pesquisando";
                lbPreco.ForeColor = Color.Black;

                double preco = await coinGeckoService.ObterPrecoBitcoinAsync();

                double? variacao = cotacaoRepository.SalvarCotacao(preco);

                AtualizarIndicacaoVisual(preco, variacao);

                ExibirHistorico();
            }
            catch (Exception ex)
            {
                lbPreco.ForeColor = Color.Red;
                lbPreco.Text = ex.Message;
            }
            finally
            {
                btnConsulta.Enabled = true;
                btnConsulta.Text = "Atualizar";
            }

        }

        private void AtualizarIndicacaoVisual(double preco, double? variacao)
        {
            if (!variacao.HasValue)
            {
                lbPreco.Text = $"{preco:C2}\nSem cotação anterior para comparação.";
                lbPreco.ForeColor = Color.Black;
            }

            else if (variacao > 0)
            {
                lbPreco.Text = $"{preco:C2}\nAlta de {variacao.Value:C2}";
                lbPreco.ForeColor = Color.Green;
            }

            else if (variacao < 0)
            {
                lbPreco.Text = $"{preco:C2}\nQueda de {Math.Abs(variacao.Value):C2}";
                lbPreco.ForeColor = Color.Red;
            }

            else
            {
                lbPreco.Text = $"{preco:C2}\nEstável";
                lbPreco.ForeColor = Color.Blue;
            }
        }

        private void ExibirHistorico()
        {
            DataTable tabela = cotacaoRepository.ObterHistorico();

            dgvHistorico.DataSource = tabela;
            ConfigurarGridHistorico();
        }

        private void ConfigurarGridHistorico()
        {
            dgvHistorico.ReadOnly = true; // impede que o usuário edite os dados exibidos no DGV
            dgvHistorico.AllowUserToAddRows = false; // impede que o usuário adicione novas linhas manualmente
            dgvHistorico.AllowUserToDeleteRows = false; // impede que o usuário exclua linhas do DGV
            dgvHistorico.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // faz com que a seleção destaque a linha inteira em vez de apenas uma célula
            dgvHistorico.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // ajusta automaticamente a largura das colunas para preencher todo o DGV

            if (dgvHistorico.Columns.Contains("DataHora"))
            {
                dgvHistorico.Columns["DataHora"].HeaderText = "Data/Hora"; // nome do cabeçalho do DGV
                dgvHistorico.Columns["DataHora"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss"; // essa aparição vai ser de acordo com o formato colocado
            }

            if (dgvHistorico.Columns.Contains("Preco"))
            {
                dgvHistorico.Columns["Preco"].HeaderText = "Preço";
                dgvHistorico.Columns["Preco"].DefaultCellStyle.Format = "C2";
            }

            if (dgvHistorico.Columns.Contains("Variacao"))
            {
                dgvHistorico.Columns["Variacao"].HeaderText = "Variação";
                dgvHistorico.Columns["Variacao"].DefaultCellStyle.Format = "C2"; // formatando para a moeda brasileira
            }
        }

        private void Form1_Load(object sender, EventArgs e) // evento executado automaticamente quando o formulário é carregado
        {
            try // carrega e exibi o histórico de cotações ao iniciar o formulário
            {
                ExibirHistorico();
            }
            catch (Exception ex) // captura possíveis erros ocorridos durante o carregamento do histórico
            {
                lbPreco.Text = ex.Message;
            }
        }
    }
}
