using System.Configuration;
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

        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CoinGeckoDb;Trusted_Connection=True;";

        private static readonly HttpClient httpClient = new HttpClient() // instância única do HttpClient reutilizada em toda a aplicação, com timeout de 10 segundos.
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnConsulta_Click(object sender, EventArgs e)
        {
            try
            {
                btnConsulta.Enabled = false; // impede novos cliques durante a consulta
                btnConsulta.Text = "Atualizando..."; // muda o texto, dando feedback

                lbPreco.Text = "Pesquisando"; // texto da label
                lbPreco.ForeColor = Color.Black; // muda a cor do texto

                double preco = await ObterPrecoBitcoin();

                double? variacao = SalvarCotacao(preco);
                                
                AtualizarIndicacaoVisual(preco, variacao);
                
                ExibirHistorico();
            }
            catch (Exception ex)
            {
                lbPreco.ForeColor = Color.Red; // mudando a cor em caso de alguma exceção
                lbPreco.Text = ex.Message; // exibindo a mensagem
            }
            finally
            {
                btnConsulta.Enabled = true; // deixo que o usuário clique novamente
                btnConsulta.Text = "Atualizar"; // muda o texto, dando feedback
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
            try // se tudo correr bem
            {

                if (!httpClient.DefaultRequestHeaders.UserAgent.Any()) // faz uma verificação se existe ou não um user agent cadastrado, caso não exista cai dentro do if
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "AvaliacaoQA/1.0 (Windows Forms; contato: netoolivio34@gmail.com)"
                    ); // essa parte mostra para a api que eu sou um usuário e caso eu abuse ela tem um contato para chegar até mim
                }

                var respostaHttp = await httpClient.GetAsync(
                    "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl"
                ); // pegando uma resposta da API

                if (!respostaHttp.IsSuccessStatusCode)
                {
                    throw new Exception(
                    $"Erro ao consultar API. Código HTTP: {(int)respostaHttp.StatusCode} - {respostaHttp.StatusCode}"
                    );
                }

                string guardandoJson = await respostaHttp.Content.ReadAsStringAsync(); // pegando um json e fazendo ele ser lido como string

                if (string.IsNullOrWhiteSpace(guardandoJson)) // se for nulo ou vier em branco
                {
                    throw new Exception("A API retornou uma resposta vazia.");
                }

                var objDesserializado = JsonSerializer.Deserialize<RespostaBitcoin>(guardandoJson); // tranformando o json em algpo que o C# entenda
                
                if (objDesserializado == null)
                {
                    throw new Exception("A API retornou uma resposta em formato inválido.");
                }

                if (objDesserializado.bitcoin == null)
                {
                    throw new Exception("A resposta da API não contém a cotação do Bitcoin.");
                }
                
                return objDesserializado.bitcoin.brl; // retornando o objeto com a moeda bitcoin e o brl que é a moeda brasileira
            }

            catch (HttpRequestException) // caso aconteceça alguma exeção, falha de internet, api fora.....
            {
                throw new Exception("Falha na comunicação com a API. Verifique sua conexão com a internet ou tente novamente mais tarde.");
            }
            catch (TaskCanceledException) // caso demore para responder, deu time out
            {
                throw new Exception("A API demorou mais do que o esperado para responder. Aguarde alguns instantes e tente novamente.");
            }
            catch (JsonException) // caso o JSON venha mal formatado
            {
                throw new Exception("A API retornou um JSON inválido.");
            }
        }

        class RespostaBitcoin // é um objeto com a prioridade bitcoin
        {
            public Moeda? bitcoin { get; set; } // está dizendo que a moeda pode vir nula e tenho que validar isso antes, o que acontece no método ObterPrecoBitcoin
        }

        class Moeda // é o que possui dentro do bitcoin
        {
            public double brl { get; set; }
        }

        private double? SalvarCotacao(double valor)
        {
            using (SqlConnection conexao = new SqlConnection(connectionString)) // criando conexão com o banco
            {
                double? ultimoPreco = ObterUltimoPreco(); // obtém a última cotação registrada no banco de dados

                double? variacao = null; // inicializará a variável que armazenará a variação entre as cotações

                if (ultimoPreco.HasValue) // verifica se existe uma cotação anterior registrada
                {
                    variacao = valor - ultimoPreco.Value; // calcula a diferença entre o preço atual e a última cotação registrada
                }

                conexao.Open(); // abrindo a conexão
                
                string sql = "INSERT INTO Cotacoes (Preco, Variacao) VALUES (@preco, @variacao)"; // o que eu quero do banco naquele momento

                using (SqlCommand comando = new SqlCommand(sql, conexao)) // cria um chamado sql que vai ser executado no banco
                {
                    comando.Parameters.AddWithValue("@preco", valor); // usado o valor preço como parâmetro por segurança 

                    comando.Parameters.AddWithValue("@variacao", variacao.HasValue ? variacao.Value : DBNull.Value);

                    comando.ExecuteNonQuery(); // executa o comando no banco, não retorna dados

                    return variacao;
                }
            }
        }

        private double? ObterUltimoPreco() // consulta o banco de dados e retorna a última cotação registrada
        {
            using (SqlConnection conexao = new SqlConnection(connectionString))
            {
                conexao.Open();

                string sql = "SELECT TOP 1 Preco FROM Cotacoes ORDER BY DataHora DESC"; // busca o preço da cotação mais recente registrada

                using (SqlCommand comando = new SqlCommand(sql, conexao)) // cria um chamado sql que vai ser executado no banco
                {

                    object resultado = comando.ExecuteScalar(); // vai me retornar apenas um único valor e válido

                    if (resultado == null || resultado == DBNull.Value) // retorna nulo caso não exista nenhuma cotação cadastrada
                    {
                        return null;
                    }

                    return Convert.ToDouble(resultado); // converte o resultado para double e retorna a última cotação encontrada
                }
            }            
        }
        
        private void ExibirHistorico()
        {
            using (SqlConnection conexao = new SqlConnection(connectionString)) // criando conexão com o banco
            {
                conexao.Open(); // abrindo a conexão
                string sql = "SELECT DataHora, Preco, Variacao FROM Cotacoes ORDER BY DataHora DESC"; // o que eu quero do banco naquele momento

                using (SqlDataAdapter adapta = new SqlDataAdapter(sql, conexao)) // e a ponte do banco e a tabela c#, busca dos dados e coloca dentro da DGV
                {
                    DataTable tabela = new DataTable(); // cria a memória da tabela
                    adapta.Fill(tabela); // executa o select e preenche

                    dgvHistorico.DataSource = tabela; // onde eu ligo a tabela com o DGV

                    ConfigurarGridHistorico();
                }
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
                dgvHistorico.Columns["Preco"].HeaderText = "Preço"; // nome do cabeçalho do DGV
                dgvHistorico.Columns["Preco"].DefaultCellStyle.Format = "C2"; // formatando para a moeda brasileira
            }

            if (dgvHistorico.Columns.Contains("Variacao"))
            {
                dgvHistorico.Columns["Variacao"].HeaderText = "Variação"; // nome do cabeçalho do DGV
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
                lbPreco.Text = "Não foi possível carregar o histórico: " + ex.Message;
            }
        }

        private void dgvHistorico_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
