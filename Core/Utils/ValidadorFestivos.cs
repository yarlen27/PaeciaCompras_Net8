using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Utils
{
    public static class ValidadorFestivos
    {


        public static bool EsSemanaSanta(DateTime fecha)
        {

            fecha = fecha.AddDays(-7);
            var esSemanaSanta = false;

            int diff = (7 + (fecha.DayOfWeek - DayOfWeek.Sunday)) % 7;
            var inicioSemana = fecha.AddDays(-1 * diff).Date;

            for (int i = 0; i < 6; i++)
            {
                if (inicioSemana.AddDays(i).Date == OtrasFechasCalculadas(fecha.Year, -2))
                {
                    return true;
                }          //viernes santo
                if (inicioSemana.AddDays(i).Date == OtrasFechasCalculadas(fecha.Year, 40, true))
                {
                    return true;
                } //jueves santo


            }



            return esSemanaSanta;
        }



        public static bool EsFestivo(DateTime fecha)
        {
            //FEstivos fijos

            fecha = fecha.Date;

            if (fecha.Date == new DateTime(fecha.Year, 1, 1))// Primero de Enero
            {
                return true;
            }


            if (fecha.Date == new DateTime(fecha.Year, 5, 1))// Primero de Mayo
            {
                return true;
            }

            if (fecha.Date == new DateTime(fecha.Year, 7, 20))// Independencia 20 de Julio
            {
                return true;
            }

            if (fecha.Date == new DateTime(fecha.Year, 7, 20))// Independencia 20 de Julio
            {
                return true;
            }

            if (fecha.Date == new DateTime(fecha.Year, 8, 7))// Batalla de Boyacá 7 de Agosto
            {
                return true;
            }


            if (fecha.Date == new DateTime(fecha.Year, 12, 8))//Maria Inmaculada 8 diciembre (religiosa)
            {
                return true;
            }

            if (fecha.Date == new DateTime(fecha.Year, 12, 25))//Navidad
            {
                return true;
            }



            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 1, 6)))
            {
                return true;

            }            // Reyes Magos Enero 6
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 3, 19)))
            {
                return true;

            }            // San Jose Marzo 19
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 6, 29)))
            {
                return true;
            }            // San Pedro y San Pablo Junio 29
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 8, 15)))
            {
                return true;

            }            // Asunción Agosto 15
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 10, 12)))
            {
                return true;

            }         // Descubrimiento de América Oct 12
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 11, 1)))
            {
                return true;

            }            // Todos los santos Nov 1
            if (fecha.Date == Calcula_Emiliani(new DateTime(fecha.Year, 11, 11)))
            {
                return true;

            }           // Independencia de Cartagena Nov 11



            //OTRAS FECHAS CALCULADAS

            if (fecha.Date == OtrasFechasCalculadas(fecha.Year, -3))
            {
                return true;
            }          //jueves santo
            if (fecha.Date == OtrasFechasCalculadas(fecha.Year, -2))
            {
                return true;
            }          //viernes santo
            if (fecha.Date == OtrasFechasCalculadas(fecha.Year, 40, true))
            {
                return true;
            }        //Ascención el Señor pascua

            if (fecha.Date == OtrasFechasCalculadas(fecha.Year, 60, true))
            {
                return true;
            }        //Corpus Cristi
            if (fecha.Date == OtrasFechasCalculadas(fecha.Year, 68, true))
            {
                return true;
            }		//Sagrado Corazón




            return false;
        }


        public static DateTime Calcula_Emiliani(DateTime fecha)
        {
            // funcion que mueve una fecha diferente a lunes al siguiente lunes en el
            // calendario y se aplica a fechas que estan bajo la ley emiliani
            //global  $y,$dia_festivo,$mes_festivo,$festivo;
            // Extrae el dia de la semana
            // 0 Domingo … 6 Sábado

            var diaDelaSemana = fecha.DayOfWeek;
            switch (diaDelaSemana)
            {
                case DayOfWeek.Sunday:
                    return fecha.AddDays(1);
                case DayOfWeek.Monday:
                    break;
                case DayOfWeek.Tuesday:
                    return fecha.AddDays(6);

                case DayOfWeek.Wednesday:

                    return fecha.AddDays(5);

                case DayOfWeek.Thursday:
                    return fecha.AddDays(4);

                case DayOfWeek.Friday:
                    return fecha.AddDays(3);

                case DayOfWeek.Saturday:
                    return fecha.AddDays(2);

                default:
                    break;
            }

            return fecha;


        }


        public static DateTime OtrasFechasCalculadas(int year, int cantidadDias = 0, bool siguienteLunes = false)
        {
            //$mes_festivo = date("n", mktime(0, 0, 0,$this->pascua_mes,$this->pascua_dia +$cantidadDias,$this->ano));
            //$dia_festivo = date("d", mktime(0, 0, 0,$this->pascua_mes,$this->pascua_dia +$cantidadDias,$this->ano));
            int mes_festivo = EasterSunday(year).AddDays(cantidadDias).Month;

            int dia_festivo = EasterSunday(year).AddDays(cantidadDias).Day;

            if (siguienteLunes)
            {
                return Calcula_Emiliani(new DateTime(year, mes_festivo, dia_festivo));
            }
            else
            {
                return new DateTime(year, mes_festivo, dia_festivo);
            }
        }


        public static DateTime EasterSunday(int year)
        {
            int month;
            int day;

            int g = year % 19;
            int c = year / 100;
            int h = h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25)
                                                + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) *
                        (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) +
                          i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

    }
}
