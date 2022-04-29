using System.Collections.Generic;

namespace SGC.Models
{
    public class ViewModelCostoParticularCurso
    {
        public int idCurso { get; set; }

        public int idCostoCursoR12 { get; set; }

        public List<ListaCostoParticularCurso> listaCostoParticularCursos { get; set; }

        public List<CostoParticularCurso> costoParticularCursosManoDeObra { get; set; }

        public List<CostoParticularCurso> costoParticularCursosEquiposYHerramientas { get; set; }

        public List<CostoParticularCurso> costoParticularCursosMateriales { get; set; }

        public List<CostoParticularCurso> costoParticularCursosOtrosGastos { get; set; }


    }
}