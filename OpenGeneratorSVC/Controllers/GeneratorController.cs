using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenGeneratorSVC.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        [HttpPost(Name = "ConsultarBases")]
        public DTO.clsRespuesta ConsultarBases()
        {
            DTO.clsRespuesta resp = new DTO.clsRespuesta();
            BLL.bllGenerator bll = new BLL.bllGenerator();
            DTO.clsResultado resultado = null;
            resp.Datos = bll.fnConsultarBases(out resultado);
            resp.Resultado = resultado;
            return resp;
        }

        [HttpPost(Name = "ConsultarTablas")]
        public DTO.clsRespuesta ConsultarTablas(DTO.clsParametros parametros)
        {
            DTO.clsRespuesta resp = new DTO.clsRespuesta();
            BLL.bllGenerator bll = new BLL.bllGenerator();
            DTO.clsResultado resultado = null;
            resp.Datos = bll.fnConsultarTablas(parametros.BaseDeDatos, out resultado);
            resp.Resultado = resultado;
            return resp;
        }

        [HttpPost(Name = "ConsultarPlantillas")]
        public DTO.clsRespuesta ConsultarPlantillas()
        {
            DTO.clsRespuesta resp = new DTO.clsRespuesta();
            BLL.bllGenerator bll = new BLL.bllGenerator();
            DTO.clsResultado resultado = null;
            resp.Datos = bll.fnConsultarPlantillas(out resultado);
            resp.Resultado = resultado;
            return resp;
        }

        [HttpPost(Name = "Generar")]
        public DTO.clsRespuesta Generar(DTO.clsParametros parametros)
        {
            DTO.clsRespuesta resp = new DTO.clsRespuesta();
            BLL.bllGenerator bll = new BLL.bllGenerator();
            DTO.clsResultado resultado = null;
            resp.Datos = bll.fnGenerar(parametros, out resultado);
            resp.Resultado = resultado;
            return resp;
        }

    }
}