using Proj4;
using System;
using System.Collections.Generic;
using System.Text;

namespace apGrafoDaSilva
{
    class Grafo
    {
        private const int NUM_VERTICES = 200;
        private Vertice[] vertices;
        private int[,] adjMatrix;
        private int numVerts;

        /// DIJKSTRA
        private DistOriginal[] percurso;
        private int infinity = int.MaxValue;
        private int verticeAtual;
        private int doInicioAteAtual;

        public Grafo()
        {
            vertices = new Vertice[NUM_VERTICES];
            adjMatrix = new int[NUM_VERTICES, NUM_VERTICES];
            numVerts = 0;

            for (int j = 0; j < NUM_VERTICES; j++)
                for (int k = 0; k < NUM_VERTICES; k++)
                    adjMatrix[j, k] = infinity;

            percurso = new DistOriginal[NUM_VERTICES];
        }

        public void NovoVertice(string rotulo)
        {
            vertices[numVerts] = new Vertice(rotulo);
            numVerts++;
        }

        public void NovaAresta(int origem, int destino)
        {
            adjMatrix[origem, destino] = 1;
        }

        public void NovaAresta(int origem, int destino, int peso)
        {
            adjMatrix[origem, destino] = peso;
        }

        private void MoverLinhas(int row, int length)
        {
            if (row != numVerts - 1)
                for (int col = 0; col < length; col++)
                    adjMatrix[row, col] = adjMatrix[row + 1, col];
        }

        private void MoverColunas(int col, int length)
        {
            if (col != numVerts - 1)
                for (int row = 0; row < length; row++)
                    adjMatrix[row, col] = adjMatrix[row, col + 1];
        }

        public void RemoverVertice(int vert)
        {
            if (vert != numVerts - 1)
            {
                for (int j = vert; j < numVerts - 1; j++)
                    vertices[j] = vertices[j + 1];

                for (int row = vert; row < numVerts; row++)
                    MoverLinhas(row, numVerts - 1);

                for (int col = vert; col < numVerts; col++)
                    MoverColunas(col, numVerts - 1);
            }

            numVerts--;
        }

        public string[,] ObterAdjacencias()
        {
            string[,] tabela = new string[numVerts + 1, numVerts + 1];

            tabela[0, 0] = "";

            for (int j = 0; j < numVerts; j++)
            {
                tabela[0, j + 1] = vertices[j].rotulo;
                tabela[j + 1, 0] = vertices[j].rotulo;

                for (int k = 0; k < numVerts; k++)
                    tabela[j + 1, k + 1] =
                        adjMatrix[j, k] == infinity ? "inf" : adjMatrix[j, k].ToString();
            }

            return tabela;
        }

        private int SemSucessores()
        {
            bool temAresta;

            for (int linha = 0; linha < numVerts; linha++)
            {
                temAresta = false;

                for (int col = 0; col < numVerts; col++)
                    if (adjMatrix[linha, col] != infinity && adjMatrix[linha, col] > 0)
                    {
                        temAresta = true;
                        break;
                    }

                if (!temAresta)
                    return linha;
            }

            return -1;
        }

        public string OrdenacaoTopologica()
        {
            Stack<string> pilha = new Stack<string>();
            int total = numVerts;

            while (numVerts > 0)
            {
                int curr = SemSucessores();
                if (curr == -1)
                    return "Erro: Grafo possui ciclos.";

                pilha.Push(vertices[curr].rotulo);
                RemoverVertice(curr);
            }

            StringBuilder sb = new StringBuilder("Ordenação Topológica: ");

            while (pilha.Count > 0)
                sb.Append(pilha.Pop() + " ");

            return sb.ToString();
        }

        private int ObterAdjNaoVisitado(int v)
        {
            for (int j = 0; j < numVerts; j++)
                if (adjMatrix[v, j] == 1 && !vertices[j].foiVisitado)
                    return j;

            return -1;
        }

        public string PercursoEmProfundidade()
        {
            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            StringBuilder sb = new StringBuilder();
            Stack<int> pilha = new Stack<int>();

            vertices[0].foiVisitado = true;
            sb.Append(vertices[0].rotulo + " ");
            pilha.Push(0);

            while (pilha.Count > 0)
            {
                int v = ObterAdjNaoVisitado(pilha.Peek());

                if (v == -1)
                    pilha.Pop();
                else
                {
                    vertices[v].foiVisitado = true;
                    sb.Append(vertices[v].rotulo + " ");
                    pilha.Push(v);
                }
            }

            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            return sb.ToString();
        }

        public string PercursoPorLargura()
        {
            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            StringBuilder sb = new StringBuilder();
            Queue<int> fila = new Queue<int>();

            vertices[0].foiVisitado = true;
            sb.Append(vertices[0].rotulo + " ");
            fila.Enqueue(0);

            while (fila.Count > 0)
            {
                int v1 = fila.Dequeue();
                int v2 = ObterAdjNaoVisitado(v1);

                while (v2 != -1)
                {
                    vertices[v2].foiVisitado = true;
                    sb.Append(vertices[v2].rotulo + " ");
                    fila.Enqueue(v2);
                    v2 = ObterAdjNaoVisitado(v1);
                }
            }

            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            return sb.ToString();
        }

        public List<string> ArvoreGeradoraMinima(int primeiro)
        {
            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            List<string> caminhos = new List<string>();
            Stack<int> pilha = new Stack<int>();

            vertices[primeiro].foiVisitado = true;
            pilha.Push(primeiro);

            while (pilha.Count > 0)
            {
                int atual = pilha.Peek();
                int prox = ObterAdjNaoVisitado(atual);

                if (prox == -1)
                    pilha.Pop();
                else
                {
                    vertices[prox].foiVisitado = true;
                    pilha.Push(prox);
                    caminhos.Add($"{vertices[atual].rotulo} --> {vertices[prox].rotulo}");
                }
            }

            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            return caminhos;
        }

        private int ObterMenor()
        {
            int menor = infinity;
            int indice = 0;

            for (int j = 0; j < numVerts; j++)
                if (!vertices[j].foiVisitado &&
                    percurso[j].distancia < menor)
                {
                    menor = percurso[j].distancia;
                    indice = j;
                }

            return indice;
        }

        private void AjustarMenorCaminho()
        {
            for (int col = 0; col < numVerts; col++)
            {
                if (!vertices[col].foiVisitado)
                {
                    int atualAteCol = adjMatrix[verticeAtual, col];
                    int inicioAteCol = doInicioAteAtual + atualAteCol;

                    if (inicioAteCol < percurso[col].distancia)
                    {
                        percurso[col].verticePai = verticeAtual;
                        percurso[col].distancia = inicioAteCol;
                    }
                }
            }
        }

        public string Caminho(int inicio, int fim)
        {
            foreach (var v in vertices)
                if (v != null) v.foiVisitado = false;

            vertices[inicio].foiVisitado = true;

            for (int j = 0; j < numVerts; j++)
                percurso[j] = new DistOriginal(inicio, adjMatrix[inicio, j]);

            for (int i = 0; i < numVerts; i++)
            {
                int menor = ObterMenor();
                verticeAtual = menor;
                doInicioAteAtual = percurso[menor].distancia;

                vertices[menor].foiVisitado = true;

                AjustarMenorCaminho();
            }

            return MontarCaminho(inicio, fim);
        }

        private string MontarCaminho(int inicio, int fim)
        {
            if (percurso[fim].distancia == infinity)
                return "Não existe caminho.";

            Stack<string> caminho = new Stack<string>();
            int atual = fim;

            while (atual != inicio)
            {
                caminho.Push(vertices[atual].rotulo);
                atual = percurso[atual].verticePai;
            }

            caminho.Push(vertices[inicio].rotulo);

            return string.Join(" --> ", caminho);
        }

        public (List<(string rotulo, int distancia)>, int distanciaTotal) 
            CaminhosComDistancias(int inicio, int fim)
        {
            foreach (var v in vertices)
            {
                if (v != null) v.foiVisitado = false;
            }

            vertices[inicio].foiVisitado = true;
            for (int j = 0; j < numVerts; j++)
            {
                percurso[j] = new DistOriginal(inicio, adjMatrix[inicio, j]);
            }

            for (int i = 0; i < numVerts; i++)
            {
                int menor = ObterMenor();
                verticeAtual = menor;
                doInicioAteAtual = percurso[menor].distancia;
                vertices[menor].foiVisitado = true;
                AjustarMenorCaminho();
            }

            List<(string rotulo, int distancia)> caminhos = new List<(string rotulo, int distancia)>();
            int atual = fim;

            while (atual != inicio)
            {
                caminhos.Add((vertices[atual].rotulo, percurso[atual].distancia));
                atual = percurso[atual].verticePai;
            }
            caminhos.Add((vertices[inicio].rotulo, 0));
            caminhos.Reverse();

            return (caminhos, percurso[fim].distancia);
        }


        private int ObterIndiceVertice(string rotulo)
        {
            for (int i = 0; i < numVerts; i++)
            {
                if (vertices[i].rotulo == rotulo)
                    return i;
            }
                
            return -1;
        }
    }
}
