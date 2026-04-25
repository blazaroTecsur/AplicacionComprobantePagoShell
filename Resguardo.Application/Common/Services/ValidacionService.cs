using Resguardo.Application.Common.Interfaces;

namespace Resguardo.Application.Common.Services
{
    public class ValidacionService : IValidacionService
    {
        private static readonly TimeSpan Mediodia = new TimeSpan(12, 0, 0);
        private static readonly TimeSpan Tres_PM = new TimeSpan(15, 0, 0);
        private static readonly TimeSpan Seis_AM = new TimeSpan(6, 0, 0);
        private static readonly TimeSpan Dsd_Diurno = new TimeSpan(5, 0, 0);
        private static readonly TimeSpan Hst_Diurno = new TimeSpan(14, 30, 0);
        private static readonly int AnticpUrgencia = 4;
        private static readonly int HoraMax = 12;
        private static readonly int HoraMin = 8;
        public ValidationResult ValidarHorarioFijado(bool diaSig, DateTime fechaServicio, TimeSpan horInicioAct, TimeSpan horFinalAct, TimeSpan horInicioNue, TimeSpan horFinalNue)
        {
            if (!diaSig)
            {
                if (horInicioNue > horFinalNue)
                    return new(false, "La hora de inicio no puede ser mayor a la hora de final. De ser el caso, indique si es un servicio hasta el día siguiente.");
            }
            int diaAdic = diaSig ? 1 : 0;    
            DateTime fechaIniAct = fechaServicio.AddTicks(horInicioAct.Ticks);
            DateTime fechaFinAct = fechaServicio.AddDays(+diaAdic).AddTicks(horFinalAct.Ticks);
            var diferenAct = fechaFinAct.Subtract(fechaIniAct);

            DateTime fechaIniNue = fechaServicio.AddTicks(horInicioNue.Ticks);
            DateTime fechaFinNue = fechaServicio.AddDays(+diaAdic).AddTicks(horFinalNue.Ticks);
            var diferenNue = fechaFinNue.Subtract(fechaIniNue);

            if (diferenNue != diferenAct)
                return new(false,
                    $"El total de horas de atención de cada servicio debe ser igual al solicitado. Es decir, si para el primer servicio, en el rango de horas solicitó un total de 8 hrs. de atención, el nuevo horario debe dar un total de 8 hrs.");

            return new(true, string.Empty);
        }
        public ValidationResult ValidarRangoHorario(bool diaSig, DateTime fechaServicio, TimeSpan horInicio, TimeSpan horFinal)
        {
            if (!diaSig)
            {
                if (horInicio > horFinal)
                    return new(false,
                        "La hora de inicio no puede ser mayor a la hora de final. De ser el caso, indique si es un servicio hasta el día siguiente");

                var diferen = horFinal.Subtract(horInicio);
                if (diferen.TotalHours > HoraMax || diferen.TotalHours < HoraMin)
                    return new(false,
                        $"El servicio solo puede ser solicitado como mínimo {HoraMin} horas y máximo de {HoraMax} horas de atención.");
            }
            else
            {
                DateTime fechaIni = fechaServicio.AddTicks(horInicio.Ticks);
                DateTime fechaFin = fechaServicio.AddDays(+1).AddTicks(horFinal.Ticks);

                var diferen = fechaFin.Subtract(fechaIni);
                if (diferen.TotalHours > HoraMax || diferen.TotalHours < HoraMin)
                    return new(false,
                        $"El servicio solo puede ser solicitado como mínimo {HoraMin} horas y máximo de {HoraMax} horas de atención.");
            }
            return new(true, string.Empty);
        }
        public ValidationResult ValidarServicioRegular(
            DateTime fechaActual,
            DateOnly fechaServicio,
            TimeOnly horaServicio)
        {
            var diaSolicitud = fechaActual.DayOfWeek;
            var horaSolicitud = TimeOnly.FromDateTime(fechaActual);
            var fechaSolicitud = DateOnly.FromDateTime(fechaActual);

            var h6am = new TimeOnly(6, 0);
            var h12pm = new TimeOnly(12, 0);
            var h3pm = new TimeOnly(15, 0);
            var h6pm = new TimeOnly(18, 0);

            // ── Lunes a jueves ───────────────────────────────────────────────────────
            // Regla conjunta: si el servicio es el día siguiente, la solicitud debe
            // ser antes de las 12pm Y la hora de servicio desde las 6am.
            // Si el servicio es cualquier día posterior, siempre válido.
            if (diaSolicitud is DayOfWeek.Monday
                             or DayOfWeek.Tuesday
                             or DayOfWeek.Wednesday
                             or DayOfWeek.Thursday)
            {
                var diaSiguiente = fechaSolicitud.AddDays(1);

                if (fechaServicio < diaSiguiente)
                    return new(false,
                        $"La fecha de servicio debe ser a partir del {diaSiguiente:dd/MM/yyyy}.");

                // Solo aplica restricción cuando el servicio es exactamente el día siguiente
                if (fechaServicio == diaSiguiente)
                {
                    if (horaSolicitud > h12pm && horaServicio < h6am)
                        return new(false,
                            $"Para servicio el {diaSiguiente:dd/MM/yyyy}, la solicitud debe " +
                            $"realizarse antes de las 12:00 pm y la hora de servicio " +
                            $"debe ser desde las 06:00 am.");

                    if (horaSolicitud > h12pm)
                        return new(false,
                            $"Para servicio el {diaSiguiente:dd/MM/yyyy}, " +
                            $"la solicitud debe realizarse antes de las 12:00 pm.");

                    if (horaServicio < h6am)
                        return new(false,
                            $"Para servicio el {diaSiguiente:dd/MM/yyyy}, " +
                            $"la hora de servicio debe ser desde las 06:00 am.");
                }

                // Días subsiguientes: siempre válido sin restricción de horas
                return new(true, string.Empty);
            }

            // ── Viernes ──────────────────────────────────────────────────────────────
            if (diaSolicitud is DayOfWeek.Friday)
            {
                var sabado = fechaSolicitud.AddDays(1);
                var domingo = fechaSolicitud.AddDays(2);
                var lunes = fechaSolicitud.AddDays(3);

                if (fechaServicio < sabado)
                    return new(false,
                        $"La fecha de servicio debe ser a partir del {sabado:dd/MM/yyyy}.");

                // Servicio el sábado: solicitud hasta las 12pm + hora servicio desde 6am (conjunta)
                if (fechaServicio == sabado)
                {
                    if (horaSolicitud > h12pm && horaServicio < h6am)
                        return new(false,
                            $"Para servicio el sábado {sabado:dd/MM/yyyy}, la solicitud debe " +
                            $"realizarse antes de las 12:00 pm y la hora de servicio " +
                            $"debe ser desde las 06:00 am.");

                    if (horaSolicitud > h12pm)
                        return new(false,
                            $"Para servicio el sábado {sabado:dd/MM/yyyy}, " +
                            $"la solicitud debe realizarse antes de las 12:00 pm.");

                    if (horaServicio < h6am)
                        return new(false,
                            $"Para servicio el sábado {sabado:dd/MM/yyyy}, " +
                            $"la hora de servicio debe ser desde las 06:00 am.");

                    return new(true, string.Empty);
                }

                // Servicio el domingo o lunes: solicitud hasta las 3pm + hora servicio desde 6am (conjunta)
                if (fechaServicio == domingo || fechaServicio == lunes)
                {
                    if (horaSolicitud > h3pm && horaServicio < h6am)
                        return new(false,
                            $"Para servicio el {NombreDia(fechaServicio.DayOfWeek)} " +
                            $"{fechaServicio:dd/MM/yyyy}, la solicitud debe realizarse " +
                            $"antes de las 3:00 pm y la hora de servicio " +
                            $"debe ser desde las 06:00 am.");

                    if (horaSolicitud > h3pm)
                        return new(false,
                            $"Para servicio el {NombreDia(fechaServicio.DayOfWeek)} " +
                            $"{fechaServicio:dd/MM/yyyy}, " +
                            $"la solicitud debe realizarse antes de las 3:00 pm.");

                    if (horaServicio < h6am)
                        return new(false,
                            $"Para servicio el {NombreDia(fechaServicio.DayOfWeek)} " +
                            $"{fechaServicio:dd/MM/yyyy}, " +
                            $"la hora de servicio debe ser desde las 06:00 am.");

                    return new(true, string.Empty);
                }

                // Días subsiguientes al lunes: siempre válido
                return new(true, string.Empty);
            }

            // ── Sábado y domingo ─────────────────────────────────────────────────────
            // Regla conjunta: si el servicio es exactamente el próximo martes,
            // la hora de servicio debe ser desde las 6pm.
            // Días posteriores al martes: siempre válido.
            if (diaSolicitud is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                var diasHastaMartes = ((int)DayOfWeek.Tuesday - (int)diaSolicitud + 7) % 7;
                if (diasHastaMartes == 0) diasHastaMartes = 7;
                var proximoMartes = fechaSolicitud.AddDays(diasHastaMartes);

                if (fechaServicio < proximoMartes)
                    return new(false,
                        $"Los fines de semana, la fecha de servicio debe ser " +
                        $"a partir del martes {proximoMartes:dd/MM/yyyy}.");

                // Solo aplica restricción de hora cuando es exactamente el martes
                if (fechaServicio == proximoMartes && horaServicio < h6pm)
                    return new(false,
                        $"Para servicio el martes {proximoMartes:dd/MM/yyyy}, " +
                        $"la hora de servicio debe ser desde las 06:00 pm.");

                // Miércoles en adelante: siempre válido
                return new(true, string.Empty);
            }

            return new(false, "Día de solicitud no reconocido.");
        }
        public ValidationResult ValidarServicioUrgencia(
            DateTime fechaActual,
            DateOnly fechaServicio,
            TimeSpan horaServicio)
        {
            TimeSpan anticip = horaServicio.Subtract(fechaActual.TimeOfDay);
            if (DateOnly.FromDateTime(fechaActual) == fechaServicio)
            {
                if (anticip.TotalHours < AnticpUrgencia)
                    return new(false,
                            $"La hora del servicio debe realizar con {AnticpUrgencia} de anticipación.");
            }
            return new(true, string.Empty);
        }
        public string ObtenerTurno(TimeSpan hraInicio)
        {
            string turno = Constantes.TURNO_NOCTURNO;
            if (hraInicio >= Dsd_Diurno && hraInicio <= Hst_Diurno)
                turno = Constantes.TURNO_DIURNO;
            return turno;
        }
        private static string NombreDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Monday => "lunes",
            DayOfWeek.Tuesday => "martes",
            DayOfWeek.Wednesday => "miércoles",
            DayOfWeek.Thursday => "jueves",
            DayOfWeek.Friday => "viernes",
            DayOfWeek.Saturday => "sábado",
            DayOfWeek.Sunday => "domingo",
            _ => string.Empty
        };
    }
}
