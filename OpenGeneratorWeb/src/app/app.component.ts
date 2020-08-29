import { Component, OnInit } from '@angular/core';
import { GlobalesService } from './servicios/globales.service';
import { FactoryService } from './servicios/factory.service';
import { clsParametros } from './cls/clsParametros';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  public autor: string = "";
  public aplicacion: string = "";
  public namespace: string = "";
  public bases: string[] = [];
  public base: string;
  public tablas: string[] = [];
  public tabla: string;
  public parametros: clsParametros = new clsParametros();
  public plantillas: string[] = [];
  public plantilla: string;
  public codigoFuente: string;

  constructor(private factory: FactoryService, private globales: GlobalesService) { }

  ngOnInit(): void {
    this.fnLimpiar();

    let vAutor = localStorage.getItem("autor");
    if (vAutor != null && vAutor.length > 0)
      this.autor = vAutor;

    let vAplicacion = localStorage.getItem("aplicacion");
    if (vAplicacion != null && vAplicacion.length > 0)
      this.aplicacion = vAplicacion;

    let vNamespace = localStorage.getItem("namespace");
    if (vNamespace != null && vNamespace.length > 0)
      this.namespace = vNamespace;

    this.fnConsultarBases();
    this.fnConsultarPlantillas();
  }

  fnLimpiar() {
    this.bases = [];
  }

  fnConsultarBases() {
    this.factory.Ejecutar("Generator/ConsultarBases", {})
      .then((resp: any) => {
        if (resp.resultado.resultado == 0) {
          this.bases = resp.datos;
          if (this.bases.length > 0) {
            let vBase = localStorage.getItem("base");
            this.base = (vBase != null && vBase.length > 0) ? vBase : this.bases[0];
            this.fnConsultarTablas();
          }
        } else {
          this.bases = [];
        }
      }).catch(resp => {
        this.globales.fnMensaje("ERROR." + resp.status + ": " + resp.statusText, false);
        this.bases = [];
      });

  }

  fnConsultarTablas() {
    this.parametros.BaseDeDatos = this.base;
    this.factory.Ejecutar("Generator/ConsultarTablas", this.parametros)
      .then((resp: any) => {
        if (resp.resultado.resultado == 0) {
          this.tablas = resp.datos;
          if (this.tablas.length > 0) {
            let vTabla = localStorage.getItem("tabla");
            this.tabla = (vTabla != null && vTabla.length > 0) ? vTabla : this.tablas[0];
          }
        } else {
          this.tablas = [];
        }
      }).catch(resp => {
        this.globales.fnMensaje("ERROR." + resp.status + ": " + resp.statusText, false);
        this.tablas = [];
      });

  }

  fnConsultarPlantillas() {
    this.factory.Ejecutar("Generator/ConsultarPlantillas", {})
      .then((resp: any) => {
        if (resp.resultado.resultado == 0) {
          this.plantillas = resp.datos;
          if (this.plantillas.length > 0) {
            let vPlantilla = localStorage.getItem("plantilla");
            this.plantilla = (vPlantilla != null && vPlantilla.length > 0) ? vPlantilla : this.plantillas[0];
          }
        } else {
          this.plantillas = [];
        }
      }).catch(resp => {
        this.globales.fnMensaje("ERROR." + resp.status + ": " + resp.statusText, false);
        this.plantillas = [];
      });
  }

  fnGenerar() {
    localStorage.setItem("autor", this.autor);
    localStorage.setItem("aplicacion", this.aplicacion);
    localStorage.setItem("namespace", this.namespace);
    localStorage.setItem("base", this.base);
    localStorage.setItem("tabla", this.tabla);
    localStorage.setItem("plantilla", this.plantilla);

    this.parametros.Autor = this.autor;
    this.parametros.Aplicacion = this.aplicacion;
    this.parametros.Namespace = this.namespace;
    this.parametros.BaseDeDatos = this.base;
    this.parametros.Tabla = this.tabla;
    this.parametros.Plantilla = this.plantilla;
    this.factory.Ejecutar("Generator/Generar", this.parametros)
      .then((resp: any) => {
        if (resp.resultado.resultado == 0) {
          this.codigoFuente = resp.datos;
        } else {
          this.codigoFuente = "";
        }
      }).catch(resp => {
        this.globales.fnMensaje("ERROR." + resp.status + ": " + resp.statusText, false);
        this.codigoFuente = "";
      });
  }

  fnCopiar(codigo) {
    codigo.select();
    document.execCommand('copy');
  }

  fnBorrar() {
    this.codigoFuente = "";
  }
}
