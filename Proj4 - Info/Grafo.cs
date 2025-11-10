using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace apGrafoDaSilva
{
    class Grafo
    {
        private const int NUM_VERTICES = 20;
        private Vertice[] vertices;
        private int[,] adjMatrix;
        int numVerts;
        DataGridView dgv;

        /// DIJKSTRA
        DistOriginal[] percurso;
        int infinity = int.MaxValue;
        int verticeAtual; // global que indica o vértice atualmente sendo visitado
        int doInicioAteAtual; // global usada para ajustar menor caminho com Djikstra
        int nTree;

        public Grafo(DataGridView dgv)
        {
            this.dgv = dgv;
            vertices = new Vertice[NUM_VERTICES];
            adjMatrix = new int[NUM_VERTICES, NUM_VERTICES];
            numVerts = 0;
            nTree = 0;
            for (int j = 0; j < NUM_VERTICES; j++) // zera toda a matriz
                for (int k = 0; k < NUM_VERTICES; k++)
                    adjMatrix[j, k] = infinity; // distância tão grande que não existe
            percurso = new DistOriginal[NUM_VERTICES];
        }
        public void NovoVertice(string label)
        {
            vertices[numVerts] = new Vertice(label);
            numVerts++;
            if (dgv != null) // se foi passado como parâmetro um dataGridView para exibição
            { // se realiza o seu ajuste para a quantidade de vértices
                dgv.RowCount = numVerts + 1;
                dgv.ColumnCount = numVerts + 1;
                dgv.Columns[numVerts].Width = 45;
            }
        }
        public void NovaAresta(int start, int eend)
        {
            adjMatrix[start, eend] = 1; // adjMatrix[eend, start] = 1; ISSO GERA CICLOS!!!
        }
        public void NovaAresta(int origem, int destino, int peso)
        {
            adjMatrix[origem, destino] = peso; // sobrecarga do método anterior
        }
        public void ExibirVertice(int v)
        {
            Console.Write(vertices[v].rotulo + " ");
        }
        public void ExibirVertice(int v, TextBox txt)
        {
            txt.Text += vertices[v].rotulo + " ";
        }
        public int SemSucessores() // encontra e retorna a linha de um vértice sem sucessores
        {
            bool temAresta;
            for (int linha = 0; linha < numVerts; linha++)
            {
                temAresta = false;
                for (int col = 0; col < numVerts; col++)
                    if (adjMatrix[linha, col] > 0 && adjMatrix[linha, col] != infinity)
                    {
                        temAresta = true;
                        break;
                    }
                if (!temAresta)
                    return linha;
            }
            return -1;
        }
        public void RemoverVertice(int vert)
        {
            if (dgv != null)
            {
                MessageBox.Show($"Matriz de Adjacências antes de remover vértice {vert}");
                ExibirAdjacencias();
            }
            if (vert != numVerts - 1)
            {
                for (int j = vert; j < numVerts - 1; j++)// remove vértice do vetor
                    vertices[j] = vertices[j + 1];
                // remove vértice da matriz
                for (int row = vert; row < numVerts; row++)
                    MoverLinhas(row, numVerts - 1);
                for (int col = vert; col < numVerts; col++)
                    MoverColunas(col, numVerts - 1);
            }
            numVerts--;
            if (dgv != null)
            {
                MessageBox.Show($"Matriz de Adjacências após remover vértice {vert}");
                ExibirAdjacencias();
                MessageBox.Show("Retornando à ordenação");
            }
        }
        private void MoverLinhas(int row, int length)
        {
            if (row != numVerts - 1)
                for (int col = 0; col < length; col++)
                    adjMatrix[row, col] = adjMatrix[row + 1, col]; // desloca para excluir
        }
        private void MoverColunas(int col, int length)
        {
            if (col != numVerts - 1)
                for (int row = 0; row < length; row++)
                    adjMatrix[row, col] = adjMatrix[row, col + 1]; // desloca para excluir
        }
        public void ExibirAdjacencias()
        {
            dgv.RowCount = numVerts + 1;
            dgv.ColumnCount = numVerts + 1;
            for (int j = 0; j < numVerts; j++)
            {
                dgv.Rows[j + 1].Cells[0].Value = vertices[j].rotulo;
                dgv.Rows[0].Cells[j + 1].Value = vertices[j].rotulo;
                for (int k = 0; k < numVerts; k++)
                    dgv.Rows[j + 1].Cells[k + 1].Value = Convert.ToString(adjMatrix[j, k]);
            }
        }
        public String OrdenacaoTopologica()
        {
            Stack<String> gPilha = new Stack<String>(); //guarda a sequência de vértices
            int origVerts = numVerts;
            while (numVerts > 0)
            {
                int currVertex = SemSucessores();
                if (currVertex == -1)
                    return "Erro: grafo possui ciclos.";
                gPilha.Push(vertices[currVertex].rotulo); // empilha vértice
                RemoverVertice(currVertex);
            }
            String resultado = "Sequência da Ordenação Topológica: ";
            while (gPilha.Count > 0)
                resultado += gPilha.Pop() + " "; // desempilha para exibir
            return resultado;
        }
        private int ObterVerticeAdjacenteNaoVisitado(int v)
        {
            for (int j = 0; j <= numVerts - 1; j++)
                if ((adjMatrix[v, j] == 1) && (!vertices[j].foiVisitado))
                    return j;
            return -1;
        }
        public void PercursoEmProfundidade(TextBox txt)
        {
            txt.Clear();
            Stack<int> gPilha = new Stack<int>(); // para guardar a sequência de vértices
            vertices[0].foiVisitado = true;
            ExibirVertice(0, txt);
            gPilha.Push(0);

            int v;
            while (gPilha.Count > 0)
            {
                v = ObterVerticeAdjacenteNaoVisitado(gPilha.Peek());
                if (v == -1)
                    gPilha.Pop();
                else
                {
                    vertices[v].foiVisitado = true;
                    ExibirVertice(v, txt);
                    gPilha.Push(v);
                }
            }
            for (int j = 0; j <= numVerts - 1; j++)
                vertices[j].foiVisitado = false;
        }
        void ProcessarNo(int i)
        {
            Console.Write(vertices[i].rotulo);
        }
        public void PercursoEmProfundidadeRec(int[,] adjMatrix, int numVerts, int part)
        {
            int i;
            ProcessarNo(part);
            vertices[part].foiVisitado = true;
            for (i = 0; i < numVerts; ++i)
                if (adjMatrix[part, i] == 1 && !vertices[i].foiVisitado)
                    PercursoEmProfundidadeRec(adjMatrix, numVerts, i);
        }
        public void percursoPorLargura(TextBox txt)
        {
            txt.Clear();
            Queue<int> gQueue = new Queue<int>();
            vertices[0].foiVisitado = true;
            ExibirVertice(0, txt);
            gQueue.Enqueue(0);
            int vert1, vert2;
            while (gQueue.Count > 0)
            {
                vert1 = gQueue.Dequeue();
                vert2 = ObterVerticeAdjacenteNaoVisitado(vert1);
                while (vert2 != -1)
                {
                    vertices[vert2].foiVisitado = true;
                    ExibirVertice(vert2, txt);
                    gQueue.Enqueue(vert2);
                    vert2 = ObterVerticeAdjacenteNaoVisitado(vert1);
                }
            }
            for (int i = 0; i < numVerts; i++)
                vertices[i].foiVisitado = false;
        }
        public void ArvoreGeradoraMinima(int primeiro, TextBox txt)
        {
            txt.Clear();
            Stack<int> gPilha = new Stack<int>(); // para guardar a sequência de vértices
            vertices[primeiro].foiVisitado = true;
            gPilha.Push(primeiro);
            int currVertex, ver;
            while (gPilha.Count > 0)
            {
                currVertex = gPilha.Peek();
                ver = ObterVerticeAdjacenteNaoVisitado(currVertex);
                if (ver == -1)
                    gPilha.Pop();
                else
                {
                    vertices[ver].foiVisitado = true;
                    gPilha.Push(ver);
                    ExibirVertice(currVertex, txt);
                    txt.Text += "-->";
                    ExibirVertice(ver, txt);
                    txt.Text += " ";
                }
            }
            for (int j = 0; j <= numVerts - 1; j++)
                vertices[j].foiVisitado = false;
        }
        public string Caminho(int inicioDoPercurso, int finalDoPercurso, ListBox lista)
        {
            for (int j = 0; j < numVerts; j++)
                vertices[j].foiVisitado = false;
            vertices[inicioDoPercurso].foiVisitado = true;
            for (int j = 0; j < numVerts; j++)
            {
                // anotamos no vetor percurso a distância entre o inicioDoPercurso e cada vértice
                // se não há ligação direta, o valor da distância será infinity
                int tempDist = adjMatrix[inicioDoPercurso, j];
                percurso[j] = new DistOriginal(inicioDoPercurso, tempDist);
            }
            for (int nTree = 0; nTree < numVerts; nTree++)
            {
                // Procuramos a saída não visitada do vértice inicioDoPercurso com a menor distância
                int indiceDoMenor = ObterMenor();
                // e anotamos essa menor distância
                int distanciaMinima = percurso[indiceDoMenor].distancia;
                // o vértice com a menor distância passa a ser o vértice atual
                // para compararmos com a distância calculada em AjustarMenorCaminho()
                verticeAtual = indiceDoMenor;
                doInicioAteAtual = percurso[indiceDoMenor].distancia;
                // visitamos o vértice com a menor distância desde o inicioDoPercurso
                vertices[verticeAtual].foiVisitado = true;
                AjustarMenorCaminho(lista);
            }
            return ExibirPercursos(inicioDoPercurso, finalDoPercurso, lista);
        }
        public int ObterMenor()
        {
            int distanciaMinima = infinity;
            int indiceDaMinima = 0;
            for (int j = 0; j < numVerts; j++)
                if (!(vertices[j].foiVisitado) && (percurso[j].distancia < distanciaMinima))
                {
                    distanciaMinima = percurso[j].distancia;
                    indiceDaMinima = j;
                }
            return indiceDaMinima;
        }
        public void AjustarMenorCaminho(ListBox lista)
        {
            for (int coluna = 0; coluna < numVerts; coluna++)
                if (!vertices[coluna].foiVisitado) // para cada vértice ainda não visitado
                {
                    // acessamos a distância desde o vértice atual (pode ser infinity)
                    int atualAteMargem = adjMatrix[verticeAtual, coluna];
                    // calculamos a distância desde inicioDoPercurso passando por vertice atual
                    // até esta saída
                    int doInicioAteMargem = doInicioAteAtual + atualAteMargem;
                    // quando encontra uma distância menor, marca o vértice a partir do
                    // qual chegamos no vértice de índice coluna, e a soma da distância
                    // percorrida para nele chegar
                    int distanciaDoCaminho = percurso[coluna].distancia;
                    if (doInicioAteMargem < distanciaDoCaminho)
                    {
                        percurso[coluna].verticePai = verticeAtual;
                        percurso[coluna].distancia = doInicioAteMargem;
                        ExibirTabela(lista);
                    }
                }
            lista.Items.Add("==================Caminho ajustado==============");
            lista.Items.Add(" ");
        }
        public void ExibirTabela(ListBox lista)
        {
            string dist = "";
            lista.Items.Add("Vértice\tVisitado?\tPeso\tVindo de");
            for (int i = 0; i < numVerts; i++)
            {
                if (percurso[i].distancia == infinity)
                    dist = "inf";
                else
                    dist = Convert.ToString(percurso[i].distancia);
                lista.Items.Add(vertices[i].rotulo + "\t" + vertices[i].foiVisitado +
                "\t\t" + dist + "\t" + vertices[percurso[i].verticePai].rotulo);
            }
            lista.Items.Add("-----------------------------------------------------");
        }
        public string ExibirPercursos(int inicioDoPercurso, int finalDoPercurso, ListBox lista)
        {
            string resultado = "";
            for (int j = 0; j < numVerts; j++)
            {
                resultado += vertices[j].rotulo + "=";
                if (percurso[j].distancia == infinity)
                    resultado += "inf";
                else
                    resultado += percurso[j].distancia + " ";
                string pai = vertices[percurso[j].verticePai].rotulo;
                resultado += "(" + pai + ") ";
            }
            lista.Items.Add(resultado);
            lista.Items.Add(" ");
            lista.Items.Add(" ");
            lista.Items.Add("Caminho entre " + vertices[inicioDoPercurso].rotulo +
            " e " + vertices[finalDoPercurso].rotulo);
            lista.Items.Add(" ");
            int onde = finalDoPercurso;
            Stack<string> pilha = new Stack<string>();
            int cont = 0;
            while (onde != inicioDoPercurso)
            {
                onde = percurso[onde].verticePai;
                pilha.Push(vertices[onde].rotulo);
                cont++;
            }
            resultado = "";
            while (pilha.Count != 0)
            {
                resultado += pilha.Pop();
                if (pilha.Count != 0)
                    resultado += " --> ";
            }
            if ((cont == 1) && (percurso[finalDoPercurso].distancia == infinity))
                resultado = "Não há caminho";
            else
                resultado += " --> " + vertices[finalDoPercurso].rotulo;
            return resultado;
        }

    }
}
