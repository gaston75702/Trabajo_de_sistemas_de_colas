using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;


namespace TP5_Carnet_De_Conducir
{
    public partial class aa : Form
    {
        public aa()
        {
            InitializeComponent();
        }
        #region Variables
        double h = 0.1;
        double t_Anterior;
        double t;

        double? beta; //Uniforme entre 0 y 1
        double? A = 136.0925;    //
        double? A_Anterior;
       


        double? L;
        double? L_Anterior;
        double Bloqueo_Cola_Cant_Min = 9;

        double? S;
        double? S_anterior;
        double? S_Duracion;
        double Bloqueo_Servidor_Cant_Min = 2;

        double? k1;
        double? k2;
        double? k3;
        double? k4;

        double Rnd_Eleccion_Atentado;
        double Guardo_Hora_Atentado;

        double? Hora_Fin_Atentado;

        bool RungeKutta1 = false;
        bool RungeKutta2 = false;
        bool RungeKutta3 = false;
        bool RungeKutta4 = false;
        bool RungeKutta5 = false;
        bool RungeKutta6 = false;
        bool RungeKutta7 = false;
        bool RungeKutta8 = false;


        int Cola_Entrada = 0;
        double Proximo_Atentado;


        double Matricula_TiempoAtencion_Uniforme_Minimo = 8.7;
        double Matricula_TiempoAtencion_Uniforme_MAximo = 15.2;

        double Renovacion_TiempoAtencion_Normal_Media = 16.7;
        double Renovacion_TiempoAtencion_Normal_Desviacion = 5;

        double Matricula_LlegadaCliente_Exponencial_Media = 3;
        double Renovacion_LlegadaCliente_Exponencial_Media = 5;

        double? Variable_1;
        bool Variable_2 = false;
        bool Variable_3 = true;
        int Variable_4;


        bool Bandera_1 = false;
        bool Bandera_2 = false;
        bool Bandera_3 = false;
        bool Bandera_4 = false;
        bool Bandera_5 = false;
        bool Bandera_6 = false;


        bool Flag_1 = false;
        bool Flag_2 = false;

        int contadorCliente = 0;
        List<Cliente> listCliente = new List<Cliente>();
        List<Servidor> ListServidor = new List<Servidor>();
        List<RungeKutta_Ataque_Servidor> ListAtaqueServidor = new List<RungeKutta_Ataque_Servidor>();
        List<RungeKutta_Bloqueo_Cola> ListBloqueoCola = new List<RungeKutta_Bloqueo_Cola> ();
        List<RungeKutta_Servidor_Bloqueado> ListServidorBloqueado = new List<RungeKutta_Servidor_Bloqueado>();

        int simulaciones = 0;
        int desde = 0;
        int hasta = 0;

        Random rnd = new Random();

        double? Menor_Hora_Proximo_Evento = 0;
        bool Bandera_Prox_Evento = false;


        //Variables para Generar la simulacion de colas
        string Evento;
        double? Reloj = 0;
        double Reloj_Anterior;


        //Variables para Llegada de Proximo Cliente
        double Random_Matricula;
        double Hora_Llegada_Matricula;
        double Random_Renovacion;
        double Hora_Llegada_Renovacion;
        double Tiempo_Entre_Llegada;
        string Tipo_Cliente;
        double Proxima_Llegada_Renovacion;
        double Proxima_Llegada_Matricula;

        //Variables para Fin de Atencion
        double Random_Tiempo_Atencion;
        double Tiempo_Atencion;
        double? Fin_Manuel = 0;

        //Zona Matricula
        double? Fin_Tomas = 0;
        double? Fin_Alicia = 0;

        //Zona Renovacion
        double? Fin_Lucia = 0;
        double? Fin_Maria = 0;


        //Variables Para Servidores
        //Servidor Matricula
        int Cola_Matricula;
        string Estado_Tomas;
        string Estado_Alicia;

        int Cola_Renovacion;
        string Estado_Lucia;
        string Estado_Maria;

        string Estado_Manuel;

        //Metricas

        int cantClienteRenovacion;
        int cantClienteMatricula;
       

        int cantClientesAtentidosM;
        int cantClientesAtentidosR;

        int maximaCantEnColaRe;
        int maximaCantEnColaMa;


        #endregion       


        public void Comenzar()
        {
            Elegir_Menor_Para_Proximo_Evento();
            if (Menor_Hora_Proximo_Evento < 480)
            {

                if (Menor_Hora_Proximo_Evento > t_Anterior && RungeKutta1 == false)
                {
                    Rnd_Eleccion_Atentado = rnd.NextDouble();
                    Guardo_Hora_Atentado = t_Anterior;
                    //Rnd_Eleccion_Atentado = 0.11;
                    if (Rnd_Eleccion_Atentado < 0.70)
                    {
                        //Se retiene la entrada
                        Reloj = Guardo_Hora_Atentado;
                        ListServidorBloqueado.Clear();
                        Runge_Kutta_Duracion_Bloqueo_Cola();
                        Evento = "Inicio Atentado Cola Sistema";
                        Hora_Fin_Atentado = t_Anterior;
                        RungeKutta1 = true;
                        RungeKutta4 = true;
                    }
                    else
                    {
                        //Se retiene un servidor
                        Reloj = Guardo_Hora_Atentado;
                        ListServidorBloqueado.Clear();
                        Runge_Kutta_Ataque_Servidor();
                        Evento = "Inicio Atentado Bloqueo Servidor";
                        Hora_Fin_Atentado = t_Anterior;
                        RungeKutta1 = true;
                        RungeKutta5 = true;

                        foreach (var item in ListServidor)
                        {
                            if (item.nombre == Servidor.Nombre.Alicia && item.estado != Servidor.Estado.Descanso)
                            {
                                item.estado = Servidor.Estado.Atentado;                               
                                if (item.Fin_Tiempo_Atencion != null)
                                {
                                    foreach (var cli in listCliente)
                                    {
                                        if (cli.Servidor == "Alicia" && cli.estado == Cliente.Estado.Siendo_Atendido && cli.Tipo_Cliente == "Matricula")
                                        {
                                            if (Estado_Tomas == "Libre" || Estado_Manuel == "Libre")
                                            {
                                                foreach (var nuevo in ListServidor)
                                                {
                                                    if (nuevo.nombre == Servidor.Nombre.Tomas && nuevo.estado == Servidor.Estado.Libre)
                                                    {
                                                        cli.estado = Cliente.Estado.Siendo_Atendido;
                                                        cli.Servidor = "Tomas";
                                                        cli.esta_en_cola = Cliente.EstaEnCola.No;
                                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                                        nuevo.estado = Servidor.Estado.Ocupado;
                                                        nuevo.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                                        Estado_Tomas = "Ocupado";
                                                        Fin_Tomas = nuevo.Fin_Tiempo_Atencion;
                                                        break;
                                                    }
                                                    if (nuevo.nombre == Servidor.Nombre.Manuel && nuevo.estado == Servidor.Estado.Libre)
                                                    {
                                                        cli.estado = Cliente.Estado.Siendo_Atendido;
                                                        cli.Servidor = "Manuel";
                                                        cli.esta_en_cola = Cliente.EstaEnCola.No;
                                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                                        nuevo.estado = Servidor.Estado.Ocupado;
                                                        nuevo.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                                        Estado_Manuel = "Ocupado";
                                                        Fin_Manuel = nuevo.Fin_Tiempo_Atencion;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                cli.estado = Cliente.Estado.Esperando_Atencion;
                                                cli.Servidor = "";
                                                cli.esta_en_cola = Cliente.EstaEnCola.Si;
                                                Cola_Matricula++;
                                                Fin_Alicia = 0;
                                                Estado_Alicia = "Atentado";
                                                item.Fin_Tiempo_Atencion = null;
                                                //item.estado = Servidor.Estado.Atentado;
                                                break;
                                            }

                                            
                                        }
                                    }
                                }
                                else
                                {
                                    item.Fin_Tiempo_Atencion = null;
                                    Fin_Alicia = 0;
                                    Estado_Alicia = "Atentado";
                                }
                                item.Fin_Tiempo_Atencion = null;
                                Fin_Alicia = 0;
                                Estado_Alicia = "Atentado";
                                break;
                            }
                        }


                    }
                }
                else
                {
                    if (Menor_Hora_Proximo_Evento > Hora_Fin_Atentado)// && RungeKutta2 == false)
                    {
                        Evento = "Fin Atentado Servidor";
                        //dgv_Se_Produce_Atentado.Rows.Clear();
                        ListAtaqueServidor.Clear();
                        Runge_Kutta_SeProduceElAtentado();
                        Reloj = Hora_Fin_Atentado;
                        Hora_Fin_Atentado = null;
                        RungeKutta1 = false;
                        //RungeKutta2 = true;

                        if (RungeKutta5 == true)
                        {
                            foreach (var item in ListServidor)
                            {
                                if (item.nombre == Servidor.Nombre.Alicia && item.estado == Servidor.Estado.Atentado)
                                {
                                    if (Cola_Matricula > 0)
                                    {
                                        foreach (var cli in listCliente)
                                        {
                                            if (cli.Tipo_Cliente == "Matricula" && cli.estado == Cliente.Estado.Esperando_Atencion)
                                            {
                                                Cola_Matricula--;
                                                cli.estado = Cliente.Estado.Siendo_Atendido;
                                                cli.Servidor = "Alicia";
                                                cli.esta_en_cola = Cliente.EstaEnCola.No;
                                                Random_Tiempo_Atencion = rnd.NextDouble();
                                                Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                                item.estado = Servidor.Estado.Ocupado;
                                                item.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                                Estado_Alicia = "Ocupado";
                                                Fin_Alicia = item.Fin_Tiempo_Atencion;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        item.estado = Servidor.Estado.Libre;
                                        Estado_Alicia = "libre";
                                        Fin_Alicia = null;
                                        //item.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                    }
                                    break;
                                }

                            }
                            RungeKutta5 = false;
                        }
                        if (RungeKutta4 == true)
                        {
                            foreach (var item in listCliente)
                            {
                                if (item.Tipo_Cliente == "Matricula" && item.esta_en_cola == Cliente.EstaEnCola.No && item.estado == Cliente.Estado.Esperando_Atencion && item.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.Si)
                                {
                                    cantClienteMatricula++;
                                    

                                    if (Estado_Tomas == "Ocupado" && (Estado_Alicia == "Ocupado" || Estado_Alicia == "Atentado") && Estado_Manuel == "Ocupado")
                                    {
                                        Cola_Matricula++;
                                        item.esta_en_cola = Cliente.EstaEnCola.Si;
                                    }
                                    else
                                    {
                                        Random_Tiempo_Atencion = rnd.NextDouble();                                                                                  
                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));


                                                foreach (var servi in ListServidor)
                                                {
                                                    if (servi.nombre == Servidor.Nombre.Tomas && servi.estado == Servidor.Estado.Libre)
                                                    {
                                                        servi.estado = Servidor.Estado.Ocupado;
                                                        servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                        Fin_Tomas = servi.Fin_Tiempo_Atencion;
                                                        Estado_Tomas = servi.estado.ToString();
                                                        item.estado = Cliente.Estado.Siendo_Atendido;
                                                        item.Servidor = servi.nombre.ToString();
                                                        Variable_3 = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (servi.nombre == Servidor.Nombre.Alicia && servi.estado == Servidor.Estado.Libre)
                                                        {
                                                            servi.estado = Servidor.Estado.Ocupado;
                                                            servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                            Fin_Alicia = servi.Fin_Tiempo_Atencion;
                                                            Estado_Alicia = servi.estado.ToString();
                                                            item.estado = Cliente.Estado.Siendo_Atendido;
                                                            item.Servidor = servi.nombre.ToString();
                                                            Variable_2 = true;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            if (servi.nombre == Servidor.Nombre.Manuel && servi.estado == Servidor.Estado.Libre)
                                                            {
                                                                servi.estado = Servidor.Estado.Ocupado;
                                                                servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                                Fin_Manuel = servi.Fin_Tiempo_Atencion;
                                                                Estado_Manuel = servi.estado.ToString();
                                                                item.estado = Cliente.Estado.Siendo_Atendido;
                                                                item.Servidor = servi.nombre.ToString();
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }      
                                    }
                                    //Cola_Matricula++;
                                }
                                if (item.Tipo_Cliente == "Renovacion" && item.esta_en_cola == Cliente.EstaEnCola.No && item.estado == Cliente.Estado.Esperando_Atencion && item.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.Si)
                                {
                                    cantClienteRenovacion++;


                                    if (Estado_Lucia == "Ocupado" && Estado_Maria == "Ocupado" && Estado_Manuel == "Ocupado")
                                    {
                                        Cola_Renovacion++;
                                        item.esta_en_cola = Cliente.EstaEnCola.Si;
                                    }
                                    else
                                    {
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        

                                        Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);


                                                foreach (var servi in ListServidor)
                                                {
                                                    if (servi.nombre == Servidor.Nombre.Lucia && servi.estado == Servidor.Estado.Libre)
                                                    {
                                                        servi.estado = Servidor.Estado.Ocupado;
                                                        servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                        Fin_Lucia = servi.Fin_Tiempo_Atencion;
                                                        Estado_Lucia = servi.estado.ToString();
                                                        item.estado = Cliente.Estado.Siendo_Atendido;
                                                        item.Servidor = servi.nombre.ToString();
                                                        Variable_3 = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (servi.nombre == Servidor.Nombre.Maria && servi.estado == Servidor.Estado.Libre)
                                                        {
                                                            servi.estado = Servidor.Estado.Ocupado;
                                                            servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                            Fin_Maria = servi.Fin_Tiempo_Atencion;
                                                            Estado_Maria = servi.estado.ToString();
                                                            item.estado = Cliente.Estado.Siendo_Atendido;
                                                            item.Servidor = servi.nombre.ToString();
                                                            Variable_2 = true;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            if (servi.nombre == Servidor.Nombre.Manuel && servi.estado == Servidor.Estado.Libre)
                                                            {
                                                                servi.estado = Servidor.Estado.Ocupado;
                                                                servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                                Fin_Manuel = servi.Fin_Tiempo_Atencion;
                                                                Estado_Manuel = servi.estado.ToString();
                                                                item.estado = Cliente.Estado.Siendo_Atendido;
                                                                item.Servidor = servi.nombre.ToString();
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            

                                        
                                    }


                                }
                            }
                            RungeKutta4 = false;
                            Cola_Entrada = 0;
                        }
                        
                    }
                }

                

                if (Menor_Hora_Proximo_Evento > 180 && Bandera_1 == false)
                {
                    Evento = "Descanso";
                    Reloj = 180;

                    foreach (var item in ListServidor)
                    {
                        if (item.nombre == Servidor.Nombre.Tomas)
                        {
                            foreach (var client in listCliente)
                            {
                                if (client.estado == Cliente.Estado.Siendo_Atendido && client.Servidor == item.nombre.ToString())
                                {
                                    Cola_Matricula++;
                                    client.estado = Cliente.Estado.Esperando_Atencion;
                                    client.Servidor = "";
                                    client.esta_en_cola = Cliente.EstaEnCola.Si;
                                    item.estado = Servidor.Estado.Descanso;
                                    item.Fin_Tiempo_Atencion = null;
                                    Estado_Tomas = "Descanso";
                                    Fin_Tomas = 0;
                                    Bandera_1 = true;
                                    
                                    break;
                                }
                            }

                            if (Bandera_1 != true)
                            {
                                item.estado = Servidor.Estado.Descanso;
                                item.Fin_Tiempo_Atencion = null;
                                Estado_Tomas = "Descanso";
                                Fin_Tomas = 0;
                                Bandera_1 = true;
                                break;
                            }
                        }

                    }
                }

                if (Menor_Hora_Proximo_Evento > 210 && Bandera_2 == false)
                {
                    Evento = "Descanso";
                    Reloj = 210;

                    foreach (var servi in ListServidor)
                    {
                        if (servi.nombre == Servidor.Nombre.Tomas)
                        {
                            if (Cola_Matricula > 0)
                            {
                                foreach (var tip in listCliente)
                                {
                                    if (tip.estado == Cliente.Estado.Esperando_Atencion && tip.Tipo_Cliente == "Matricula" && tip.esta_en_cola == Cliente.EstaEnCola.Si)
                                    {
                                        Cola_Matricula--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        //Tiempo_Atencion = 8.7 + (Random_Tiempo_Atencion * 6.5);
                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                        servi.estado = Servidor.Estado.Ocupado;
                                        servi.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                        tip.estado = Cliente.Estado.Siendo_Atendido;
                                        tip.Servidor = servi.nombre.ToString();
                                        tip.esta_en_cola = Cliente.EstaEnCola.No;
                                        Estado_Tomas = "Ocupado";
                                        Fin_Tomas = servi.Fin_Tiempo_Atencion;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                servi.estado = Servidor.Estado.Libre;
                                Estado_Tomas = "Libre";
                                Fin_Tomas = 0;
                                break;
                            }
                        }
                    }


                    foreach (var item in ListServidor)
                    {
                        if (item.nombre == Servidor.Nombre.Lucia)
                        {
                            foreach (var client in listCliente)
                            {
                                if (client.estado == Cliente.Estado.Siendo_Atendido && client.Servidor == item.nombre.ToString())
                                {
                                    Cola_Renovacion++;
                                    client.estado = Cliente.Estado.Esperando_Atencion;
                                    client.Servidor = "";
                                    client.esta_en_cola = Cliente.EstaEnCola.Si;
                                    item.estado = Servidor.Estado.Descanso;
                                    item.Fin_Tiempo_Atencion = null;
                                    Estado_Lucia = "Descanso";
                                    Fin_Lucia = 0;
                                    Bandera_2 = true;
                                    
                                    break;
                                }
                            }

                            if (Bandera_2 != true)
                            {
                                item.estado = Servidor.Estado.Descanso;
                                item.Fin_Tiempo_Atencion = null;
                                Estado_Lucia = "Descanso";
                                Fin_Lucia = 0;
                                Bandera_2 = true;
                                break;
                            }
                        }

                    }
                }

                if (Menor_Hora_Proximo_Evento > 240 && Bandera_3 == false)
                {
                    Evento = "Descanso";
                    Reloj = 240;

                    foreach (var servi in ListServidor)
                    {
                        if (servi.nombre == Servidor.Nombre.Lucia)
                        {
                            if (Cola_Renovacion > 0)
                            {
                                foreach (var tip in listCliente)
                                {
                                    if (tip.estado == Cliente.Estado.Esperando_Atencion && tip.Tipo_Cliente == "Renovacion" && tip.esta_en_cola == Cliente.EstaEnCola.Si)
                                    {
                                        Cola_Renovacion--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);
                                        //Tiempo_Atencion = 8.7 + (Random_Tiempo_Atencion * 6.5);
                                        servi.estado = Servidor.Estado.Ocupado;
                                        servi.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                        tip.estado = Cliente.Estado.Siendo_Atendido;
                                        tip.Servidor = servi.nombre.ToString();
                                        tip.esta_en_cola = Cliente.EstaEnCola.No;
                                        Estado_Lucia = "Ocupado";
                                        Fin_Lucia = servi.Fin_Tiempo_Atencion;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                servi.estado = Servidor.Estado.Libre;
                                Estado_Lucia = "Libre";
                                Fin_Lucia = 0;
                                break;
                            }
                        }
                    }

                    foreach (var item in ListServidor)
                    {
                        if (item.nombre == Servidor.Nombre.Manuel)
                        {
                            foreach (var client in listCliente)
                            {
                                if (client.estado == Cliente.Estado.Siendo_Atendido && client.Servidor == item.nombre.ToString())
                                {
                                    if (client.Tipo_Cliente == "Matricula")
                                    {
                                        Cola_Matricula++;
                                        client.estado = Cliente.Estado.Esperando_Atencion;
                                        client.Servidor = "";
                                        client.esta_en_cola = Cliente.EstaEnCola.Si;
                                        item.estado = Servidor.Estado.Descanso;
                                        item.Fin_Tiempo_Atencion = null;
                                        Estado_Manuel = "Descanso";
                                        Fin_Manuel = 0;
                                        Bandera_3 = true;
                                        
                                        break;
                                    }
                                    else
                                    {
                                        Cola_Renovacion++;
                                        client.estado = Cliente.Estado.Esperando_Atencion;
                                        client.Servidor = "";
                                        client.esta_en_cola = Cliente.EstaEnCola.Si;
                                        item.estado = Servidor.Estado.Descanso;
                                        item.Fin_Tiempo_Atencion = null;
                                        Estado_Manuel = "Descanso";
                                        Fin_Manuel = 0;
                                        Bandera_3 = true;
                                        
                                        break;
                                    }
                                }
                            }

                            if (Bandera_3 != true)
                            {
                                item.estado = Servidor.Estado.Descanso;
                                item.Fin_Tiempo_Atencion = null;
                                Estado_Manuel = "Descanso";
                                Fin_Manuel = 0;
                                Bandera_3 = true;
                                break;
                            }
                        }

                    }
                }

                if (Menor_Hora_Proximo_Evento > 270 && Bandera_4 == false)
                {
                    Evento = "Descanso";
                    Reloj = 270;

                    foreach (var servi in ListServidor)
                    {
                        if (servi.nombre == Servidor.Nombre.Manuel)
                        {
                            
                            foreach (var tip in listCliente)
                            {
                                if (tip.estado == Cliente.Estado.Esperando_Atencion && tip.esta_en_cola == Cliente.EstaEnCola.Si)
                                {
                                    if (tip.Tipo_Cliente == "Matricula")
                                    {
                                        Cola_Matricula--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                        Flag_1 = true;
                                        
                                    }
                                    else
                                    {
                                        Cola_Renovacion--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);
                                        Flag_1 = true;
                                    }
                                    servi.estado = Servidor.Estado.Ocupado;
                                    servi.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                    tip.estado = Cliente.Estado.Siendo_Atendido;
                                    tip.Servidor = servi.nombre.ToString();
                                    tip.esta_en_cola = Cliente.EstaEnCola.No;
                                    Estado_Manuel = "Ocupado";
                                    Fin_Manuel = servi.Fin_Tiempo_Atencion;
                                    break;
                                }
                            }
                            
                            if(Flag_1 == false)
                            {
                                servi.estado = Servidor.Estado.Libre;
                                Estado_Manuel = "Libre";
                                Fin_Manuel = 0;
                                break;
                            }
                        }
                    }

                    foreach (var item in ListServidor)
                    {
                        if (item.nombre == Servidor.Nombre.Alicia)
                        {
                            foreach (var client in listCliente)
                            {
                                if (client.estado == Cliente.Estado.Siendo_Atendido && client.Servidor == item.nombre.ToString())
                                {
                                    Cola_Matricula++;
                                    client.estado = Cliente.Estado.Esperando_Atencion;
                                    client.Servidor = "";
                                    client.esta_en_cola = Cliente.EstaEnCola.Si;
                                    item.estado = Servidor.Estado.Descanso;
                                    item.Fin_Tiempo_Atencion = null;
                                    Estado_Alicia = "Descanso";
                                    Fin_Alicia = 0;
                                    Bandera_4 = true;
                                    
                                    break;
                                }
                            }

                            if (Bandera_4 != true)
                            {
                                item.estado = Servidor.Estado.Descanso;
                                item.Fin_Tiempo_Atencion = null;
                                Estado_Alicia = "Descanso";
                                Fin_Alicia = 0;
                                Bandera_4 = true;
                                break;
                            }
                        }

                    }
                }

                if (Menor_Hora_Proximo_Evento > 300 && Bandera_5 == false)
                {
                    Evento = "Descanso";
                    Reloj = 300;

                    foreach (var servi in ListServidor)
                    {
                        if (servi.nombre == Servidor.Nombre.Alicia)
                        {
                            if (Cola_Matricula > 0)
                            {
                                foreach (var tip in listCliente)
                                {
                                    if (tip.estado == Cliente.Estado.Esperando_Atencion && tip.Tipo_Cliente == "Matricula" && tip.esta_en_cola == Cliente.EstaEnCola.Si)
                                    {
                                        Cola_Matricula--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                        servi.estado = Servidor.Estado.Ocupado;
                                        servi.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                        tip.estado = Cliente.Estado.Siendo_Atendido;
                                        tip.Servidor = servi.nombre.ToString();
                                        tip.esta_en_cola = Cliente.EstaEnCola.No;
                                        Estado_Alicia = "Ocupado";
                                        Fin_Alicia = servi.Fin_Tiempo_Atencion;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                servi.estado = Servidor.Estado.Libre;
                                Estado_Alicia = "Libre";
                                Fin_Alicia = 0;
                                break;
                            }
                        }
                    }

                    foreach (var item in ListServidor)
                    {
                        if (item.nombre == Servidor.Nombre.Maria)
                        {
                            foreach (var client in listCliente)
                            {
                                if (client.estado == Cliente.Estado.Siendo_Atendido && client.Tipo_Cliente == "Renovacion" && client.Servidor == item.nombre.ToString())
                                {
                                    Cola_Renovacion++;
                                    client.estado = Cliente.Estado.Esperando_Atencion;
                                    client.Servidor = "";
                                    client.esta_en_cola = Cliente.EstaEnCola.Si;
                                    item.estado = Servidor.Estado.Descanso;
                                    item.Fin_Tiempo_Atencion = null;
                                    Estado_Maria = "Descanso";
                                    Fin_Maria = 0;
                                    Bandera_2 = true;
                                    
                                    break;
                                }
                            }
                            if (Bandera_5 != true)
                            {
                                item.estado = Servidor.Estado.Descanso;
                                item.Fin_Tiempo_Atencion = null;
                                Estado_Maria = "Descanso";
                                Fin_Maria = 0;
                                Bandera_5 = true;
                                break;
                            }
                        }

                    }
                }

                if (Menor_Hora_Proximo_Evento > 330 && Bandera_6 == false)
                {
                    Evento = "Fin Descanso";
                    Reloj = 330;

                    foreach (var servi in ListServidor)
                    {
                        if (servi.nombre == Servidor.Nombre.Maria)
                        {
                            if (Cola_Renovacion > 0)
                            {
                                foreach (var tip in listCliente)
                                {
                                    if (tip.estado == Cliente.Estado.Esperando_Atencion && tip.Tipo_Cliente == "Renovacion" && tip.esta_en_cola == Cliente.EstaEnCola.Si)
                                    {
                                        Cola_Renovacion--;
                                        Random_Tiempo_Atencion = rnd.NextDouble();
                                        Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);
                                        servi.estado = Servidor.Estado.Ocupado;
                                        servi.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;
                                        tip.estado = Cliente.Estado.Siendo_Atendido;
                                        tip.Servidor = servi.nombre.ToString();
                                        tip.esta_en_cola = Cliente.EstaEnCola.No;
                                        Estado_Maria = "Ocupado";
                                        Fin_Maria = servi.Fin_Tiempo_Atencion;
                                        Bandera_6 = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                servi.estado = Servidor.Estado.Libre;
                                Estado_Maria = "Libre";
                                Fin_Maria = 0;
                                Bandera_6 = true;
                                break;
                            }
                        }
                    }
                }


                if (Evento == "Llegada Cliente" && RungeKutta4 == false) //ACA AGREGUE LAS 2 BOLUDECES CUALQUIER COSA BORRARLO
                {
                    Reloj = Menor_Hora_Proximo_Evento;
                    #region Llegada Cliente
                    
                        foreach (var cliente in listCliente)
                        {
                            if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Matricula" && cliente.esta_en_cola == Cliente.EstaEnCola.No && Menor_Hora_Proximo_Evento == Proxima_Llegada_Matricula)
                            {
                                Tipo_Cliente = "Matricula";
                                cantClienteMatricula++;


                                break;
                            }
                            else if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Renovacion" && cliente.esta_en_cola == Cliente.EstaEnCola.No && Menor_Hora_Proximo_Evento == Proxima_Llegada_Renovacion)
                            {
                                Tipo_Cliente = "Renovacion";
                                cantClienteRenovacion++;

                                break;
                            }

                        }

                        if (Tipo_Cliente == "Matricula")
                        {
                            Evento = "Llegada Cliente Matricula";
                            
                            if (Cola_Matricula == 0)
                            {
                                if (Estado_Tomas == "Ocupado" && (Estado_Alicia == "Ocupado" || Estado_Alicia == "Atentado") && Estado_Manuel == "Ocupado")
                                {
                                    Cola_Matricula++;

                                    foreach (var cliente in listCliente)
                                    {
                                        if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Matricula" && cliente.esta_en_cola == Cliente.EstaEnCola.No)
                                        {
                                            cliente.esta_en_cola = Cliente.EstaEnCola.Si;
                                            //cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.NO_Ultimo;
                                            break;
                                        }
                                    }

                                }
                                else
                                {
                                    Random_Tiempo_Atencion = rnd.NextDouble();
                                    foreach (var item in listCliente)
                                    {
                                        if (item.estado == Cliente.Estado.Esperando_Atencion && item.Tipo_Cliente == "Matricula")
                                        {
                                            Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                            

                                            foreach (var servi in ListServidor)
                                            {
                                                if (servi.nombre == Servidor.Nombre.Tomas && servi.estado == Servidor.Estado.Libre)
                                                {
                                                    servi.estado = Servidor.Estado.Ocupado;
                                                    servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                    Fin_Tomas = servi.Fin_Tiempo_Atencion;
                                                    Estado_Tomas = servi.estado.ToString();
                                                    item.estado = Cliente.Estado.Siendo_Atendido;
                                                    item.Servidor = servi.nombre.ToString();
                                                    Variable_3 = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (servi.nombre == Servidor.Nombre.Alicia && servi.estado == Servidor.Estado.Libre)
                                                    {
                                                        servi.estado = Servidor.Estado.Ocupado;
                                                        servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                        Fin_Alicia = servi.Fin_Tiempo_Atencion;
                                                        Estado_Alicia = servi.estado.ToString();
                                                        item.estado = Cliente.Estado.Siendo_Atendido;
                                                        item.Servidor = servi.nombre.ToString();
                                                        Variable_2 = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (servi.nombre == Servidor.Nombre.Manuel && servi.estado == Servidor.Estado.Libre)
                                                        {
                                                            servi.estado = Servidor.Estado.Ocupado;
                                                            servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                            Fin_Manuel = servi.Fin_Tiempo_Atencion;
                                                            Estado_Manuel = servi.estado.ToString();
                                                            item.estado = Cliente.Estado.Siendo_Atendido;
                                                            item.Servidor = servi.nombre.ToString();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }


                                        }
                                    }
                                }
                            }
                            else if (Cola_Matricula > 0)
                            {
                                Cola_Matricula++;
                                foreach (var cliente in listCliente)
                                {
                                    if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Matricula" && cliente.esta_en_cola == Cliente.EstaEnCola.No)
                                    {
                                        cliente.esta_en_cola = Cliente.EstaEnCola.Si;
                                        //cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.NO_Ultimo;
                                        break;
                                    }
                                }
                            }

                            Calcular_Siguiente_Cliente_Matricula();
                            
                            
                        }
                        else
                        {
                            Evento = "Llegada Cliente Renovacion";
                            if (Cola_Renovacion == 0)
                            {
                                if (Estado_Lucia == "Ocupado" && Estado_Maria == "Ocupado" && Estado_Manuel == "Ocupado")
                                {
                                    Cola_Renovacion++;

                                    foreach (var cliente in listCliente)
                                    {
                                        if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Renovacion" && cliente.esta_en_cola == Cliente.EstaEnCola.No)
                                        {
                                            cliente.esta_en_cola = Cliente.EstaEnCola.Si;
                                            //cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.NO_Ultimo;
                                        }
                                    }
                                }
                                else
                                {

                                    Random_Tiempo_Atencion = rnd.NextDouble();
                                    foreach (var item in listCliente)
                                    {
                                        if (item.estado == Cliente.Estado.Esperando_Atencion && item.Tipo_Cliente == "Renovacion")
                                        {
                                            Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);


                                            foreach (var servi in ListServidor)
                                            {
                                                if (servi.nombre == Servidor.Nombre.Lucia && servi.estado == Servidor.Estado.Libre)
                                                {
                                                    servi.estado = Servidor.Estado.Ocupado;
                                                    servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                    Fin_Lucia = servi.Fin_Tiempo_Atencion;
                                                    Estado_Lucia = servi.estado.ToString();
                                                    item.estado = Cliente.Estado.Siendo_Atendido;
                                                    item.Servidor = servi.nombre.ToString();
                                                    Variable_3 = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (servi.nombre == Servidor.Nombre.Maria && servi.estado == Servidor.Estado.Libre)
                                                    {
                                                        servi.estado = Servidor.Estado.Ocupado;
                                                        servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                        Fin_Maria = servi.Fin_Tiempo_Atencion;
                                                        Estado_Maria = servi.estado.ToString();
                                                        item.estado = Cliente.Estado.Siendo_Atendido;
                                                        item.Servidor = servi.nombre.ToString();
                                                        Variable_2 = true;
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        if (servi.nombre == Servidor.Nombre.Manuel && servi.estado == Servidor.Estado.Libre)
                                                        {
                                                            servi.estado = Servidor.Estado.Ocupado;
                                                            servi.Fin_Tiempo_Atencion = Reloj + Tiempo_Atencion;
                                                            Fin_Manuel = servi.Fin_Tiempo_Atencion;
                                                            Estado_Manuel = servi.estado.ToString();
                                                            item.estado = Cliente.Estado.Siendo_Atendido;
                                                            item.Servidor = servi.nombre.ToString();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            else if (Cola_Renovacion > 0)
                            {
                                Cola_Renovacion++;

                                foreach (var cliente in listCliente)
                                {
                                    if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Renovacion" && cliente.esta_en_cola == Cliente.EstaEnCola.No)
                                    {
                                        cliente.esta_en_cola = Cliente.EstaEnCola.Si;
                                        //cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.NO_Ultimo;
                                    }
                                }
                            }

                            Calcular_Siguiente_Cliente_Renovacion();
                        }


                        //Calcular_Tiempo_Entre_Llegada();
                        if (Cola_Matricula > maximaCantEnColaMa)
                        {
                            maximaCantEnColaMa = Cola_Matricula;
                        }
                        if (Cola_Renovacion > maximaCantEnColaRe)
                        {

                            maximaCantEnColaRe = Cola_Renovacion;
                        }
                    
                    
                    
                    #endregion

                } ////ACA AGREGUE LAS 2 BOLUDECES CUALQUIER COSA BORARRLO
                else
                {
                    if (Evento == "Llegada Cliente")
                    {
                        Reloj = Menor_Hora_Proximo_Evento;
                        foreach (var cliente in listCliente)
                        {
                            if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Matricula" && cliente.esta_en_cola == Cliente.EstaEnCola.No && Menor_Hora_Proximo_Evento == Proxima_Llegada_Matricula && cliente.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.No)//&& cliente.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.Ultimo)
                            {
                                Evento = "Llegada Cliente Matricula";
                                cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.Si;
                                //cantClienteMatricula++;
                                Calcular_Siguiente_Cliente_Matricula();
                                Cola_Entrada++;

                                break;
                            }
                            else if (cliente.estado == Cliente.Estado.Esperando_Atencion && cliente.Tipo_Cliente == "Renovacion" && cliente.esta_en_cola == Cliente.EstaEnCola.No && Menor_Hora_Proximo_Evento == Proxima_Llegada_Renovacion && cliente.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.No)// && cliente.Esta_en_Bloqueo_COla == Cliente.Es_ultimo.Ultimo)
                            {
                                Evento = "Llegada Cliente Renovacion";
                                cliente.Esta_en_Bloqueo_COla = Cliente.Es_ultimo.Si;
                                //cantClienteRenovacion++;
                                Calcular_Siguiente_Cliente_Renovacion();
                                Cola_Entrada++;
                                break;
                            }

                        }
                    }
                    

                }

                if (Evento == "Fin Atencion")
                {
                    Reloj = Menor_Hora_Proximo_Evento;
                    foreach (var servidor in ListServidor)
                    {
                        if (servidor.Fin_Tiempo_Atencion == Menor_Hora_Proximo_Evento)
                        {
                            foreach (var cliente in listCliente)
                            {
                                if (cliente.estado == Cliente.Estado.Siendo_Atendido && cliente.Servidor == servidor.nombre.ToString())
                                {
                                    if (cliente.Tipo_Cliente == "Matricula")
                                    {
                                        cliente.estado = Cliente.Estado.Destruido;
                                        cantClientesAtentidosM ++;
                                        if (Cola_Matricula > 0)
                                        {
                                            Cola_Matricula--;
                                            Random_Tiempo_Atencion = rnd.NextDouble();
                                            //Tiempo_Atencion = 0.000694444444444444;
                                            Tiempo_Atencion = Matricula_TiempoAtencion_Uniforme_Minimo + (Random_Tiempo_Atencion * (Matricula_TiempoAtencion_Uniforme_MAximo - Matricula_TiempoAtencion_Uniforme_Minimo));
                                            //Tiempo_Atencion = 8.7 + (Random_Tiempo_Atencion * 6.5);

                                            foreach (var item in listCliente)
                                            {
                                                if (item.estado == Cliente.Estado.Esperando_Atencion && item.Tipo_Cliente == "Matricula" && item.esta_en_cola == Cliente.EstaEnCola.Si)
                                                {
                                                    item.esta_en_cola = Cliente.EstaEnCola.No;
                                                    item.estado = Cliente.Estado.Siendo_Atendido;
                                                    item.Servidor = servidor.nombre.ToString();
                                                    //servidor.estado = Servidor.Estado.Ocupado;
                                                    servidor.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;

                                                    if (servidor.nombre == Servidor.Nombre.Tomas)
                                                    {
                                                        Fin_Tomas = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Tomas = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    if (servidor.nombre == Servidor.Nombre.Alicia)
                                                    {
                                                        Fin_Alicia = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Alicia = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    if (servidor.nombre == Servidor.Nombre.Manuel)
                                                    {
                                                        Fin_Manuel = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Manuel = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;

                                        }
                                        else
                                        {
                                            servidor.estado = Servidor.Estado.Libre;
                                            servidor.Fin_Tiempo_Atencion = null;
                                            if (servidor.nombre == Servidor.Nombre.Tomas)
                                            {
                                                Fin_Tomas = 0;
                                                Estado_Tomas = "Libre";
                                                break;
                                            }
                                            if (servidor.nombre == Servidor.Nombre.Alicia)
                                            {
                                                Fin_Alicia = 0;
                                                Estado_Alicia = "Libre";
                                                break;
                                            }
                                            if (servidor.nombre == Servidor.Nombre.Manuel)
                                            {
                                                Fin_Manuel = 0;
                                                Estado_Manuel = "Libre";
                                                break;
                                            }
                                            break;
                                        }


                                    }
                                    else
                                    {
                                        cliente.estado = Cliente.Estado.Destruido;
                                        cantClientesAtentidosR++;
                                        if (Cola_Renovacion > 0)
                                        {
                                            Cola_Renovacion--;
                                            Random_Tiempo_Atencion = rnd.NextDouble();
                                            //Tiempo_Atencion = 0.0111921296296296 + (Random_Tiempo_Atencion * 0.00347222222222222);
                                            Tiempo_Atencion = Renovacion_TiempoAtencion_Normal_Media + (Random_Tiempo_Atencion * Renovacion_TiempoAtencion_Normal_Desviacion);
                                            //Tiempo_Atencion = 16.7 + (Random_Tiempo_Atencion * 5);
                                            foreach (var item in listCliente)
                                            {
                                                if (item.estado == Cliente.Estado.Esperando_Atencion && item.Tipo_Cliente == "Renovacion" && item.esta_en_cola == Cliente.EstaEnCola.Si)
                                                {
                                                    item.esta_en_cola = Cliente.EstaEnCola.No;
                                                    item.estado = Cliente.Estado.Siendo_Atendido;
                                                    item.Servidor = servidor.nombre.ToString();
                                                    servidor.estado = Servidor.Estado.Ocupado;
                                                    servidor.Fin_Tiempo_Atencion = Tiempo_Atencion + Reloj;

                                                    if (servidor.nombre == Servidor.Nombre.Lucia)
                                                    {
                                                        Fin_Lucia = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Lucia = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    if (servidor.nombre == Servidor.Nombre.Maria)
                                                    {
                                                        Fin_Maria = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Maria = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    if (servidor.nombre == Servidor.Nombre.Manuel)
                                                    {
                                                        Fin_Manuel = servidor.Fin_Tiempo_Atencion;
                                                        Estado_Manuel = servidor.estado.ToString();
                                                        break;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;

                                        }
                                        else
                                        {
                                            servidor.estado = Servidor.Estado.Libre;
                                            servidor.Fin_Tiempo_Atencion = null;

                                            if (servidor.nombre == Servidor.Nombre.Lucia)
                                            {
                                                Fin_Lucia = 0;
                                                Estado_Lucia = "Libre";
                                                break;
                                            }
                                            if (servidor.nombre == Servidor.Nombre.Maria)
                                            {
                                                Fin_Maria = 0;
                                                Estado_Maria = "Libre";
                                                break;
                                            }
                                            if (servidor.nombre == Servidor.Nombre.Manuel)
                                            {
                                                Fin_Manuel = 0;
                                                Estado_Manuel = "Libre";
                                                break;
                                            }
                                            break;
                                        }
                                        //break;
                                    }
                                }

                            }
                            break;
                        }




                    }

                }
            }
            else
            {
                Variable_3 = false;
                Reloj = 0;
                contadorCliente = 0;
                Bandera_1 = false;
                Bandera_2 = false;
                Bandera_3 = false;
                Bandera_4 = false;
                Bandera_5 = false;

                cantClienteRenovacion = 0;
                cantClienteMatricula = 0;

                cantClientesAtentidosM = 0;
                cantClientesAtentidosR = 0;

                maximaCantEnColaRe = 0;
                maximaCantEnColaMa = 0;
                




                foreach (var item in listCliente)
                {
                    dgv_Clientes.Rows.Add(item.numero, item.estado, item.Servidor, item.Tipo_Cliente, item.TiempoEspera, item.esta_en_cola);
                }
                Simulacion_Cero();
            }
        }


        public void Elegir_Menor_Para_Proximo_Evento()
        {

            Variable_1 = ListServidor.Min(x => x.Fin_Tiempo_Atencion);

            if (Variable_1 != null)
            {
                if (Proxima_Llegada_Renovacion> Variable_1 && Proxima_Llegada_Matricula > Variable_1)
                {
                    Menor_Hora_Proximo_Evento = Variable_1;
                    Evento = "Fin Atencion";
                }
                else
                {
                    if (Proxima_Llegada_Renovacion > Proxima_Llegada_Matricula)
                    {
                        Menor_Hora_Proximo_Evento = Proxima_Llegada_Matricula;
                        Evento = "Llegada Cliente";
                        //Calcular_Siguiente_Cliente_Matricula();
                    }
                    else
                    {
                        Menor_Hora_Proximo_Evento = Proxima_Llegada_Renovacion;
                        Evento = "Llegada Cliente";
                        //Calcular_Siguiente_Cliente_Renovacion();
                    }
                    
                }
            }
            else
            {
                if (Proxima_Llegada_Renovacion > Proxima_Llegada_Matricula)
                {
                    Menor_Hora_Proximo_Evento = Proxima_Llegada_Matricula;
                    Evento = "Llegada Cliente";
                    //Calcular_Siguiente_Cliente_Matricula();
                }
                else
                {
                    Menor_Hora_Proximo_Evento = Proxima_Llegada_Renovacion;
                    Evento = "Llegada Cliente";
                    //Calcular_Siguiente_Cliente_Renovacion();
                }
            }            
        }




        public void Calcular_Siguiente_Cliente_Matricula()
        {
            Random_Matricula = rnd.NextDouble();
            Hora_Llegada_Matricula = - Matricula_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Matricula);
            

            Tiempo_Entre_Llegada = Hora_Llegada_Matricula;
            Proxima_Llegada_Matricula = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Matricula", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No , Esta_en_Bloqueo_COla = Cliente.Es_ultimo.No });  
        }

        public void Calcular_Siguiente_Cliente_Renovacion()
        {
            Random_Renovacion = rnd.NextDouble();
            Hora_Llegada_Renovacion = - Renovacion_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Renovacion);

            Tiempo_Entre_Llegada = Hora_Llegada_Renovacion;
            Proxima_Llegada_Renovacion = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Renovacion", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No , Esta_en_Bloqueo_COla = Cliente.Es_ultimo.No });
        }


        public void Calcular_Siguiente_Cliente_Matricula_Atentado_Cola()
        {
            Random_Matricula = rnd.NextDouble();
            Hora_Llegada_Matricula = -Matricula_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Matricula);


            Tiempo_Entre_Llegada = Hora_Llegada_Matricula;
            Proxima_Llegada_Matricula = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Matricula", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No, Esta_en_Bloqueo_COla = Cliente.Es_ultimo.Si });
        }

        public void Calcular_Siguiente_Cliente_Renovacion_Atentado_Cola()
        {
            Random_Renovacion = rnd.NextDouble();
            Hora_Llegada_Renovacion = -Renovacion_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Renovacion);

            Tiempo_Entre_Llegada = Hora_Llegada_Renovacion;
            Proxima_Llegada_Renovacion = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Renovacion", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No, Esta_en_Bloqueo_COla = Cliente.Es_ultimo.Si });
        }


        public class Cliente
        {
            public int numero;
            public enum Estado { Esperando_Atencion, Siendo_Atendido, Destruido};
            public Estado estado;
            public string Servidor;
            public string Tipo_Cliente;
            public double? TiempoEspera;
            public enum EstaEnCola {Si,No}
            public EstaEnCola esta_en_cola;
            public enum Es_ultimo {Si, No}
            public Es_ultimo Esta_en_Bloqueo_COla;
        }

        public class Servidor
        {
            public enum Nombre {Tomas,Alicia,Lucia,Maria,Manuel};
            public Nombre nombre;
            public enum Estado { Libre, Ocupado, Descanso, Atentado };
            public Estado estado;
            public double? Fin_Tiempo_Atencion;
        }

        public class RungeKutta_Ataque_Servidor
        {
            public double to;
            public double Ao;
            public double K1;
            public double K2;
            public double K3;
            public double K4;
            public double ti_mas_1;
            public double Ai_mas_1;

        }
        public class RungeKutta_Bloqueo_Cola
        {
            public double to;
            public double Lo;
            public double K1;
            public double K2;
            public double K3;
            public double K4;
            public double ti_mas_1;
            public double Li_mas_1;
        }

        public class RungeKutta_Servidor_Bloqueado
        {
            public double to;
            public double So;
            public double K1;
            public double K2;
            public double K3;
            public double K4;
            public double ti_mas_1;
            public double Si_mas_1;
        }


        public void cargarGrilla()
        {
            dgv_ClaseCLiente.Rows.Add(Evento,Math.Round((double)Reloj,4),Cola_Entrada,Math.Round(Random_Matricula,4), Math.Round(Hora_Llegada_Matricula, 4), Math.Round(Proxima_Llegada_Matricula, 4), Math.Round(Random_Renovacion,4), Math.Round(Hora_Llegada_Renovacion,4)
                , Math.Round(Proxima_Llegada_Renovacion,4), Math.Round(Random_Tiempo_Atencion,4), Fin_Tomas,Fin_Alicia,Fin_Lucia,Fin_Maria,Fin_Manuel
                ,Cola_Matricula,Estado_Tomas,Estado_Alicia,Cola_Renovacion,Estado_Lucia,Estado_Maria,Estado_Manuel,
                Math.Round(Proximo_Atentado,4), cantClienteRenovacion, cantClienteMatricula , cantClientesAtentidosR , cantClientesAtentidosM , maximaCantEnColaRe , maximaCantEnColaMa);
        }

        public void Simulacion_Cero()
        {
            listCliente.Clear();

            Calcular_Tiempo_Entre_Llegada();

            //Proxima_Llegada = Tiempo_Entre_Llegada;
            //Proxima_Llegada_Anterior = Tiempo_Entre_Llegada;

            foreach (var item in ListServidor)
            {
                item.estado = Servidor.Estado.Libre;
                item.Fin_Tiempo_Atencion = null;
            }

            
            Evento = "Inicio Simulacion";

            //Menor_Hora_Proximo_Evento = Proxima_Llegada_Renovacion;  

            RungeKutta1 = false;
            RungeKutta2 = false;
            RungeKutta3 = false;
            RungeKutta4 = false;
            RungeKutta5 = false;

            Estado_Tomas = "Libre";
            Estado_Alicia = "Libre";
            Estado_Lucia = "Libre";
            Estado_Maria = "Libre";
            Estado_Manuel = "Libre";
            Fin_Tomas = 0;
            Fin_Alicia = 0;
            Fin_Lucia = 0;
            Fin_Maria = 0;
            Fin_Manuel = 0;

            Cola_Matricula = 0;
            Cola_Renovacion = 0;

            dgv_Se_Produce_Atentado.Rows.Clear();

            Runge_Kutta_SeProduceElAtentado();
            //t_Anterior = t_Anterior * 9;

            Hora_Fin_Atentado = null;

            cargarGrilla();
        }

        



        public void Calcular_Tiempo_Entre_Llegada()
        {
            Random_Matricula = rnd.NextDouble();
            Hora_Llegada_Matricula = - Matricula_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Matricula);
            Random_Renovacion = rnd.NextDouble();
            Hora_Llegada_Renovacion = - Renovacion_LlegadaCliente_Exponencial_Media * Math.Log(1 - Random_Renovacion);

            Tiempo_Entre_Llegada = Hora_Llegada_Matricula;
            Proxima_Llegada_Matricula = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Matricula", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No, Esta_en_Bloqueo_COla = Cliente.Es_ultimo.No });

            Tiempo_Entre_Llegada = Hora_Llegada_Renovacion;
            Proxima_Llegada_Renovacion = (double)(Tiempo_Entre_Llegada + Reloj);
            contadorCliente++;
            listCliente.Add(new Cliente { numero = contadorCliente, estado = Cliente.Estado.Esperando_Atencion, Servidor = "", Tipo_Cliente = "Renovacion", TiempoEspera = 0, esta_en_cola = Cliente.EstaEnCola.No , Esta_en_Bloqueo_COla = Cliente.Es_ultimo.No });

        }

        


        public void CargarGrillaCliente()
        {
            dgv_Clientes.Rows.Add();
        }

        private void dgv_ClaseCLiente_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgv_ClaseCLiente.CurrentRow.DefaultCellStyle.BackColor = Color.Red;
            
        }

        private void dgv_ClaseCLiente_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
            dgv_ClaseCLiente.CurrentRow.DefaultCellStyle.BackColor = Color.White;
            
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            ListServidor.Add(new Servidor { nombre = Servidor.Nombre.Tomas, estado = Servidor.Estado.Libre, Fin_Tiempo_Atencion = null });
            ListServidor.Add(new Servidor { nombre = Servidor.Nombre.Alicia, estado = Servidor.Estado.Libre, Fin_Tiempo_Atencion = null });
            ListServidor.Add(new Servidor { nombre = Servidor.Nombre.Lucia, estado = Servidor.Estado.Libre, Fin_Tiempo_Atencion = null });
            ListServidor.Add(new Servidor { nombre = Servidor.Nombre.Maria, estado = Servidor.Estado.Libre, Fin_Tiempo_Atencion = null });
            ListServidor.Add(new Servidor { nombre = Servidor.Nombre.Manuel, estado = Servidor.Estado.Libre, Fin_Tiempo_Atencion = null });

            Variable_3 = true;
        }

        private void pintarGrilla() 
        {
            dgv_ClaseCLiente.Columns["Column1"].Frozen = true;
            dgv_ClaseCLiente.Columns["Column2"].Frozen = true;

            dgv_ClaseCLiente.Columns["Column1"].DefaultCellStyle.BackColor = Color.LightSalmon;
            dgv_ClaseCLiente.Columns["Column2"].DefaultCellStyle.BackColor = Color.LightSalmon;

            dgv_ClaseCLiente.Columns["Column3"].DefaultCellStyle.BackColor = Color.LightBlue;
            dgv_ClaseCLiente.Columns["Column4"].DefaultCellStyle.BackColor = Color.LightBlue;
            dgv_ClaseCLiente.Columns["Column27"].DefaultCellStyle.BackColor = Color.LightBlue;
            dgv_ClaseCLiente.Columns["Column5"].DefaultCellStyle.BackColor = Color.LightBlue;
            dgv_ClaseCLiente.Columns["Column6"].DefaultCellStyle.BackColor = Color.LightBlue;
            dgv_ClaseCLiente.Columns["Column7"].DefaultCellStyle.BackColor = Color.LightBlue;

            dgv_ClaseCLiente.Columns["Column8"].DefaultCellStyle.BackColor = Color.LightPink;
            dgv_ClaseCLiente.Columns["Column9"].DefaultCellStyle.BackColor = Color.LightPink;
            dgv_ClaseCLiente.Columns["Column10"].DefaultCellStyle.BackColor = Color.LightPink;
            dgv_ClaseCLiente.Columns["Column11"].DefaultCellStyle.BackColor = Color.LightPink;
            dgv_ClaseCLiente.Columns["Column12"].DefaultCellStyle.BackColor = Color.LightPink;
            dgv_ClaseCLiente.Columns["Column13"].DefaultCellStyle.BackColor = Color.LightPink;

            dgv_ClaseCLiente.Columns["Column14"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column15"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column16"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column17"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column18"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column19"].DefaultCellStyle.BackColor = Color.LightGreen;
            dgv_ClaseCLiente.Columns["Column20"].DefaultCellStyle.BackColor = Color.LightGreen;


            dgv_ClaseCLiente.Columns["Column21"].DefaultCellStyle.BackColor = Color.LightGray;
            dgv_ClaseCLiente.Columns["Column22"].DefaultCellStyle.BackColor = Color.LightGray;
            dgv_ClaseCLiente.Columns["Column23"].DefaultCellStyle.BackColor = Color.LightGray;
            dgv_ClaseCLiente.Columns["Column24"].DefaultCellStyle.BackColor = Color.LightGray;
            dgv_ClaseCLiente.Columns["Column25"].DefaultCellStyle.BackColor = Color.LightGray;
            dgv_ClaseCLiente.Columns["Column26"].DefaultCellStyle.BackColor = Color.LightGray;

        }



        public void Runge_Kutta_SeProduceElAtentado() //DA / dt = β * A   t=1==9min
        {
            beta = rnd.NextDouble();     //Es uniforme entre 0 y 1 entonces seria igual al Ramdon generado
            t = 0;
            k1 = 0;
            k2 = 0;
            k3 = 0;
            k4 = 0;
            A = 136.0925;

            while (A < 272.185)
            {
                t_Anterior = t;
                A_Anterior = A;
                
                k1 = beta * A;
                k2 = beta * (A + ((h / 2) * k1));
                k3 = beta * (A + ((h / 2) * k2));
                k4 = beta * (A + (h * k3));

                
                A = A + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                t = t + h;

                ListAtaqueServidor.Add(new RungeKutta_Ataque_Servidor { to = t_Anterior, Ao = (double)A_Anterior, K1 = (double)k1, K2 = (double)k2, K3 = (double)k3, K4 = (double)k4, ti_mas_1 = t,Ai_mas_1 = (double)A });

                //Cargar_Grilla_Proximo_Atentado();
            }
            
            t_Anterior = t;
            A_Anterior = A;

            k1 = beta * A;
            k2 = beta * (A + ((h / 2) * k1));
            k3 = beta * (A + ((h / 2) * k2));
            k4 = beta * (A + (h * k3));


            A = A + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
            t = t + h;

            ListAtaqueServidor.Add(new RungeKutta_Ataque_Servidor { to = t_Anterior, Ao = (double)A_Anterior, K1 = (double)k1, K2 = (double)k2, K3 = (double)k3, K4 = (double)k4, ti_mas_1 = t, Ai_mas_1 = (double)A });


            //Cargar_Grilla_Proximo_Atentado();

            t_Anterior = (t_Anterior * 9) + (double)Reloj;
            Proximo_Atentado = t_Anterior;
        }


        public void Runge_Kutta_Duracion_Bloqueo_Cola() //DL / dt = - ((L /0.8)*t2)) - L    t=1==5min
        {
            //h = 0.1;
            L = Reloj;
            t = 0;
            k1 = 0;
            k2 = 0;
            k3 = 0;
            k4 = 0;

            L_Anterior = 1000;

            while (L_Anterior - L > 1)
            {
                t_Anterior = t;
                L_Anterior = L;

                k1 = -((L /0.8) * Math.Pow(t,2)) - L;
                k2 = -(((L+((h/2)*k1)) / 0.8) * Math.Pow(t, 2)) - (L + ((h / 2) * k1));
                k3 = -(((L + ((h / 2) * k2)) / 0.8) * Math.Pow(t, 2)) - (L + ((h / 2) * k2));
                k4 = -(((L + (h* k3)) / 0.8) * Math.Pow(t, 2)) - (L + (h  * k3));
  
                L = L + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                t = t + h;

                ListServidorBloqueado.Add(new RungeKutta_Servidor_Bloqueado { to = t_Anterior, So = (double)L_Anterior, K1 = (double)k1, K2 = (double)k2, K3 = (double)k3, K4 = (double)k4, ti_mas_1 = t, Si_mas_1 = (double)L });

            }

            //t_Anterior = t;
            //L_Anterior = L;

            //k1 = -((L / 0.8) * Math.Pow(t, 2)) - L;
            //k2 = -(((L + ((h / 2) * k1)) / 0.8) * Math.Pow(t, 2)) - (L + ((h / 2) * k1));
            //k3 = -(((L + ((h / 2) * k2)) / 0.8) * Math.Pow(t, 2)) - (L + ((h / 2) * k2));
            //k4 = -((L + (h * k3) / 0.8) * Math.Pow(t, 2)) - (L + (h * k3));

            //L = L + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
            //t = t + h;
            //Cargar_Grilla_Atentado_Actual(t_Anterior, (double)L_Anterior, (double)k1, (double)k2, (double)k3, (double)k4, t, (double)L);
            t_Anterior = (t_Anterior * Bloqueo_Cola_Cant_Min) + (double)Reloj;
            //MessageBox.Show(t_Anterior.ToString());
        }


        public void Runge_Kutta_Ataque_Servidor() //DS / dt = (0.2 * S) + 3 - t      t=1==2min
        {
            S = Reloj;
            S_Duracion = S * 1.35;
            t = 0;
            k1 = 0;
            k2 = 0;
            k3 = 0;
            k4 = 0;

            while (S < S_Duracion)
            {
                t_Anterior = t;
                S_anterior = S;


                k1 = (0.2 * S) + 3 - t;
                k2 = (0.2 * (S + ((h / 2) * k1))) + 3 - t;
                k3 = (0.2 * (S + ((h / 2) * k2))) + 3 - t;
                k4 = (0.2 * (S + (h * k3))) + 3 - t;


                S = S + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                t = t + h;

                ListServidorBloqueado.Add(new RungeKutta_Servidor_Bloqueado { to = t_Anterior, So = (double)S_anterior, K1 = (double)k1, K2 = (double)k2, K3 = (double)k3, K4 = (double)k4, ti_mas_1 = t, Si_mas_1 = (double)S });

                //Cargar_Grilla_Atentado_Actual(t_Anterior,(double)S_anterior, (double)k1, (double)k2, (double)k3, (double)k4,t, (double)S);
            }

            t_Anterior = t;
            S_anterior = S;


            k1 = (0.2 * S) + 3 - t;
            k2 = (0.2 * (S + ((h / 2) * k1))) + 3 - t;
            k3 = (0.2 * (S + ((h / 2) * k2))) + 3 - t;
            k4 = (0.2 * (S + (h * k3))) + 3 - t;


            S = S + ((h / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
            t = t + h;

            ListServidorBloqueado.Add(new RungeKutta_Servidor_Bloqueado { to = t_Anterior, So = (double)S_anterior, K1 = (double)k1, K2 = (double)k2, K3 = (double)k3, K4 = (double)k4, ti_mas_1 = t, Si_mas_1 = (double)S });

            //Cargar_Grilla_Atentado_Actual(t_Anterior, (double)S_anterior, (double)k1, (double)k2, (double)k3, (double)k4, t, (double)S);
            t_Anterior = (t_Anterior * Bloqueo_Servidor_Cant_Min) + (double)Reloj;
        }


        public void Cargar_Grilla_Proximo_Atentado()
        {
            dgv_Se_Produce_Atentado.Rows.Add(Math.Round(t_Anterior,4), Math.Round((double)A_Anterior,4), 
                Math.Round((double)k1,4), Math.Round((double)k2,4), Math.Round((double)k3,4), Math.Round((double)k4,4), Math.Round(t,4), Math.Round((double)A,4));
        }

        public void Cargar_Grilla_Atentado_Actual(double tiempo_anterior, double Variable_A_Usar_Anterior, double K1, double K2,double K3,double K4,double tiempo_Actual, double Variable_Actual)
        {
            dgv_Atentado_Actual.Rows.Add(Math.Round(tiempo_anterior, 4), Math.Round(Variable_A_Usar_Anterior, 4),
                Math.Round(K1, 4), Math.Round(K2, 4), Math.Round(K3, 4), Math.Round(K4, 4), Math.Round(tiempo_Actual, 4), Math.Round(Variable_Actual, 4));
        }


        private void botonSIMULAR_Click(object sender, EventArgs e)
        {

            PARAMETROS();
            
            
            if (txt_simulacion.Text != "" && txt_Desde.Text != "")
            {
                desde = Convert.ToInt32(txt_Desde.Text);
                simulaciones = Convert.ToInt32(txt_simulacion.Text);
                
                Variable_3 = true;

                Simulacion_Cero();

                if (desde < simulaciones)
                {
                    txt_Hasta.Text = (desde + 400).ToString();
                    hasta = (desde + 400);

                    for (int i = 0; i < desde; i++)
                    {
                        Comenzar();
                    }
                    cargarGrilla();


                    if ( hasta > simulaciones)
                    {
                        for (int i = 0; i < (simulaciones - desde); i++)
                        {
                            Comenzar();
                            cargarGrilla();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 400; i++)
                        {
                            Comenzar();
                            cargarGrilla();
                        }

                        for (int i = 0; i < (simulaciones - hasta - 1); i++)
                        {
                            Comenzar();
                        }

                        if (hasta != simulaciones)
                        {
                            Comenzar();
                            cargarGrilla();
                        }

                    }

                    if (Variable_3 == true)
                    {
                        foreach (var item in listCliente)
                        {
                            dgv_Clientes.Rows.Add(item.numero, item.estado, item.Servidor, item.Tipo_Cliente, item.TiempoEspera, item.esta_en_cola);
                        }
                    }
                    pintarGrilla();


                    foreach (var item in ListAtaqueServidor)
                    {
                        dgv_Se_Produce_Atentado.Rows.Add(Math.Round(item.to,4), Math.Round(item.Ao,4), Math.Round(item.K1,4), Math.Round(item.K2,4), Math.Round(item.K3,4), Math.Round(item.K4,4), Math.Round(item.ti_mas_1,4), Math.Round(item.Ai_mas_1,4));
                    }
                    //////////////////////////////////////
                    //foreach (var item in ListBloqueoCola)
                    //{
                    //    dgv_Atentado_Actual.Rows.Add(item.to, item.Lo, item.K1, item.K2, item.K3, item.K4, item.ti_mas_1, item.Li_mas_1);
                    //}

                    foreach (var item in ListServidorBloqueado)
                    {
                        dgv_Atentado_Actual.Rows.Add(Math.Round(item.to,4), Math.Round(item.So,4), Math.Round(item.K1,4), Math.Round(item.K2,4), Math.Round(item.K3,4), Math.Round(item.K4,4), Math.Round(item.ti_mas_1,4), Math.Round(item.Si_mas_1,4));
                    }

                }
                else
                {
                    MessageBox.Show("Por Favor seleccione un DESDE menor a la cantidad de simulaciones");
                }




                //for (int i = 0; i < simulaciones; i++)
                //{
                //    Comenzar();
                //    cargarGrilla();
                //}

                //if (Variable_3 == true)
                //{ 
                //    foreach (var item in listCliente)
                //    {
                //        dgv_Clientes.Rows.Add(item.numero, item.estado, item.Servidor, item.Tipo_Cliente, item.TiempoEspera, item.esta_en_cola);
                //    }
                //}
                //pintarGrilla();

            }
            else
            {
                MessageBox.Show("Coloque valores en la celda Simular y en la celda Desde");
            }

        }



        public void PARAMETROS()
        {
            if (txt_Matricula_LlegadaCliente_Exp_Media.Text != "")
            {
                Matricula_LlegadaCliente_Exponencial_Media = Convert.ToInt32(txt_Matricula_LlegadaCliente_Exp_Media.Text);
            }

            if (txt_Renovacion_LlegadaCliente_Exp_Media.Text != "")
            {
                Renovacion_LlegadaCliente_Exponencial_Media = Convert.ToInt32(txt_Renovacion_LlegadaCliente_Exp_Media.Text);
            }


            if (txt_Matricula_TiempoAtencion_Uniforme_Minimo.Text != "" && txt_Matricula_TiempoAtencion_Uniforme_Maximo.Text != "")
            {
                Matricula_TiempoAtencion_Uniforme_Minimo = Convert.ToInt32(txt_Matricula_TiempoAtencion_Uniforme_Minimo.Text);
                Matricula_TiempoAtencion_Uniforme_MAximo = Convert.ToInt32(txt_Matricula_TiempoAtencion_Uniforme_Maximo.Text);
            }
            if (txt_Renovacion_TiempoAtencion_Normal_Media.Text != "" && txt_Renovacion_TiempoAtencion_Normal_Desviacion.Text != "")
            {
                Renovacion_TiempoAtencion_Normal_Media = Convert.ToInt32(txt_Renovacion_TiempoAtencion_Normal_Media.Text);
                Renovacion_TiempoAtencion_Normal_Desviacion = Convert.ToInt32(txt_Renovacion_TiempoAtencion_Normal_Desviacion.Text);
            }

            //if (txt_h_Paso.Text != "")
            //{
            //    h = Convert.ToInt32(txt_h_Paso.Text);
            //}

            if (txt_Cantidad_Minutos_Bloqueo_Cola.Text != "")
            {
                Bloqueo_Cola_Cant_Min = Convert.ToInt32(txt_Cantidad_Minutos_Bloqueo_Cola.Text);
            }

            if (txt_Cantidad_Minutos_Bloqueo_Servidor.Text != "")
            {
                Bloqueo_Servidor_Cant_Min = Convert.ToInt32(txt_Cantidad_Minutos_Bloqueo_Servidor.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Reloj = 70;
            Runge_Kutta_Duracion_Bloqueo_Cola();

            foreach (var item in ListServidorBloqueado)
            {
                dgv_Atentado_Actual.Rows.Add(Math.Round(item.to, 4), Math.Round(item.So, 4), Math.Round(item.K1, 4), Math.Round(item.K2, 4), Math.Round(item.K3, 4), Math.Round(item.K4, 4), Math.Round(item.ti_mas_1, 4), Math.Round(item.Si_mas_1, 4));
            }
        }
    }
}
