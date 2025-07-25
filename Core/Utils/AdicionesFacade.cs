using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Bll;
using Core.BLL;
using Core.Models;

namespace Core.Utils
{
    public class AdicionesFacade
    {
        private readonly AdicionPedidoServicioBLL _adicionPedidoServicioBll;
        private readonly SuspensionBLL _suspensionBll;
        private readonly ReanudacionBLL _reanudacionBll;
        private readonly OrdenCompraBLL _ordenCompraBll;

        public AdicionesFacade(AdicionPedidoServicioBLL adicionPedidoServicioBll, SuspensionBLL suspensionBll, ReanudacionBLL reanudacionBll, OrdenCompraBLL ordenCompraBll)
        {
            _adicionPedidoServicioBll = adicionPedidoServicioBll;
            _suspensionBll = suspensionBll;
            _reanudacionBll = reanudacionBll;
            _ordenCompraBll = ordenCompraBll;
        }

        public async Task<IPolizas> GetById(Guid id, string tipoModificacion)
        {
            switch (tipoModificacion)
            {
                case EnumTipoAdicion.Adicion:

                    return (await this._adicionPedidoServicioBll.GetById(id));
                    break;
                case EnumTipoAdicion.Reanudacion:
                    return (await this._reanudacionBll.GetById(id));

                    break;
                case EnumTipoAdicion.Suspension:
                    return (await this._suspensionBll.GetById(id));

                    break;

            }

            return null;
        }

        public async Task<string> UpdatePolizas(Guid id, Guid polizaId, string tipoModificacion, string uploadId)
        {

            var result = string.Empty;
            Poliza poliza;
            switch (tipoModificacion)
            {
                case EnumTipoAdicion.Adicion:
                    var adicion = (await this._adicionPedidoServicioBll.GetById(id));
                    poliza = adicion.poliza.FirstOrDefault(x => x.id == polizaId);
                    result = SetFileId(poliza, uploadId);
                    await this._adicionPedidoServicioBll.Update(adicion);

                    break;
                case EnumTipoAdicion.Reanudacion:
                    var reanudacion = (await this._reanudacionBll.GetById(id));
                    poliza = reanudacion.poliza.FirstOrDefault(x => x.id == polizaId);
                    result = SetFileId(poliza, uploadId);
                    await this._reanudacionBll.Update(reanudacion);

                    break;
                case EnumTipoAdicion.Suspension:
                    var suspension = (await this._suspensionBll.GetById(id));
                    poliza = suspension.poliza.FirstOrDefault(x => x.id == polizaId);
                    result = SetFileId(poliza, uploadId);
                    await this._suspensionBll.Update(suspension);

                    break;
            }


            if (result.Length > 0)
            {
                return result;
            }
            return "Upload Successful.";
        }



        public async Task<string> UpdateContrato(Guid id, string tipoModificacion, string uploadId)
        {

            var result = string.Empty;
            Poliza poliza;
            switch (tipoModificacion)
            {
                case EnumTipoAdicion.Adicion:
                    var adicion = (await this._adicionPedidoServicioBll.GetById(id));
                    adicion.archivoContrato = uploadId;
                    await this._adicionPedidoServicioBll.Update(adicion);

                    break;
                case EnumTipoAdicion.Reanudacion:
                    var reanudacion = (await this._reanudacionBll.GetById(id));
                    reanudacion.archivoContrato = uploadId;
                    await this._reanudacionBll.Update(reanudacion);

                    break;
                case EnumTipoAdicion.Suspension:
                    var suspension = (await this._suspensionBll.GetById(id));
                    suspension.archivoContrato  = uploadId;
                    await this._suspensionBll.Update(suspension);

                    break;
            }


            if (result.Length > 0)
            {
                return result;
            }
            return "Upload Successful.";
        }


        public async Task<string> UpdateContratoFirmado(Guid id, string tipoModificacion, string uploadId)
        {
            Guid idOrden = Guid.Empty;
            var result = string.Empty;
            Poliza poliza;
            switch (tipoModificacion)
            {
                case EnumTipoAdicion.Adicion:
                    var adicion = (await this._adicionPedidoServicioBll.GetById(id));

                    idOrden = adicion.idOrden;
                    adicion.archivoContratoFirmado = uploadId;
                    await this._adicionPedidoServicioBll.Update(adicion);

                    break;
                case EnumTipoAdicion.Reanudacion:
                    var reanudacion = (await this._reanudacionBll.GetById(id));

                    idOrden = reanudacion.idOrden;

                    reanudacion.archivoContratoFirmado = uploadId;
                    await this._reanudacionBll.Update(reanudacion);

                    break;
                case EnumTipoAdicion.Suspension:
                    var suspension = (await this._suspensionBll.GetById(id));

                    idOrden = suspension.idOrden;

                    suspension.archivoContratoFirmado = uploadId;
                    await this._suspensionBll.Update(suspension);

                    break;
            }


            await this._ordenCompraBll.NoPendiente(idOrden);

            if (result.Length > 0)
            {
                return result;
            }
            return "Upload Successful.";
        }


        public string SetFileId(Poliza poliza, string uploadId)
        {


            if (poliza != null)
            {
                poliza.archivo = uploadId;

                return string.Empty;
            }
            else
            {

                return "Upload Failed: Poliza no encontrada";
            }
        }
    }
}
