// Eduardo 24126
// Júlio 24137

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proj4
{
    class Vertice
    {
        public bool foiVisitado;
        public string rotulo;
        private bool estaAtivo;
        public Vertice(string rotulo)
        {
            this.rotulo = rotulo;
            this.foiVisitado = false;
            this.estaAtivo = true;
        }
    }
}
