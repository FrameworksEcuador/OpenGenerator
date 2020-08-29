import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpResponse } from '@angular/common/http';
import { GlobalesService } from './globales.service';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable()
export class FactoryService {
  public headers: HttpHeaders;
  public headersFiles: HttpHeaders;
  public url: string = '';
  public url_completa: string = '';
  private static token: string;

  constructor(private http: HttpClient, public globales: GlobalesService) {
    this.headers = new HttpHeaders();
    this.headersFiles = new HttpHeaders();

    this.headers.append('Content-Type', 'application/json');
    this.headers.append('Accept', 'application/json');
    this.headers.append('Access-Control-Allow-Origin', '*');
    this.headers.append('Access-Control-Allow-Methods', 'GET, HEAD, POST, PUT, DELETE, TRACE, OPTIONS');
    this.headers.append('Access-Control-Allow-Headers', 'Origin, x-auth-token, Authorization, X-Requested-With, Content-Type, Accept, Cache-Control');

    this.headersFiles.append('Content-Type', 'multipart/form-data');

    if (!environment.production) {
      this.url = 'http://localhost/OpenGeneratorSVC/';           // Server local 
      //this.url = 'https://localhost:44380/';         // Ambiente de desarrollo local
    }
    else {
      this.url = 'http://localhost/OpenGeneratorSVC/';           // Server local 
      //this.url = 'http://portalzoom.cedia.edu.ec/MediaCollectorApi/';           // Server de CEDIA con puerto 80
      //this.url = 'http://portalzoom.cedia.edu.ec:5000/';           // Server de CEDIA con puerto 5000 -- si funciona
      //this.url = 'https://186.4.141.215:8443/MediaCollectorApi/';   // Dirección externa de red. Reemplazar por la dirección de los web services
    }
    //console.log("URL: " + this.url);                // Activar al inicio para identificar la URL a donde se está llamando
  }

  Ejecutar(comando: string, registro: any): Promise<any> {
    if (!environment.production && (FactoryService.token == null || FactoryService.token == undefined)) {
      FactoryService.token = localStorage.getItem("token");
      if (FactoryService.token == null || FactoryService.token == undefined)
        FactoryService.token = 'FRAMEWORKS-GENERATOR-QUITO-20200824';
      if (this.globales.Login == undefined || this.globales.Login == null || this.globales.Login.length <= 0)
        this.globales.Login = localStorage.getItem("login");
      if (this.globales.Perfil == undefined || this.globales.Perfil == null || this.globales.Perfil.length <= 0)
        this.globales.Perfil = localStorage.getItem("perfil");
    }
    registro.Token = FactoryService.token;
    if (!environment.production) {
      console.log("snd: " + this.url + comando + JSON.stringify(registro));
    }

    setTimeout(() => this.globales.TransaccionesPendientes += 1);           // setTimeout Para evitar el error: ExpressionChangedAfterItHasBeenCheckedError
    return this.http.post(this.url + comando, registro, { headers: this.headers })
      .toPromise()
      .then((response: HttpResponse<any>) => {
        setTimeout(() => this.globales.TransaccionesPendientes -= 1);
        if (!environment.production) {
          console.log("rec: " + JSON.stringify(response));
        }
        let respuesta: any = response;
        if (respuesta != null) {
          if (respuesta.resultado != null && respuesta.resultado.token != null) {
            FactoryService.token = respuesta.resultado.token;
            if (!environment.production && FactoryService.token && FactoryService.token.length > 0) {
              localStorage.setItem("token", FactoryService.token);
            }
          } else if (respuesta.datos != null && respuesta.datos.Token != null) {
            FactoryService.token = respuesta.datos.Token;
            if (!environment.production && FactoryService.token && FactoryService.token.length > 0) {
              localStorage.setItem("token", FactoryService.token);
            }
          }
        }
        return respuesta || {};
      })
      .catch((error: any): Promise<any> => {
        //this.globales.TransaccionesPendientes -= 1;
        setTimeout(() => this.globales.TransaccionesPendientes -= 1);
        return Promise.reject(error.message || error);
      });
  }

  Subir(comando: string, fData: FormData): Promise<any> {
    if (!environment.production && (FactoryService.token == null || FactoryService.token == undefined)) {
      //FactoryService.token = 'FRAMEWORKS-EPMAPS-QUITO-MARSED-20180910'; // localStorage.getItem("token");
      FactoryService.token = localStorage.getItem("token");
      if (FactoryService.token == null || FactoryService.token == undefined)
        FactoryService.token = 'FRAMEWORKS-GENERATOR-QUITO-20200824';
      if (this.globales.Login == undefined || this.globales.Login == null || this.globales.Login.length <= 0)
        this.globales.Login = localStorage.getItem("login");
      if (this.globales.Perfil == undefined || this.globales.Perfil == null || this.globales.Perfil.length <= 0)
        this.globales.Perfil = localStorage.getItem("perfil");
    }
    fData.append('token', FactoryService.token);
    if (!environment.production) {
      console.log("snd: " + this.url + comando + JSON.stringify(fData));
    }

    setTimeout(() => this.globales.TransaccionesPendientes += 1);
    return this.http.post(this.url + comando, fData, { headers: this.headersFiles }).toPromise().then(
      (response: Response) => {
        setTimeout(() => this.globales.TransaccionesPendientes -= 1);
        if (!environment.production) {
          console.log("rec: " + JSON.stringify(response));
        }
        let respuesta: any = response;
        if (respuesta != null) {
          if (respuesta.resultado != null && respuesta.resultado.token != null) {
            FactoryService.token = respuesta.resultado.token;
            if (!environment.production && FactoryService.token && FactoryService.token.length > 0) {
              localStorage.setItem("token", FactoryService.token);
            }
          } else if (respuesta.datos != null && respuesta.datos.Token != null) {
            FactoryService.token = respuesta.datos.Token;
            if (!environment.production && FactoryService.token && FactoryService.token.length > 0) {
              localStorage.setItem("token", FactoryService.token);
            }
          }
        }
        return respuesta || {};
      })
      .catch((error: any): Promise<any> => {
        this.globales.TransaccionesPendientes -= 1;
        return Promise.reject(error.message || error);
      });
  }

  public Descargar(dto: any): Observable<Blob> {
    setTimeout(() => this.globales.TransaccionesPendientes += 1);
    return this.http.post<Blob>(this.url + 'reportes/GenerarReporteExcel', dto, { responseType: 'blob' as 'json' });
  }

}
