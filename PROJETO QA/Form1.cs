using System.Data;
using System.Data.SqlClient;
using System.Text.Json;

namespace PROJETO_QA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CoinGeckoDb;Trusted_Connection=True;";
        private const string CoinGeckoUrl = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl";

        private static readonly HttpClient httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        private async void btnConsulta_Click(object sender, EventArgs e)
        {
            try
            {
                btnConsulta.Enabled = false; // impede novos cliques durante a consulta
                btnConsulta.Text = "Atualizando...";

                lbPreco.Text = "Pesquisando";
                lbPreco.ForeColor = Color.Black;

                double preco = await ObterPrecoBitcoin();

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
                btnConsulta.Enabled = true; // deixo que o usuário clique novamente
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

            else if (variacao < 0 )
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

        private async Task<double> ObterPrecoBitcoin() // async pois o método usa await, algo de de fora e Task double para devolver double
        {
            try
            {

                if (!httpClient.DefaultRequestHeaders.UserAgent.Any()) // faz uma verificação se existe ou não um user agent cadastrado, caso não exista cai dentro do if
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "AvaliacaoQA/1.0 (Windows Forms; contato: netoolivio34@gmail.com)"
                    ); // essa parte mostra para a api que eu sou um usuário e caso eu abuse ela tem um contato para chegar até mim
                }

                var respostaHttp = await httpClient.GetAsync(CoinGeckoUrl); // pegando uma resposta da API

                if (!respostaHttp.IsSuccessStatusCode)
                {
                    throw new Exception(
                    $"Erro ao consultar API. Código HTTP: {(int)respostaHttp.StatusCode} - {respostaHttp.StatusCode}"
                    );
                }

                string json = await respostaHttp.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json)) // se for nulo ou vier em branco
                {
                    throw new Exception("A API retornou uma resposta vazia.");
                }

                var respostaBitcoin = JsonSerializer.Deserialize<RespostaBitcoin>(json); // tranformando o json em algo que o C# entenda
                
                if (respostaBitcoin == null)
                {
                    throw new Exception("A API retornou uma resposta em formato inválido.");
                }

                if (respostaBitcoin.bitcoin == null)
                {
                    throw new Exception("A resposta da API não contém a cotação do Bitcoin.");
                }
                
                return respostaBitcoin.bitcoin.brl;
            }

            catch (HttpRequestException) 
            {
                throw new Exception("Falha na comunicação com a API. Verifique sua conexão com a internet ou tente novamente mais tarde.");
            }
            catch (TaskCanceledException)
            {
                throw new Exception("A API demorou mais do que o esperado para responder. Aguarde alguns instantes e tente novamente.");
            }
            catch (JsonException)
            {
                throw new Exception("A API retornou um JSON inválido.");
            }
        }

        class RespostaBitcoin
        {
            public Moeda? bitcoin { get; set; } // está dizendo que a moeda pode vir nula e tenho que validar isso antes, o que acontece no método ObterPrecoBitcoin
        }

        class Moeda
        {
            public double brl { get; set; }
        }

        private double? SalvarCotacao(double valor)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString)) 
                {
                    double? ultimoPreco = ObterUltimoPreco(); // obtém a última cotação registrada no banco de dados

                    double? variacao = null;

                    if (ultimoPreco.HasValue) // verifica se existe uma cotação anterior registrada
                    {
                        variacao = valor - ultimoPreco.Value;
                    }

                    conexao.Open();

                    string sql = "INSERT INTO Cotacoes (Preco, Variacao) VALUES (@preco, @variacao)";

                    using (SqlCommand comando = new SqlCommand(sql, conexao)) 
                    {
                        comando.Parameters.AddWithValue("@preco", valor); // usado o valor preço como parâmetro por segurança 

                        comando.Parameters.AddWithValue("@variacao", variacao.HasValue ? variacao.Value : DBNull.Value);

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

                    string sql = "SELECT TOP 1 Preco FROM Cotacoes ORDER BY DataHora DESC"; // busca o preço da cotação mais recente registrada

                    using (SqlCommand comando = new SqlCommand(sql, conexao))
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
                    string sql = "SELECT DataHora, Preco, Variacao FROM Cotacoes ORDER BY DataHora DESC";

                    using (SqlDataAdapter adaptador = new SqlDataAdapter(sql, conexao)) // e a ponte do banco e a tabela c#, busca dos dados e coloca dentro da DGV
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
