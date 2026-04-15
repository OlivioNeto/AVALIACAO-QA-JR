using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Json;
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnConsulta_Click(object sender, EventArgs e)
        {
            try
            {
                lbPreco.Text = "Pesquisando";

                double preco = await ObterPrecoBitcoin();
                lbPreco.Text = preco.ToString("C");
                SalvarCotacao(preco);
                ExbirHistorico();
            }
            catch (Exception ex)
            {
                lbPreco.Text = ex.Message;
            }

        }

        private async Task<double> ObterPrecoBitcoin() // async pois o método usa await, algo de de fora e Task double para devolver double
        {

            HttpClient clientHttp = new HttpClient(); // variavel para requisição

            clientHttp.DefaultRequestHeaders.UserAgent.ParseAdd(
                "AvaliacaoQA/1.0 (Windows Forms; contato: netoolivio34@gmail.com)"
            ); // essa parte mostra para a api que eu sou um usuário e caso eu abuse ela tem um contato para chegar até mim

            var respostaHttp = await clientHttp.GetAsync("https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl"); // pegando uma resposta da API

            string guardandoJson = await respostaHttp.Content.ReadAsStringAsync(); // pegando um json e fazendo ele ser lido como string

            var objDesserializado = JsonSerializer.Deserialize<RespostaBitcoin>(guardandoJson); // tranformando o json em algpo que o C# entenda

            return objDesserializado.bitcoin.brl; // retornando o objeto com a moeda bitcoin e o brl que é a moeda brasileira
        }

        class RespostaBitcoin // é um objeto com a prioridade bitcoin
        {
            public Moeda bitcoin { get; set; }
        }

        class Moeda // é o que possui dentro do bitcoin
        {
            public double brl { get; set; }
        }

        private void SalvarCotacao(double valor)
        {
            using (SqlConnection conexao = new SqlConnection(connectionString)) // criando conexão com o banco
            {
                conexao.Open(); // abrindo a conexão
                string sql = "INSERT INTO Cotacoes (Preco, Variacao) VALUES (@preco, NULL)"; // o que eu quero do banco naquele momento

                using (SqlCommand comando = new SqlCommand(sql, conexao)) // cria um chamado sql que vai ser executado no banco
                {
                    comando.Parameters.AddWithValue("@preco", valor); // usado o valor preço como parâmetro por segurança 
                    comando.ExecuteNonQuery(); // executa o comando no banco, não retorna dados
                }
            }
        }

        private void dgvHistorico_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ExbirHistorico()
        {
            using (SqlConnection conexao = new SqlConnection(connectionString)) // // criando conexão com o banco
            {
                conexao.Open(); // abrindo a conexão
                string sql = "SELECT DataHora, Preco, Variacao FROM Cotacoes ORDER BY DataHora DESC"; // o que eu quero do banco naquele momento

                using (SqlDataAdapter adapta = new SqlDataAdapter(sql, conexao)) // e a ponte do banco e a tabela c#, busca dos dados e coloca dentro da DGV
                {
                    DataTable tabela = new DataTable(); // cria a memória da tabela
                    adapta.Fill(tabela); // executa o select e preenche

                    dgvHistorico.DataSource = tabela; // onde eu ligo a tabela com o DGV
                }
            }     
        }
    }
}
