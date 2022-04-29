using SGC.Models.Feedback;
using System.Data.Entity;

namespace SGC.Models
{
    public partial class InsecapContext : DbContext
    {
        public InsecapContext()
           : base("name=localhost")
        {
            //Database.SetInitializer<InsecapContext>(null);


        }
        public virtual DbSet<FeedbackItemCommentMoodle> FeedbackItemCommentMoodle { get; set; }
        public virtual DbSet<Attempts> Attempts { get; set; }
        public virtual DbSet<AttemptsQuizUser> AttemptsQuizUser { get; set; }

        public virtual DbSet<FeedbackItemDataMoodle> FeedbackItemDataMoodle { get; set; }
        public virtual DbSet<FeedbackItemMoodle> FeedbackItemMoodle { get; set; }
        public virtual DbSet<FeedbackMoodle> FeedbackMoodle { get; set; }
        public virtual DbSet<Publicidad> Publicidad { get; set; }
        public virtual DbSet<PublicidadCliente> PublicidadCliente { get; set; }
        public virtual DbSet<LinkComercializacion> LinkComercializacion { get; set; }
        public virtual DbSet<Link> Link { get; set; }
        public virtual DbSet<LinkType> LinkTypes { get; set; }
        public virtual DbSet<Faena> Faena { get; set; }
        public virtual DbSet<FaenaCliente> FaenaCliente { get; set; }
        public virtual DbSet<PostCurso> PostCurso { get; set; }
        public virtual DbSet<Observacion> Observacions { get; set; }
        public virtual DbSet<RelatorHistorialComercializacion> RelatorHistorialComercializacion { get; set; }
        public virtual DbSet<TipoVentaHistorialComercializacion> TipoVentaHistorialComercializacion { get; set; }
        public virtual DbSet<HistorialComercializacion> HistorialComercializacion { get; set; }
        public virtual DbSet<R53> R53 { get; set; }
        public virtual DbSet<TextoEmail> TextoEmail { get; set; }
        public virtual DbSet<R43> R43 { get; set; }
        public virtual DbSet<ConfiguracionUsuarioRelator> ConfiguracionUsuarioRelator { get; set; }
        public virtual DbSet<CotizacionAporteCapacitacion> CotizacionAporteCapacitacion { get; set; }
        public virtual DbSet<AporteCapacitacion> AporteCapacitacion { get; set; }
        public virtual DbSet<UrlMaterialCurso> UrlMaterialCurso { get; set; }
        public virtual DbSet<NotificacionConfig> NotificacionConfig { get; set; }
        public virtual DbSet<MetasSucursal> MetasSucursal { get; set; }
        public virtual DbSet<MetasVendedor> MetasVendedor { get; set; }
        public virtual DbSet<Meta> Meta { get; set; }
        public virtual DbSet<Url> Url { get; set; }
        public virtual DbSet<R16> R16 { get; set; }
        public virtual DbSet<R52> R52 { get; set; }
        public virtual DbSet<R24> R24 { get; set; }
        public virtual DbSet<R23> R23 { get; set; }
        public virtual DbSet<CredencialesFile> CredencialesFile { get; set; }
        public virtual DbSet<FacturaStorage> FacturaStorage { get; set; }
        public virtual DbSet<Sucursal> Sucursal { get; set; }
        public virtual DbSet<ConfiguracionUsuarioParticipante> ConfiguracionUsuarioParticipante { get; set; }
        public virtual DbSet<RespuestaEvaluacion> RespuestaEvaluacion { get; set; }
        public virtual DbSet<Evaluacion> Evaluacion { get; set; }
        public virtual DbSet<Encuesta> Encuesta { get; set; }
        public virtual DbSet<SeccionEncuesta> SeccionEncuesta { get; set; }
        public virtual DbSet<R19> R19 { get; set; }
        public virtual DbSet<MaterialCurso> MaterialCurso { get; set; }
        public virtual DbSet<Template> Template { get; set; }
        public virtual DbSet<FacturaEstadoFactura> FacturaEstadoFactura { get; set; }
        public virtual DbSet<Factura> Factura { get; set; }
        public virtual DbSet<EstadoNotificacion> EstadoNotificacion { get; set; }
        public virtual DbSet<Notificacion> Notificacion { get; set; }
        public virtual DbSet<RelatorCursoSolicitado> RelatorCursoSolicitado { get; set; }
        public virtual DbSet<SalidaTerreno> SalidaTerreno { get; set; }
        //public virtual DbSet<Vendedor> Vendedor { get; set; }
        //public virtual DbSet<TipoDocCompromiso> TipoDocCompromiso { get; set; }
        public virtual DbSet<DocumentoCompromiso> DocumentoCompromiso { get; set; }
        public virtual DbSet<Participante> Participante { get; set; }
        public virtual DbSet<Bloque> Bloque { get; set; }
        public virtual DbSet<Sala> Sala { get; set; }
        public virtual DbSet<LugarAlmuerzo> LugarAlmuerzo { get; set; }
        public virtual DbSet<Otic> Otic { get; set; }
        public virtual DbSet<Comercializacion> Comercializacion { get; set; }
        public virtual DbSet<ComercializacionEstadoComercializacion> ComercializacionEstadoComercializacion { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<RelatorCurso> RelatorCurso { get; set; }
        public virtual DbSet<CalendarizacionAbierta> CalendarizacionAbierta { get; set; }
        public virtual DbSet<Calendarizacion> Calendarizacions { get; set; }
        public virtual DbSet<ClienteTipoDocumentosPago> ClienteTipoDocumentosPago { get; set; }
        public virtual DbSet<ClienteGiro> ClienteGiro { get; set; }
        public virtual DbSet<EncargadoPago> EncargadoPago { get; set; }
        public virtual DbSet<RepresentanteLegal> RepresentanteLegal { get; set; }
        public virtual DbSet<FileAzure> FileAzure { get; set; }
        public virtual DbSet<CategoriaItem> CategoriaItems { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Mandante> Mandante { get; set; }
        public virtual DbSet<Contacto> Contacto { get; set; }
        public virtual DbSet<ClienteContacto> ClienteContacto { get; set; }

        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<ClienteUsuario> ClienteUsuario { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<MenuTemp> MenuTemp { get; set; }

        public virtual DbSet<CustomPermission> CustomPermission { get; set; }
        public virtual DbSet<PermissionMenu> PermissionMenu { get; set; }

        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }

        public virtual DbSet<Giro> Giro { get; set; }
        public virtual DbSet<FormatoDocumentoR50> FormatoDocumentoR50 { get; set; }
        public virtual DbSet<TiposDocumentosPago> TiposDocumentosPago { get; set; }
        public virtual DbSet<EstadoComercial> EstadoComercial { get; set; }
        public virtual DbSet<Test> Test { get; set; }
        public virtual DbSet<AspNetUsersHistorial> AspNetUsersHistorial { get; set; }

        public virtual DbSet<R06_ActaReunion> R06_ActaReunion { get; set; }

        public virtual DbSet<ParticipantesReunion> ParticipantesReunion { get; set; }
        public virtual DbSet<QuizAprendizajePreguntas> QuizAprendizajePreguntas { get; set; }
        public virtual DbSet<QuizAprendizajeRespuestas> QuizAprendizajeRespuestas { get; set; }
        public virtual DbSet<QuizAprendizajeResultados> QuizAprendizajeResultados { get; set; }
        public virtual DbSet<QuizAprendizajeParticipantesRespuestas> QuizAprendizajeParticipantesRespuestas { get; set; }
        public virtual DbSet<QuizAprendizajeParticipantesResultados> QuizAprendizajeParticipantesResultados { get; set; }
        public virtual DbSet<Categoria> Categoria { get; set; }
        public virtual DbSet<Inventario> Inventario { get; set; }
        public virtual DbSet<InventarioCaracteristicas> InventarioCaracteristicas { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<AspNetRoles>()
                .HasMany(e => e.Permission)
                .WithRequired(e => e.AspNetRoles)
                .HasForeignKey(e => e.RoleID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<AspNetRoles>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.CustomPermission)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Menu>()
                .HasMany(e => e.CustomPermission)
                .WithRequired(e => e.Menu)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Menu>()
                .HasMany(e => e.Permission)
                .WithRequired(e => e.Menu)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Contacto>().HasKey(q => q.idContacto);
            modelBuilder.Entity<Cliente>().HasKey(q => q.idCliente);
            modelBuilder.Entity<ClienteContacto>().HasKey(q =>
                new
                {
                    q.idContacto,
                    q.idCliente
                });

            // Relationships
            modelBuilder.Entity<ClienteContacto>()
                .HasRequired(t => t.cliente)
                .WithMany(t => t.clienteContactos)
                .HasForeignKey(t => t.idCliente);

            modelBuilder.Entity<ClienteContacto>()
                .HasRequired(t => t.contacto)
                .WithMany(t => t.clienteContactos)
                .HasForeignKey(t => t.idContacto);


            modelBuilder.Entity<ClienteContactoCotizacion>().HasKey(q =>
            new
            {
                q.idContacto,
                q.idCliente
            });

            // Relationships
            modelBuilder.Entity<ClienteContactoCotizacion>()
                .HasRequired(t => t.cliente)
                .WithMany(t => t.clienteContactoCotizacion)
                .HasForeignKey(t => t.idCliente);

            modelBuilder.Entity<ClienteContactoCotizacion>()
                .HasRequired(t => t.contacto)
                .WithMany(t => t.clienteContactoCotizacion)
                .HasForeignKey(t => t.idContacto);



            modelBuilder.Entity<ClienteGiro>().HasKey(q =>
                new
                {
                    q.idGiro,
                    q.idCliente
                });

            modelBuilder.Entity<ClienteGiro>()
                .HasRequired(t => t.cliente)
                .WithMany(t => t.clienteGiros)
                .HasForeignKey(t => t.idCliente);

            modelBuilder.Entity<ClienteGiro>()
                .HasRequired(t => t.giro)
                .WithMany(t => t.clienteGiro)
                .HasForeignKey(t => t.idGiro);


            modelBuilder.Entity<ClienteTipoDocumentosPago>().HasKey(q =>
                new
                {
                    q.idTipoDocumentosPago,
                    q.idCliente
                });

            modelBuilder.Entity<ClienteTipoDocumentosPago>()
                .HasRequired(t => t.cliente)
                .WithMany(t => t.clienteTipoDocumentosPagos)
                .HasForeignKey(t => t.idCliente);

            modelBuilder.Entity<ClienteTipoDocumentosPago>()
                .HasRequired(t => t.tipoDocumentosPago)
                .WithMany(t => t.clienteTipoDocumentosPago)
                .HasForeignKey(t => t.idTipoDocumentosPago);


            modelBuilder.Entity<RelatorCurso>().HasKey(q =>
                new
                {
                    q.idCurso,
                    q.idRelator
                });

            modelBuilder.Entity<RelatorCurso>()
                .HasRequired(t => t.curso)
                .WithMany(t => t.relatorCurso)
                .HasForeignKey(t => t.idCurso);

            modelBuilder.Entity<RelatorCurso>()
                .HasRequired(t => t.relator)
                .WithMany(t => t.relatorCurso)
                .HasForeignKey(t => t.idRelator);



            modelBuilder.Entity<RelatorCursoSolicitado>().HasKey(q =>
                new
                {
                    q.idCurso,
                    q.idRelator
                });

            modelBuilder.Entity<RelatorCursoSolicitado>()
                .HasRequired(t => t.curso)
                .WithMany(t => t.relatorCursoSolicitado)
                .HasForeignKey(t => t.idCurso);

            modelBuilder.Entity<RelatorCursoSolicitado>()
                .HasRequired(t => t.relator)
                .WithMany(t => t.relatorCursoSolicitado)
                .HasForeignKey(t => t.idRelator);


            modelBuilder.Entity<CotizacionAporteCapacitacion>().HasKey(x =>
                new
                {
                    x.idCotizacion,
                    x.idAporteCapacitacion
                });

            modelBuilder.Entity<CotizacionAporteCapacitacion>()
                .HasRequired(x => x.cotizacion)
                .WithMany(x => x.cotizacionAporteCapacitacion)
                .HasForeignKey(x => x.idCotizacion);

            modelBuilder.Entity<CotizacionAporteCapacitacion>()
                .HasRequired(x => x.aporteCapacitacion)
                .WithMany(x => x.cotizacionAporteCapacitacion)
                .HasForeignKey(x => x.idAporteCapacitacion);
        }

        //public System.Data.Entity.DbSet<SGC.Models.Relator.DatosPersonales> DatosPersonales { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.Relator> Relators { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.DatosBancarios> DatosBancarios { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.ExperienciaLaboral> ExperienciaLaborals { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.TituloCurricular> TituloCurriculars { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.DetalleTitulo> DetalleTitulos { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Storage> Storages { get; set; }

        //public System.Data.Entity.DbSet<SGC.Models.Relator.DatosCurriculares> DatosCurriculares { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.Curso> Curso { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.R51> R51 { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Checklist> Checklist { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.R51_Checklist> R51_Checklist { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Pais> Pais { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Ciudad> Ciudad { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.R11> R11 { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.ContenidoEspecificoR11> ContenidoEspecificoR11 { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.ItemContenidoEspecificoR11> ItemContenidoEspecificoR11 { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.EscolaridadR11> EscolaridadR11 { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.CategoriaR11> CategoriaR11 { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.FormatoDocumento> FormatoDocumento { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.TipoFormatoDocumento> TipoFormatoDocumento { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.Item> Items { get; set; }

        object placeHolderVariable;
        public System.Data.Entity.DbSet<SGC.Models.Cotizacion_R13> Cotizacion_R13 { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.Costo> Costo { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.ListaDetalleCosto> ListaDetalleCosto { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.CostoCursoR12> CostoCursoR12 { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.CostoParticularCurso> CostoParticularCurso { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.ListaCostoParticularCurso> ListaCostoParticularCurso { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.ClienteContactoCotizacion> ClienteContactoCotizacion { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.Notas> Notas { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Asistencia> Asistencias { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.Formulario> Formulario { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.PreguntasFormulario> PreguntasFormulario { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.RespuestasFormulario> RespuestasFormulario { get; set; }
        public System.Data.Entity.DbSet<SGC.Models.RespuestasContestadasFormulario> RespuestasContestadasFormulario { get; set; }
        //public System.Data.Entity.DbSet<SGC.Models.TipoFormulario> TipoFormulario { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.ParametrosMoodle> ParametrosMoodles { get; set; }

        public System.Data.Entity.DbSet<SGC.Models.UsuarioMoodle> UsuarioMoodles { get; set; }
        
    }
}