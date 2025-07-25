using Cobalto.Mongo.Core.BLL;
using Core.Bll;
using Core.Models;
using Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.BLL
{
    public class ProyectoBLL : BaseBLL<Proyecto>
    {
        public ProyectoBLL(IConfiguration configuration, IHttpContextAccessor httpContext) : base(configuration, httpContext)
        {

        }

        public async Task<bool> EsDiaPedido(Guid id)
        {
            var proyecto = await this.GetById(id);


            var currentDate = DateTime.Now.DayOfWeek;

            var dayDictionary = new Dictionary<string, int>();


            dayDictionary.Add("Domingo", 0);
            dayDictionary.Add("Lunes", 1);
            dayDictionary.Add("Martes", 2);
            dayDictionary.Add("Miércoles", 3);
            dayDictionary.Add("Jueves", 4);
            dayDictionary.Add("Viernes", 5);
            dayDictionary.Add("Sábado", 6);

            if ((DayOfWeek)dayDictionary[proyecto.diaPedido] == DateTime.Now.DayOfWeek)
            {
                return true;
            }
            else
            {
                var diaAnterior = DateTime.Now.AddDays(-1);

                var esFestivo = ValidadorFestivos.EsFestivo(diaAnterior);

                return esFestivo && ((DayOfWeek)dayDictionary[proyecto.diaPedido] == diaAnterior.DayOfWeek);
            }


        }

        public async Task<List<Empresa>> Empresas()
        {
            var proyectos =  await this.GetAll();

            var empresas = proyectos.GroupBy(x => x.nit);

            var empresasUnicas = from g in empresas
                                 select new Empresa
                                 {
                                     nit = g.Key,
                                     empresa = g.FirstOrDefault().empresa
                                 };

            return empresasUnicas.ToList();
        }

        public async Task<EjecutadoProyecto> EjecutadoProyecto(Guid id)
        {
            EjecutadoProyecto result = new EjecutadoProyecto();

            Proyecto proyecto = await this.GetById(id);


            result.presupuestoProyecto = proyecto.limitePresupuesto;


            return result;

        }

        public async Task<List<Notificacion>> ProximosVencimientos()
        {
            var proyectos = await this.GetAll();
            //Proyectos que esten a menos de 15 dias de terminar pero que aun no se hayan finalizado
            var proyectosVencidos = proyectos.Where(x => x.fin.HasValue &&( x.fin.Value.AddDays(-15) < DateTime.Now && x.fin > DateTime.Now)).ToList();


            var result = new List<Notificacion>();

            foreach (var item in proyectosVencidos)
            {
                var notificacion = new Notificacion
                {
                    Fecha = DateTime.Now,
                    Leido = false,
                    Mensaje = $"El proyecto {item.nombre} está próximo a finalizar, RECUERDE HACER LAS EVALUACIONES DE PROVEEDORES, SI YA LAS REALIZÓ OMITA ESTE MENSAJE",
                    Para = "Evaluador",
                    Titulo = "EVALUACION PROVEEDORES Proyecto próximo a finalizar"
                };
                result.Add(notificacion);

            }


            return result;

        }
    }

    public class Notificacion : BaseTable
    {

        public string Titulo { get; set; }
        public DateTime Fecha { get; set; }
        public bool Leido { get; set; }
        public string Mensaje { get; set; }
        public string Para { get; set; }
    }
}
