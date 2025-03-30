using ChieuT4_Nhom05_WebQLCF.Models.Momo;
using ChieuT4_Nhom05_WebQLCF.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChieuT4_Nhom05_WebQLCF.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
