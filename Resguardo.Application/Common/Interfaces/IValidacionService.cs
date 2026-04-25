namespace Resguardo.Application.Common.Interfaces
{
    public interface IValidacionService
    {
        public ValidationResult ValidarRangoHorario(
            bool diaDig, 
            DateTime fechaServicio, 
            TimeSpan horInicio, 
            TimeSpan horFinal);
        public ValidationResult ValidarServicioRegular(
            DateTime fechaActual,
            DateOnly fechaServicio,
            TimeOnly horaServicio);
        public ValidationResult ValidarServicioUrgencia(
            DateTime fechaActual,
            DateOnly fechaServicio,
            TimeSpan horaServicio);
        public string ObtenerTurno(TimeSpan hraInicio);
        public ValidationResult ValidarHorarioFijado(
            bool diaSig,
            DateTime fechaServicio,
            TimeSpan horInicioAct,
            TimeSpan horFinalAct,
            TimeSpan horInicioNue,
            TimeSpan horFinalNue);
    }
}
