namespace SGC.Models
{
    public class RelatorCursoSolicitado
    {
        public virtual int idCurso { get; set; }

        public virtual int idRelator { get; set; }

        public virtual Curso curso { get; set; }

        public virtual Relator relator { get; set; }
    }
}