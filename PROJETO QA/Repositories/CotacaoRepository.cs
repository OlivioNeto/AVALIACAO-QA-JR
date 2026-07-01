using System.Data;
using System.Data.SqlClient;

namespace PROJETO_QA.Repositories
{
    internal class CotacaoRepository
    {
        private readonly string connectionString;

        private const string InserirCotacaoSql = "INSERT INTO Cotacoes (Preco, Variacao) VALUES (@preco, @variacao)";

        private const string ObterUltimoPrecoSql = "SELECT TOP 1 Preco FROM Cotacoes ORDER BY DataHora DESC"; // busca o preço da cotação mais recente registrada

        private const string ExibirHistoricoSql = "SELECT DataHora, Preco, Variacao FROM Cotacoes ORDER BY DataHora DESC";

        public CotacaoRepository(string connectionString)
        {
            this.connectionString = connectionString;
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

        public double? SalvarCotacao(double preco)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString))
                {
                    double? ultimoPreco = ObterUltimoPreco();

                    double? variacao = null;

                    if (ultimoPreco.HasValue)
                    {
                        variacao = preco - ultimoPreco.Value;
                    }

                    conexao.Open();

                    using (SqlCommand comando = new SqlCommand(InserirCotacaoSql, conexao))
                    {
                        comando.Parameters.Add("@preco", SqlDbType.Decimal).Value = preco;

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

        public DataTable ObterHistorico()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(connectionString))
                {
                    conexao.Open();

                    using (SqlDataAdapter adaptador = new SqlDataAdapter(ExibirHistoricoSql, conexao))
                    {
                        DataTable tabela = new DataTable();
                        adaptador.Fill(tabela);

                        return tabela;
                    }
                }
            }
            catch (SqlException)
            {
                throw new Exception("Não foi possível carregar o histórico de cotações no banco de dados.");
            }
        }
    }
}
