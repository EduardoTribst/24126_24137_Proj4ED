using apGrafoDaSilva;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Proj4
{
    public partial class Form1 : Form
    {
        ArvoreAVL<Cidade> arvore;
        Grafo grafoCaminhos;
        List<string> caminhoDestacado;
        bool processoInclusaoCidade;

        public Form1()
        {
            InitializeComponent();
            arvore = new ArvoreAVL<Cidade>();
            grafoCaminhos = new Grafo();
            caminhoDestacado = null;
            processoInclusaoCidade = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LerArquivos();
        }

        private void txtNomeCidade_Leave(object sender, EventArgs e)
        {
            if (processoInclusaoCidade)
            {
                string nomeCidade = RemoverAcentos(txtNomeCidade.Text.Trim());

                txtNomeCidade.Text = nomeCidade;

                if (nomeCidade == "")
                {
                    MessageBox.Show("Nome da cidade não pode ser vazio.");
                    TerminarInclusao();
                    return;
                }

                if (arvore.Existe(new Cidade(nomeCidade, 0, 0)))
                {
                    MessageBox.Show("Cidade já existe.");
                    TerminarInclusao();
                }
                else
                {
                    MessageBox.Show("Clique no ponto do mapa onde a cidade será incluída.");
                }
            }
        }

        private void LerArquivos()
        {
            dlgAbrir.Title = "Selecione o arquivo binário de cidades";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoCidades = dlgAbrir.FileName;

                arvore.LerArquivoDeRegistros(nomeArquivoCidades);
                arvore.Desenhar(pnlArvore);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }

            dlgAbrir.Title = "Selecione o arquivo texto de ligações";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoLigacoes = dlgAbrir.FileName;
                LerArquivoLigacoes(nomeArquivoLigacoes);
            }
            else
            {
                MessageBox.Show("Selecione um arquivo");
                return;
            }
        }

        private void LerArquivoLigacoes(string nomeArquivo)
        {
            StreamReader arquivoLeitura = new StreamReader(nomeArquivo);

            string linha;
            string cidadeAtual = null;
            Cidade cidadeOrigemObj = null;

            while (!arquivoLeitura.EndOfStream)
            {
                linha = arquivoLeitura.ReadLine();

                if (linha == "")
                    continue;

                var valores = linha.Split(';');

                string cidadeOrigem = RemoverAcentos(valores[0]);
                string cidadeDestino = RemoverAcentos(valores[1]);
                int distancia = Convert.ToInt32(valores[2]);

                if (cidadeAtual != cidadeOrigem)
                {
                    if (arvore.Existe(new Cidade(cidadeOrigem, 0, 0)))
                        cidadeOrigemObj = arvore.Atual.Info;
                    else
                        cidadeOrigemObj = null;

                    cidadeAtual = cidadeOrigem;
                }

                if (cidadeOrigemObj == null)
                    continue;

                Cidade cidadeDestinoObj;

                if (arvore.Existe(new Cidade(cidadeDestino, 0, 0)))
                    cidadeDestinoObj = arvore.Atual.Info;
                else
                    cidadeDestinoObj = null;

                if (cidadeDestinoObj == null)
                    continue;

                bool ok1 = cidadeOrigemObj.CriarLigacao(cidadeDestino, distancia);
                bool ok2 = cidadeDestinoObj.CriarLigacao(cidadeOrigem, distancia);

                if (!ok1 || !ok2)
                {
                    MessageBox.Show("Erro ao adicionar ligação: " + cidadeOrigem + " - " + cidadeDestino);
                    continue;
                }

                int idxOrigem = grafoCaminhos.ObterIndiceVertice(cidadeOrigem);
                if (idxOrigem == -1)
                    grafoCaminhos.NovoVertice(cidadeOrigem);

                int idxDestino = grafoCaminhos.ObterIndiceVertice(cidadeDestino);
                if (idxDestino == -1)
                    grafoCaminhos.NovoVertice(cidadeDestino);

                idxOrigem = grafoCaminhos.ObterIndiceVertice(cidadeOrigem);
                idxDestino = grafoCaminhos.ObterIndiceVertice(cidadeDestino);

                grafoCaminhos.NovaAresta(idxOrigem, idxDestino, distancia, bidirecional: true);
            }

            arquivoLeitura.Close();
        }

        void SalvarArquivos()
        {
            dlgAbrir.Title = "Selecione o arquivo binário de cidades para salvar";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoCidades = dlgAbrir.FileName;
                arvore.GravarArquivoDeRegistros(nomeArquivoCidades);
            }

            dlgAbrir.Title = "Selecione o arquivo texto de ligações para salvar";
            if (dlgAbrir.ShowDialog() == DialogResult.OK)
            {
                string nomeArquivoLigacoes = dlgAbrir.FileName;
                SalvarArquivoLigacoes(nomeArquivoLigacoes);
            }

            void SalvarArquivoLigacoes(string nomeArquivo)
            {
                StreamWriter arquivoEscrita = new StreamWriter(nomeArquivo);
                List<Cidade> listaCidades = new List<Cidade>();
                arvore.VisitarEmOrdem(ref listaCidades);

                foreach (Cidade cidade in listaCidades)
                {
                    foreach (Ligacao lig in cidade.ListarLigacoes())
                    {
                        string cidade1 = cidade.Nome.Trim();
                        string cidade2 = lig.Destino.Trim();

                        // so salva se for a primeira cidade em ordem alfabetica
                        if (string.Compare(cidade1, cidade2, StringComparison.Ordinal) < 0)
                        {
                            arquivoEscrita.WriteLine($"{cidade1};{cidade2};{lig.Distancia}");
                        }
                    }
                }

                arquivoEscrita.Close();
            }

        }

        private void pbMapa_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            double larguraAtual = pbMapa.Width;
            double alturaAtual = pbMapa.Height;

            List<Cidade> listaCidades = new List<Cidade>();
            arvore.VisitarEmOrdem(ref listaCidades);

            Pen corCaminhos = new Pen(Color.DarkGray, 2);
            Brush corCidade = new SolidBrush(Color.Red);
            Font fonte = new Font("Arial", 10, FontStyle.Bold);

            foreach (Cidade cidade in listaCidades)
            {
                PointF origem = Converter(cidade.X, cidade.Y, larguraAtual, alturaAtual);

                foreach (Ligacao ligacao in cidade.ListarLigacoes())
                {
                    arvore.Existe(new Cidade(ligacao.Destino, 0, 0));
                    if (arvore.Atual == null)
                        continue;

                    PointF destino = Converter(arvore.Atual.Info.X, arvore.Atual.Info.Y, larguraAtual, alturaAtual);
                    g.DrawLine(corCaminhos, origem, destino);
                }

                g.FillEllipse(corCidade, origem.X - 4, origem.Y - 4, 8, 8);
                g.DrawString(cidade.Nome, fonte, Brushes.Black, origem.X + 6, origem.Y - 6);
            }

            if (caminhoDestacado != null && caminhoDestacado.Count > 1)
            {
                Pen corDestaque = new Pen(Color.Blue, 4);

                for (int i = 0; i < caminhoDestacado.Count - 1; i++)
                {
                    string nomeA = caminhoDestacado[i];
                    string nomeB = caminhoDestacado[i + 1];

                    arvore.Existe(new Cidade(nomeA, 0, 0));
                    Cidade cidA = arvore.Atual.Info;

                    arvore.Existe(new Cidade(nomeB, 0, 0));
                    Cidade cidB = arvore.Atual.Info;

                    if (cidA == null || cidB == null)
                        continue;

                    PointF p1 = Converter(cidA.X, cidA.Y, larguraAtual, alturaAtual);
                    PointF p2 = Converter(cidB.X, cidB.Y, larguraAtual, alturaAtual);

                    g.DrawLine(corDestaque, p1, p2);
                }
            }

            PointF Converter(double x, double y, double largAtual, double altAtual)
            {
                float novoX = (float)(x * largAtual);
                float novoY = (float)(y * altAtual);
                return new PointF(novoX, novoY);
            }
        }

        private void btnBuscarCaminho_Click(object sender, EventArgs e)
        {
            string origem = RemoverAcentos(txtNomeCidade.Text.Trim());
            string destino = RemoverAcentos(cbxCidadeDestino.Text.Trim());

            (List<(String, int)>, int) resultadoCaminho =
                grafoCaminhos.CaminhosComDistancias(origem, destino);

            int distTotal = resultadoCaminho.Item2;
            List<(String, int)> caminho = resultadoCaminho.Item1;

            dgvRotas.Rows.Clear();

            foreach (var item in caminho)
                dgvRotas.Rows.Add(item.Item1, item.Item2);

            lbDistanciaTotal.Text = $"Distância total: {distTotal} km";

            caminhoDestacado = caminho.Select(c => c.Item1).ToList();
            pbMapa.Invalidate();
        }

        private void btnIncluirCidade_Click(object sender, EventArgs e)
        {
            if (!processoInclusaoCidade)
                IniciarInclusao();
            else
                TerminarInclusao();
        }

        private void IniciarInclusao()
        {
            processoInclusaoCidade = true;

            MessageBox.Show("Processo de inclusão iniciado.\n" +
                "Digite o nome da cidade e clique no mapa para definir a localização.\n" +
                "Para cancelar, clique novamente no botão.");

            udX.Value = 0;
            udY.Value = 0;
            txtNomeCidade.Text = "";

            btnBuscarCidade.Enabled = false;
            btnAlterarCidade.Enabled = false;
            btnExcluirCidade.Enabled = false;
        }

        private void TerminarInclusao()
        {
            processoInclusaoCidade = false;
            pbMapa.Cursor = Cursors.Default;

            btnBuscarCidade.Enabled = true;
            btnAlterarCidade.Enabled = true;
            btnExcluirCidade.Enabled = true;
        }

        private string RemoverAcentos(string texto)
        {
            string novoTexto = "";
            string textoNormalize = texto.Normalize(NormalizationForm.FormD);

            foreach (var letra in textoNormalize)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                    novoTexto += letra;
            }

            return novoTexto.Normalize(NormalizationForm.FormC).Trim();
        }

        private void pbMapa_MouseClick(object sender, MouseEventArgs e)
        {
            double xProporcional = (double)e.X / pbMapa.Width;
            double yProporcional = (double)e.Y / pbMapa.Height;

            udX.Value = (decimal)xProporcional;
            udY.Value = (decimal)yProporcional;

            if (processoInclusaoCidade)
            {
                if (txtNomeCidade.Text.Trim() == "")
                {
                    MessageBox.Show("Digite o nome da cidade antes de clicar no mapa.");
                    return;
                }

                string nomeCidade = RemoverAcentos(txtNomeCidade.Text.Trim());
                txtNomeCidade.Text = nomeCidade;

                if (MessageBox.Show($"Confirma a inclusão da cidade {nomeCidade}?",
                    "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Cidade novaCidade = new Cidade(nomeCidade, xProporcional, yProporcional);
                    arvore.InserirBalanceado(novaCidade);
                    grafoCaminhos.NovoVertice(nomeCidade);

                    TerminarInclusao();
                    pbMapa.Invalidate();
                    pnlArvore.Invalidate();
                }
            }
        }

        private void btnBuscarCidade_Click(object sender, EventArgs e)
        {
            string nomeCidade = RemoverAcentos(txtNomeCidade.Text.Trim());
            txtNomeCidade.Text = nomeCidade;

            if (nomeCidade != "")
            {
                if (arvore.Existe(new Cidade(nomeCidade, 0, 0)))
                {
                    udX.Value = (decimal)arvore.Atual.Info.X;
                    udY.Value = (decimal)arvore.Atual.Info.Y;

                    dgvLigacoes.Rows.Clear();

                    foreach (Ligacao ligacao in arvore.Atual.Info.ListarLigacoes())
                    {
                        dgvLigacoes.Rows.Add(ligacao.Destino, ligacao.Distancia);
                    }
                }
                else
                {
                    MessageBox.Show("Cidade não encontrada.");
                }
            }
            else
            {
                MessageBox.Show("Digite o nome da cidade para buscar.");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SalvarArquivos();
        }

        private void cbxCidadeDestino_Click(object sender, EventArgs e)
        {
            List<Cidade> listaCidades = new List<Cidade>();
            arvore.VisitarEmOrdem(ref listaCidades);
            cbxCidadeDestino.Items.Clear();
            foreach (Cidade cidade in listaCidades)
            {
                cbxCidadeDestino.Items.Add(cidade.Nome);
            }
            cbxCidadeDestino.SelectedItem = cbxCidadeDestino.Items[0];

        }

        private void btnAlterarCidade_Click(object sender, EventArgs e)
        {
            // altera apenas as coordenadas da cidade
            string nomeCidade = RemoverAcentos(txtNomeCidade.Text.Trim());
            if (nomeCidade != null)
            {
                if (arvore.Existe(new Cidade(nomeCidade, 0, 0)))
                {
                    Cidade cidade = arvore.Atual.Info;
                    cidade.X = (double)udX.Value;
                    cidade.Y = (double)udY.Value;
                    pbMapa.Invalidate();
                }
                else
                {
                    MessageBox.Show("Cidade não encontrada.");
                }
            }
        }

        private void btnExcluirCidade_Click(object sender, EventArgs e)
        {
            // verifica se tem caminhos
            if (arvore.Existe(new Cidade(txtNomeCidade.Text, 0, 0)))
            {
                Cidade cidadeAExcluir = arvore.Atual.Info;

                // verifica se existe caminhos -> sim: pode
                int qntsLigacoes = cidadeAExcluir.ListarLigacoes().Count;

                if (qntsLigacoes == 0)
                {
                    if (MessageBox.Show($"Confirma a exclusão da cidade {cidadeAExcluir.Nome.Trim()}?",
                    "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        // exclui na arvore
                        arvore.Excluir(cidadeAExcluir);
                        // excliui no grafo
                        grafoCaminhos.RemoverVertice(cidadeAExcluir.Nome);

                        pbMapa.Invalidate();
                        pnlArvore.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Exclusão cancelada");
                    }
                }
                else
                {
                    MessageBox.Show($"A cidade {cidadeAExcluir.Nome.Trim()} possui {qntsLigacoes} ligações.");
                }
            }
            else
            {
                MessageBox.Show("Cidade não existe.");
            }
        }

        private void pnlArvore_Paint(object sender, PaintEventArgs e)
        {
            arvore.Desenhar(pnlArvore);
        }

        private void btnIncluirCaminho_Click(object sender, EventArgs e)
        {

            var cidadeOrigem = txtNomeCidade.Text;
            var cidadeDestino = txtNovoDestino.Text;
            var distancia = (int)nuDistancia.Value;
            if (cidadeDestino != "" && nuDistancia.Value != 0)
            {
                arvore.Existe(new Cidade(RemoverAcentos(cidadeDestino), 0, 0));
                var cidadeNovoDestino = arvore.Atual;
                if (arvore.Existe(new Cidade(RemoverAcentos(cidadeOrigem), 0, 0)) && cidadeNovoDestino != null)
                {
                    arvore.Atual.Info.CriarLigacao(cidadeDestino, distancia);
                    cidadeNovoDestino.Info.CriarLigacao(cidadeOrigem, distancia);

                    grafoCaminhos.NovaAresta(
                        grafoCaminhos.ObterIndiceVertice(RemoverAcentos(cidadeOrigem)),
                        grafoCaminhos.ObterIndiceVertice(RemoverAcentos(cidadeDestino)),
                        distancia,
                        bidirecional: true);

                    MessageBox.Show("Caminho adicionado com sucesso");

                    pbMapa.Invalidate();

                }
                else
                {
                    MessageBox.Show("Cidade de origem ou destino não existe.");
                }
            }
        }

        private void btnExcluirCaminho_Click(object sender, EventArgs e)
        {
            var cidadeOrigem = txtNomeCidade.Text;
            var cidadeDestino = txtNovoDestino.Text;

            if (!string.IsNullOrWhiteSpace(cidadeDestino))
            {
                string origemSemAcento = RemoverAcentos(cidadeOrigem);
                string destinoSemAcento = RemoverAcentos(cidadeDestino);

                // Localiza destino
                if (!arvore.Existe(new Cidade(destinoSemAcento, 0, 0)))
                {
                    MessageBox.Show("Cidade de destino não existe.");
                    return;
                }

                var cidadeNovoDestino = arvore.Atual.Info;

                // Localiza origem
                if (!arvore.Existe(new Cidade(origemSemAcento, 0, 0)))
                {
                    MessageBox.Show("Cidade de origem não existe.");
                    return;
                }

                var cidadeOrigemNoArvore = arvore.Atual.Info;

                // Exclui ligações nos dois lados
                cidadeOrigemNoArvore.ExcluirLigacao(destinoSemAcento);
                cidadeNovoDestino.ExcluirLigacao(origemSemAcento);

                // Remove no grafo
                grafoCaminhos.RemoverAresta(
                    grafoCaminhos.ObterIndiceVertice(origemSemAcento),
                    grafoCaminhos.ObterIndiceVertice(destinoSemAcento),
                    bidirecional: true
                );

                MessageBox.Show("Caminho removido com sucesso");

                pbMapa.Invalidate();
            }
        }

        private void timerRelogio_Tick(object sender, EventArgs e)
        {
            lbRelogio.Text = DateTime.Now.ToString("HH:mm:ss");

            if (DateTime.Now.Day == 25 && DateTime.Now.Month == 12)
            {
                lbMensagem.Text = "Feliz Natal!";
            }
            else if (DateTime.Now.Day == 1 && DateTime.Now.Month == 1)
            {
                lbMensagem.Text = "Feliz Ano Novo!";
            }
            else
            {
                lbMensagem.Text = "Ainda não é Natal nem Ano Novo";
            }
        }
    }
}
