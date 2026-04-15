using System.Configuration;
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
            }
            catch
            {
                lbPreco.Text = "Erro ao consultar API, tente novamente mais tarde!";
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
            public Moeda bitcoin { get; set;  }
        }

        class Moeda // é o que possui dentro do bitcoin
        { 
            public double brl {  get; set; }
        }

    }
}
