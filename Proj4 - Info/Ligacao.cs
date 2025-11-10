using System;

namespace Proj4
{
    public class Ligacao : IComparable<Ligacao>
    {
        string origem, destino;
        int distancia;

        public Ligacao(string origem, string destino, int distancia)
        {
            this.origem = origem;
            this.destino = destino;
            this.distancia = distancia;
        }

        public int CompareTo(Ligacao other)
        {
            return (origem + destino).CompareTo(other.origem + other.destino);
        }
    }
}
