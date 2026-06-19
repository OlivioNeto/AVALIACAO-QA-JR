using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using PROJETO_QA.Services;

namespace PROJETO_QA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["CoinGeckoDb"]?.ConnectionString
                ?? throw new InvalidOperationException("Connection string 'CoinGeckoDb' não foi encontrada no arquivo de configuração.");  
        }

        private readonly CoinGeckoService coinGeckoService = new CoinGeckoService();

        private readonly string connectionString;

        private const string InserirCotacaoSql = "INSERT INTO Cotacoes (Preco, Variacao) VALUES (@preco, @variacao)";

        private const string ObterUltimoPrecoSql = "SELECT TOP 1 Preco FROM Cotacoes ORDER BY DataHora DESC"; // busca o preço da cotação mais recente registrada

        private const string ExibirHistoricoSql = "SELECT DataHora, Preco, Variacao FROM Cotacoes ORDER BY DataHora DESC";

        private async void btnConsulta_Click(object sender, EventArgs e)
        {
            try
            {
                btnConsulta.Enabled = false;
                btnConsulta.Text = "Atualizando...";

                lbPreco.Text = "Pesquisando";
                lbPreco.ForeColor = Color.Black;

                double preco = await coinGeckoService.ObterPrecoBitcoinAsync();

                double? variacao = SalvarCotacao(preco);
                                
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

        private double? SalvarCotacao(double valor)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString)) 
                {
                    double? ultimoPreco = ObterUltimoPreco();

                    double? variacao = null;

                    if (ultimoPreco.HasValue)
                    {
                        variacao = valor - ultimoPreco.Value;
                    }

                    conexao.Open();

                    using (SqlCommand comando = new SqlCommand(InserirCotacaoSql, conexao)) 
                    {
                        comando.Parameters.Add("@preco", SqlDbType.Decimal).Value = valor;

                        comando.Parameters.Add("@variacao", SqlDbType.Decimal).Value = variacao.HasValue ? variacao.Value : DBNull.Value;

                        comando.ExecuteNonQuery();

                        return variacao;
                    }
                }
            } 
            catch (SqlException)
            {
                throw new Exception("Não foi possível salvar a cotação no banco de dados. Verifique se o LocalDB, o banco CoinGeckoDb e a tabela Cotacoes estão configurados corretamente.");
            }
        }

        private double? ObterUltimoPreco()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString))
                {
                    conexao.Open();

                    using (SqlCommand comando = new SqlCommand(ObterUltimoPrecoSql, conexao))
                    {
                        object resultado = comando.ExecuteScalar(); // vai me retornar apenas um único valor e válido

                        if (resultado == null || resultado == DBNull.Value)
                        {
                            return null;
                        }

                        return Convert.ToDouble(resultado);
                    }
                }
            } 
            catch (SqlException)
            {
                throw new Exception("Não foi possível consultar a última cotação no banco de dados.");
            }
            
        }
        
        private void ExibirHistorico()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString))
                {
                    conexao.Open();

                    using (SqlDataAdapter adaptador = new SqlDataAdapter(ExibirHistoricoSql, conexao)) // e a ponte do banco e a tabela c#, busca dos dados e coloca dentro da DGV
                    {
                        DataTable tabela = new DataTable(); // cria a memória da tabela
                        adaptador.Fill(tabela); // executa o select e preenche

                        dgvHistorico.DataSource = tabela; // onde eu ligo a tabela com o DGV

                        ConfigurarGridHistorico();
                    }
                }
            }
            catch (SqlException)
            {
                throw new Exception("Não foi possível carregar o histórico de cotações no banco de dados.");
            }
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
