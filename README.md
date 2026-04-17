# AVALIA-O-QA-JR
Repositório criado afim de entregar todo o material solicitado para a vaga de QA-JR

No exercício 1 eu até tentei utilizar o chat para me ajudar, não que criar os casos fosse um problema mas sim para que ele me mostrasse um sistema que tivesse as expecificações pedidas até baixei no meu celular o sistema Minha agenda para tentar fazer com ele mas não seguia as mesmas coisas, foi usado o google agenda e fiz todos os casos de fluxos de sucesso, os negativos o google não segue algumas especificações como agendar um horário de término menor que o do começo e isso seria um teste a ser feito por exemplo, por esse motivo foi feito apenas os testes com o fluxo de sucesso e não foi usado IA em momento algúm.

Nos exercícios 2 e 5.2 não foi usado nenhum tipo de IA, nem para corrigir os textos, nem para pesquisas e respostas. Tudo o que foi escrito nas perguntas dejesadas foi através de pesquisa principalmente com vídeos no Youtube, o que não foi feito foi realmente por falta de conhecimento.

No exercício 3 eu usei o chat gpt apenas para me guiar no que testar mas as questões foram todas feitas por mim sem a influência dele

Para o exercício 4 foi usado apenas o chatGPT, ele me acompanhou em todo o processo, fui bem específico no prompt pedindo para ele apenas me auxiliar, tentar me dar o caminho mas não as respostas, vou deixar os dois chats que usei para desenvolver o trabalho. Lá mostram históricos das minhas tentativas e conversas com ele sobre o que fazer e o roteiro que seguiriamos, ele foi o meu tutor nesse projeto, vão ter partes em que ele sim fez uma parte da lógica ou alterações, mas como no chat mesmo mostra eu pedi explicações pra ele sobre o que significava cada linha e anotei nos comentários de alguns códigos. Deixei de fazer a parte de cotações mostrar no DataGridView pois se continuasse ai o código seria mais de IA do que meu, fui até onde consegui e mais um pouco.

Para rodar o projeto você vai precisar dar um git clone nesse repositório, ter baixado em seu computador o Visual Studio com os arquivos necessários para rodar uma aplicação C#, para esse repositório em específico e esse projeto não é necessário baixar nenhuma dependência, clocando o repositório e tendo o Visual Studio com as configurações do C# ele irá rodar normalmente. Para criar o banco de dados você deve ir no menu na parte de cima do Visual Studio e clicar em Exbir, depois localizar algo parecido com: Pesquisador de objetos do SQL Server e clicar, irá aparecer SQL Server com um simbolo de um banco de dados ao lado você irá expandir e achará algo como (localdb)\............ apertar o botão direito e nova consulta, lá você irá colar o seguinte código
CREATE DATABASE CoinGeckoDb;
GO

USE CoinGeckoDb;
GO

CREATE TABLE Cotacoes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DataHora DATETIME NOT NULL DEFAULT GETDATE(),
    Preco DECIMAL(18,2) NOT NULL,
    Variacao DECIMAL(8,4) NULL
);
GO

Com ele você cria sempre o que está acima do GO, pois com o Script que foi passado dava um erro de criação, após isso pressione as teclas: CTRL + SHIFT + E ou apenas procure pelo botão verde que parece um play logo a cima da linha 1.

Endpoint: 
https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=brl

No exercício 5 eu até tenho uma leve lembrança de como cria um VIEW, mas como ela exigia criar a variação do mesmo que é o exercício anterior, por questão de lógica se eu entregasse um pronto e o outro não eu claramente copiaria tudo da IA, não quis fazer isso, então eu até mandei ao chat ele me deu a rsposta direta e quando eu vi realmente achei que ficaria muito descarado e não realizei o exercício


 Segue o link do chat principal, que me ajudou a fazer o exercício 4: [https://chatgpt.com/share/69df82a2-68e0-83e9-913c-eee6b3455455].
 Segue o link do segundo chat que usei: https://chatgpt.com/share/69e17cc2-c050-83e9-9396-b22161c7e50a
