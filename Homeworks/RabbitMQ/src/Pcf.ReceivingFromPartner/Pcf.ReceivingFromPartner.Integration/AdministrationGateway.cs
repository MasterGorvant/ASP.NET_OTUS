using System;
using System.Net.Http;
using System.Threading.Tasks;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;

namespace Pcf.ReceivingFromPartner.Integration
{
    public class AdministrationGateway
        : IAdministrationGateway
    {
        private readonly NotificationService _notificationService;
        public AdministrationGateway(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task NotifyAdminAboutPartnerManagerPromoCode(Guid partnerManagerId)
        {
            await _notificationService.Notify(partnerManagerId, "PartnerManagerPromoCode");
        }
    }
}