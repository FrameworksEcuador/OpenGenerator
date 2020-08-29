import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class GlobalesService {
  public TransaccionesPendientes: number = 0;     // Número de transacciones pendientes enviadas por http. Para ngx-loading
  public Titulo: string;          // Título de la Pantalla que se presenta
  public Mensaje: string;         // Los mensajes que se presentan en todas las pantallas
  public AlertaOk: boolean;       // La alerta es OK. True=ok, False=error
  public Login: String;           // El login del usuario que ha ingresado
  public Perfil: String = 'ADM';  // El Perfil del usuario
  public Grupo: String = '';      // El grupo al que pertenece el usuario, el admin queda en blanco
  public GrupoNombre: String = '';// El nombre del grupo al que pertenece el usuario, el admin queda en blanco
  public Nombres: String = '';
  public Apellidos: String = '';
  public EsAdministrador: String = 'N';   // S=Si es Administrador, N=Usuario

  constructor(private messageService: MessageService) {
    this.Mensaje = '';
    this.Titulo = '';
  }

  public fnMensaje(msj: string, ok: boolean = true): void {
    //var elementos = msj.split(':');
    this.AlertaOk = ok;
    this.Mensaje = msj;
    if (ok) {
      this.messageService.add({ severity: 'success', summary: '', detail: msj });
    } else {
      this.messageService.add({ severity: 'error', summary: '', detail: msj, life: 10000 });
    }
  }

  public fnMensajeLimpiar(): void {
    this.messageService.clear();
    this.Mensaje = '';
  }
}
